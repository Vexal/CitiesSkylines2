using Colossal;
using Colossal.Entities;
using Colossal.Mathematics;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Objects;
using Game.Pathfind;
using Game.Rendering;
using Game.Routes;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using UnityEngine.InputSystem;

namespace EmploymentTracker
{
	[BurstCompile]
	internal partial class HighlightRoutesSystem : UISystemBase
    {
        private Entity selectedEntity = default;
		private SelectionType selectionType;
		private SimpleOverlayRendererSystem overlayRenderSystem;
		private OverlayRenderSystem overlayRenderSystem2;
		private EmploymentTrackerSettings settings;
		private EntityQuery hasTargetQuery;
		private EntityQuery hasPathQuery;
		private ToolSystem toolSystem;

		private HighlightFeatures highlightFeatures = new HighlightFeatures();
		private RouteOptions routeHighlightOptions = new RouteOptions();
		private int threadBatchSize = 16;
		private bool[] activeLaneIndexes = new bool[1024];
		private DefaultToolSystem defaultToolSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.hasTargetQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Target>()
			},
				Any = new ComponentType[]
			{
				ComponentType.ReadOnly<Vehicle>(),
				ComponentType.ReadOnly<Creature>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>()
				}
			});

			this.hasPathQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<PathOwner>(),
				ComponentType.ReadOnly<PathElement>(),
			},
				Any = new ComponentType[]
			{
				ComponentType.ReadOnly<Vehicle>(),
				ComponentType.ReadOnly<Creature>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>()
				}
			});

			//Init UI and IO
			this.settings = Mod.INSTANCE.getSettings();
			this.initBindings();

			this.toolSystem = World.GetExistingSystemManaged<ToolSystem>();
			this.defaultToolSystem = World.GetExistingSystemManaged<DefaultToolSystem>();
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			this.enableBindings();

			this.overlayRenderSystem = World.GetExistingSystemManaged<SimpleOverlayRendererSystem>();
			this.overlayRenderSystem2 = World.GetExistingSystemManaged<OverlayRenderSystem>();

			this.highlightFeatures = new HighlightFeatures(settings);
			this.routeHighlightOptions = new RouteOptions(settings);

			this.settings.onSettingsApplied += this.applySettings;
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
			this.disableBindings();
			this.reset();
		}

		private bool toggled = true;
		private bool pathingToggled = true;
		private bool pathVolumeToggled = false;
		private bool defaultHasDebugSelect = false;
		private bool laneSetDirty = false;
		private int hoverLane = -1;
		private Bezier4x3 hoverCurve = default;
		private NativeHashSet<Entity> commutingEntities;
		private NativeList<Bezier4x3> targetLaneCurves = default;

		long frameCount = 0;

		protected override void OnUpdate()
		{
			var clock = new Stopwatch();
			clock.Start();
			base.OnUpdate();
			if (this.highlightFeatures.dirty)
			{
				this.reset();
				this.highlightFeatures.dirty = false;
			}

			if (this.togglePathVolumeDisplayAction.WasPressedThisFrame())
			{
				this.toggleRouteVolumeToolActive(!this.pathVolumeToggled);
			}
			if (this.toggleRenderTypeAction.WasPressedThisFrame())
			{
				this.useNewRenderer = !this.useNewRenderer;
			}

			Entity selected = this.getSelected();
			SelectionType newSelectionType = this.getEntityRouteType(selected);
			
			//only compute most highlighting when object selection changes (except for dynamic pathfinding)
			bool updatedSelection = (this.toggled || this.pathVolumeToggled) && (selected != this.selectedEntity || newSelectionType != this.selectionType);
			if (updatedSelection)
			{
				this.reset();
				this.selectedEntity = selected;
				this.selectionType = newSelectionType;

				if (this.debugActiveBinding.value)
				{
					this.selectionTypeBinding.Update(this.selectionType.ToString());
					this.bindings["Selected Entity"] = this.selectedEntity.Index.ToString() + "-" + this.selectedEntity.Version.ToString();
				}
			}

			//check if hot key disable/enable highlighting was pressed
			if (!this.checkFrameToggles())
			{
				this.endFrame(clock);
				return;
			}

			if (this.pathVolumeToggled)
			{
				if (updatedSelection)
				{
					this.resetActiveLanes();
				}
			}

			//only need to update building/target highlights when selection changes
			if (updatedSelection || this.laneSetDirty || (this.refreshTransitingEntitiesBinding.value && (++this.frameCount % 64 == 0)))
			{
				this.laneSetDirty = false;
				var searchTimer = new Stopwatch();
				searchTimer.Start();

				this.populateRouteEntities();

				searchTimer.Stop();

				if (this.debugActiveBinding.value)
				{
					this.trackedEntityCount.Update(this.commutingEntities.Count);
					this.bindings["Search Time (ms)"] = searchTimer.ElapsedMilliseconds.ToString();
				}				
			}

			if (this.commutingEntities.IsCreated)
			{
				this.doRouteJobs(this.selectionType == SelectionType.BUILDING && !this.routeHighlightOptions.incomingRoutesTransit);
			}

			this.endFrame(clock);
		}

		private void doRouteJobs(bool ignoreTransit)
		{
			if (this.commutingEntities.IsEmpty)
			{
				return;
			}

			CalculateRoutesJob calculateRoutesJob = new CalculateRoutesJob();
			calculateRoutesJob.input = new NativeList<Entity>(this.commutingEntities.Count, Allocator.TempJob);

			foreach (Entity entity in this.commutingEntities)
			{
				calculateRoutesJob.input.Add(entity);
			}

			calculateRoutesJob.storageInfoLookup = GetEntityStorageInfoLookup();
			calculateRoutesJob.pathOnwerLookup = GetComponentLookup<PathOwner>(true);
			calculateRoutesJob.curveLookup = GetComponentLookup<Curve>(true);
			calculateRoutesJob.ownerLookup = GetComponentLookup<Owner>(true);
			calculateRoutesJob.routeLaneLookup = GetComponentLookup<RouteLane>(true);
			calculateRoutesJob.waypointLookup = GetComponentLookup<Waypoint>(true);
			calculateRoutesJob.pedestrianLaneLookup = GetComponentLookup<PedestrianLane>(true);
			calculateRoutesJob.trackLaneLookup = GetComponentLookup<TrackLane>(true);
			calculateRoutesJob.secondaryLaneLookup = GetComponentLookup<SecondaryLane>(true);
			calculateRoutesJob.currentTransportLookup = GetComponentLookup<CurrentTransport>(true);
			calculateRoutesJob.currentVehicleLookup = GetComponentLookup<CurrentVehicle>(true);
			calculateRoutesJob.deletedLookup = GetComponentLookup<Deleted>(true);
			calculateRoutesJob.pathElementLookup = GetBufferLookup<PathElement>(true);
			calculateRoutesJob.routeSegmentLookup = GetBufferLookup<RouteSegment>(true);
			calculateRoutesJob.carNavigationLaneSegmentLookup = GetBufferLookup<CarNavigationLane>(true);
			calculateRoutesJob.incomingRoutesTransit = !ignoreTransit;

			NativeCounter totalCount = new NativeCounter(Allocator.TempJob);
			calculateRoutesJob.curveCounter = totalCount.ToConcurrent();

			int routeBatchSize = this.threadBatchSize;

			calculateRoutesJob.batchSize = routeBatchSize;

			int batchCount = (calculateRoutesJob.input.Length / routeBatchSize) + 1;

			calculateRoutesJob.results = new NativeArray<NativeHashMap<CurveDef, int>>(batchCount, Allocator.TempJob);
			for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
			{
				calculateRoutesJob.results[batchIndex] = new NativeHashMap<CurveDef, int>(20000, Allocator.TempJob);
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			JobHandle routeJob = calculateRoutesJob.ScheduleBatch(calculateRoutesJob.input.Length, routeBatchSize);

			calculateRoutesJob.input.Dispose(routeJob);

			routeJob.Complete();
			stopwatch.Stop();
			var elapsed_time = stopwatch.ElapsedMilliseconds;

			stopwatch.Restart();

			//Weight segments with multiple entities passing over heavier
			NativeHashMap<CurveDef, int> resultCurves = MathUtil.mergeResultCurves(ref calculateRoutesJob.results,
				out int maxVehicleWeight, out int maxPedestrianWeight, out int maxTransitWeight, out int maxGenericWeight);


			stopwatch.Stop();
			var streamReadTime = stopwatch.ElapsedMilliseconds;
			if (this.debugActiveBinding.value)
			{
				this.totalSegmentCount.Update(totalCount.Count);
				this.uniqueSegmentCount.Update(resultCurves.Count);
				this.bindings["Route Time (ms)"] = elapsed_time.ToString();
			}

			totalCount.Dispose();

			for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
			{
				calculateRoutesJob.results[batchIndex].Dispose();
			}

			calculateRoutesJob.results.Dispose();

			stopwatch.Restart();
			if (resultCurves.Count > 0)
			{
				NativeArray<CurveDef> curveArray = new NativeArray<CurveDef>(resultCurves.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				NativeArray<int> curveCount = new NativeArray<int>(resultCurves.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

				int ind = 0;
				foreach (var curve in resultCurves)
				{
					curveArray[ind] = curve.Key;
					curveCount[ind] = curve.Value;
					++ind;
				}

				resultCurves.Dispose();
				JobHandle routeJobHandle;
				if (this.useNewRenderer)
				{
					RouteRenderJob job = new RouteRenderJob();
					job.curveDefs = curveArray;
					job.curveCounts = curveCount;
					job.maxVehicleCount = maxVehicleWeight;
					job.maxTransitCount = maxTransitWeight;
					job.maxGenericVehicleCount = maxGenericWeight;
					job.maxPedestrianCount = maxPedestrianWeight;
					job.selectedCurves = this.targetLaneCurves;
					job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
					job.routeHighlightOptions = this.routeHighlightOptions;
					if (this.hoverLane >= 0)
					{
						job.hoverCurve = this.hoverCurve;
						job.isHovering = true;
					}


					routeJobHandle = job.Schedule(dependencies);
					this.overlayRenderSystem.AddBufferWriter(routeJobHandle);
				}
				else
				{
					RouteRenderJobOld job = new RouteRenderJobOld();
					job.curveDefs = curveArray;
					job.curveCounts = curveCount;
					job.maxVehicleCount = maxVehicleWeight;
					job.maxTransitCount = maxTransitWeight;
					job.maxPedestrianCount = maxPedestrianWeight;
					job.overlayBuffer = this.overlayRenderSystem2.GetBuffer(out JobHandle dependencies);
					job.routeHighlightOptions = this.routeHighlightOptions;

					routeJobHandle = job.Schedule(dependencies);
					this.overlayRenderSystem2.AddBufferWriter(routeJobHandle);
				}

				curveArray.Dispose(routeJobHandle);
				curveCount.Dispose(routeJobHandle);
				//routeJobHandle.Complete();
			}

			stopwatch.Stop();

			if (this.debugActiveBinding.value)
			{
				var renderTime = stopwatch.ElapsedMilliseconds;
				this.bindings["Render Time (ms)" + (this.useNewRenderer ? " - n" : "")] = renderTime.ToString();
				this.bindings["Stream Time (ms)"] = streamReadTime.ToString();
			}
		}

		private Entity getSelected()
		{
			return this.toolSystem.selected;
		}

		private void populateRouteEntities()
		{
			if (this.commutingEntities.IsCreated)
			{
				this.commutingEntities.Dispose();
			}

			this.commutingEntities = new NativeHashSet<Entity>(128, Allocator.Persistent);

			if (this.pathVolumeToggled)
			{
				NativeHashSet<Entity> targets = this.getSelectedEntityTargets();
				

				if (this.debugActiveBinding.value)
				{
					this.bindings["Target Count"] = targets.Count.ToString();
				}

				if (targets.Count > 0)
				{
					var searchCounter = new Colossal.NativeCounter(Allocator.TempJob);
					var resultCounter = new Colossal.NativeCounter(Allocator.TempJob);

					EntityPathSearchJob searchJob = new EntityPathSearchJob();
					searchJob.targets = targets;

					searchJob.results = new NativeArray<Entity>(20000, Allocator.TempJob);
					searchJob.pathHandle = SystemAPI.GetBufferTypeHandle<PathElement>(true);
					searchJob.immediateCarLaneHandle = SystemAPI.GetBufferTypeHandle<CarNavigationLane>(true);
					searchJob.pathOwnerHandle = SystemAPI.GetComponentTypeHandle<PathOwner>(true);
					searchJob.entityHandle = SystemAPI.GetEntityTypeHandle();
					searchJob.searchCounter = searchCounter.ToConcurrent();
					searchJob.resultCounter = resultCounter.ToConcurrent();

					var jobHandle = JobChunkExtensions.ScheduleParallel(searchJob, this.hasPathQuery, default);

					targets.Dispose(jobHandle);

					jobHandle.Complete();

					if (this.debugActiveBinding.value)
					{
						this.bindings["Search Count"] = searchCounter.Count.ToString();
					}

					for (int i = 0; i < resultCounter.Count; ++i)
					{
						Entity e = searchJob.results[i];

						SelectionType entityRouteType = this.getEntityRouteType(e);
						if (entityRouteType != SelectionType.CAR_OCCUPANT)
						{
							this.commutingEntities.Add(e);
						}
					}

					searchCounter.Dispose();
					resultCounter.Dispose();

					searchJob.results.Dispose();
				}
                else
                {
                     targets.Dispose();
                }
			}
			else if (this.selectionType == SelectionType.BUILDING && this.routeHighlightOptions.incomingRoutes)
			{
				EntityTargetSearchJob searchJob = new EntityTargetSearchJob();
				searchJob.searchTarget = this.selectedEntity;
				if (EntityManager.TryGetBuffer<Renter>(this.selectedEntity, true, out var renterBuffer) && renterBuffer.Length > 0)
				{
					searchJob.searchTarget2 = renterBuffer[0].m_Renter;
					searchJob.hasTarget2 = true;
				}
				else
				{
					searchJob.searchTarget2 = default;
					searchJob.hasTarget2 = false;
				}

				searchJob.results = new NativeList<Entity>(128, Allocator.TempJob);
				searchJob.targetHandle = SystemAPI.GetComponentTypeHandle<Target>(true);
				searchJob.entityHandle = SystemAPI.GetEntityTypeHandle();
				searchJob.searchCounter = new NativeCounter(Allocator.TempJob);
				var jobHandle = JobChunkExtensions.Schedule(searchJob, this.hasTargetQuery, default);

				
				jobHandle.Complete();

				if (this.debugActiveBinding.value)
				{
					this.bindings["Search Count"] = searchJob.searchCounter.Count.ToString();
					this.bindings.Remove("Target Count");
				}
					
				searchJob.searchCounter.Dispose();

				for (int i = 0; i < searchJob.results.Length; ++i)
				{
					Entity e = searchJob.results[i];

					SelectionType entityRouteType = this.getEntityRouteType(e);
					if (entityRouteType != SelectionType.CAR_OCCUPANT)
					{
						this.commutingEntities.Add(e);
					}
				}

				searchJob.results.Dispose();	
			}
			else if (this.routeHighlightOptions.highlightSelected)
			{
				EntitySelectJob entitySelectJob = new EntitySelectJob();
				entitySelectJob.input = this.selectedEntity;
				entitySelectJob.inputSelectionType = this.selectionType;
				entitySelectJob.results = new NativeList<Entity>(Allocator.TempJob);
				entitySelectJob.targetLookup = GetComponentLookup<Target>();
				entitySelectJob.controllerLookup = GetComponentLookup<Controller>();
				entitySelectJob.currentVehicleLookup = GetComponentLookup<CurrentVehicle>();
				entitySelectJob.animalLookup = GetComponentLookup<Animal>();
				entitySelectJob.publicTransportLookup = GetComponentLookup<PublicTransport>();
				entitySelectJob.currentTransportLookup = GetComponentLookup<CurrentTransport>();
				entitySelectJob.passengerLookup = GetBufferLookup<Passenger>();
				entitySelectJob.layoutElementLookup = GetBufferLookup<LayoutElement>();
				entitySelectJob.pathElementLookup = GetBufferLookup<PathElement>();

				entitySelectJob.highlightTransitPassengerRoutes = this.routeHighlightOptions.transitPassengerRoutes;

				JobHandle jobHandle = entitySelectJob.Schedule();

				jobHandle.Complete();

				foreach (Entity result in entitySelectJob.results)
				{
					this.commutingEntities.Add(result);
				}

				entitySelectJob.results.Dispose();
			}
		}

		private NativeHashSet<Entity> getSelectedEntityTargets()
		{
			NativeHashSet<Entity> targets = new NativeHashSet<Entity>(16, Allocator.TempJob);

			if (EntityManager.TryGetBuffer<Renter>(this.selectedEntity, true, out var renterBuffer) && renterBuffer.Length > 0)
			{
				targets.Add(renterBuffer[0].m_Renter);
			}
			if (EntityManager.TryGetBuffer<SubLane>(this.selectedEntity, true, out var laneBuffer))
			{
				if (this.targetLaneCurves.IsCreated)
				{
					this.targetLaneCurves.Dispose();
				}

				if (laneBuffer.Length > 0)
				{
					this.targetLaneCurves = new NativeList<Bezier4x3>(16, Allocator.Persistent);
					for (int i = 0; i < laneBuffer.Length; ++i)
					{
						if (this.activeLaneIndexes[i])
						{
							targets.Add(laneBuffer[i].m_SubLane);
							if (EntityManager.TryGetComponent<Curve>(laneBuffer[i].m_SubLane, out Curve curve))
							{
								this.targetLaneCurves.Add(curve.m_Bezier);
							}
						}
					}
				}
			}
			if (EntityManager.TryGetBuffer<SubObject>(this.selectedEntity, true, out var subnetBuffer))
			{
				for (int i = 0; i < subnetBuffer.Length; ++i)
				{
					if (EntityManager.TryGetBuffer<SubLane>(subnetBuffer[i].m_SubObject, true, out var subnetLaneBuffer))
					{
						for (int j = 0; j < subnetLaneBuffer.Length; ++j)
						{
							targets.Add(subnetLaneBuffer[j].m_SubLane);
						}
					}
				}
			}

			return targets;
		}

		private SelectionType getEntityRouteType(Entity e)
		{
			if (EntityManager.HasComponent<Road>(e))
			{
				return SelectionType.ROAD;
			} 
			else if (EntityManager.HasComponent<PublicTransport>(e))
			{
				if (EntityManager.HasComponent<Train>(e))
				{
					return SelectionType.TRAIN;
				}
				else if (EntityManager.HasComponent<Watercraft>(e))
				{
					return SelectionType.BOAT;
				}
				else if (EntityManager.HasComponent<Airplane>(e))
				{
					return SelectionType.AIRPLANE;
				}
				else if (EntityManager.HasComponent<Car>(e))
				{
					return SelectionType.TRANSIT;
				}
			}
			if (EntityManager.HasComponent<Building>(e))
			{
				return SelectionType.BUILDING;
			}
			else if (EntityManager.HasComponent<Car>(e))
			{
				return SelectionType.CAR;
			}
			else if (EntityManager.HasComponent<Human>(e))
			{
				if (EntityManager.TryGetComponent(e, out CurrentVehicle currentVehicle))
				{
					if (EntityManager.HasComponent<PublicTransport>(currentVehicle.m_Vehicle))
					{
						if (EntityManager.HasComponent<Train>(currentVehicle.m_Vehicle))
						{
							return SelectionType.TRAIN_OCCUPANT;
						}
						else if (EntityManager.HasComponent<Watercraft>(currentVehicle.m_Vehicle))
						{
							return SelectionType.BOAT_OCCUPANT;
						}
						else if (EntityManager.HasComponent<Car>(currentVehicle.m_Vehicle))
						{
							return SelectionType.TRANSIT_OCCUPANT;
						}
					}
					else if (EntityManager.HasComponent<Car>(currentVehicle.m_Vehicle))
					{
						return SelectionType.CAR_OCCUPANT;
					}
				} 
				else
				{
					return SelectionType.HUMAN;
				}
			}
			else if (EntityManager.HasComponent<Animal>(e))
			{
				return SelectionType.ANIMAL;
			}
			else if (EntityManager.HasComponent<HouseholdMember>(e) && EntityManager.HasComponent<CurrentTransport>(e))
			{
				return SelectionType.RESIDENT;
			}

			return SelectionType.UNKNOWN;
		}

		private void info(string message)
		{
			if (Mod.log != null && message != null)
			{
				Mod.log.Info(message);
			}
		}

		private void reset()
		{
			this.selectedEntity = default;
			this.resetActiveLanes();
			if (this.commutingEntities.IsCreated)
			{ 
				this.commutingEntities.Dispose();
			}

			if (this.debugActiveBinding.value)
			{
				this.trackedEntityCount.Update(0);
			}

			if (this.targetLaneCurves.IsCreated)
			{
				this.targetLaneCurves.Dispose();
			}
		}

		private void endFrame(Stopwatch startTime)
		{
			if (this.debugActiveBinding.value)
			{
				startTime.Stop();
				this.bindings["Total Time (ms)"] = startTime.ElapsedMilliseconds.ToString();

				this.updateBindings();
			}
		}

		private void setActiveLanes(string laneList)
		{
			string[] laneVals = laneList.Split(',');
			for (int i = 0; i < laneVals.Length; i++)
			{
				this.activeLaneIndexes[i] = laneVals[i] == "1";
			}

			this.laneSetDirty = true;
			this.updateLaneBinding();
		}

		private void resetActiveLanes()
		{
			if (EntityManager.Exists(this.selectedEntity) && EntityManager.TryGetBuffer<SubLane>(this.selectedEntity, true, out var laneBuffer))
			{
				this.laneCount = laneBuffer.Length;
				for (int i = 0; i < this.laneCount; i++)
				{
					this.activeLaneIndexes[i] = true;
				}
			} else
			{
				this.laneCount = 0;
			}

			this.hoverCurve = default;
			this.hoverLane = -1;
			this.laneSetDirty = false;

			this.updateLaneBinding();
		}

		private int laneCount = 0;

		private void updateLaneBinding()
		{
			string laneIds = "";
			if (this.laneCount == 1)
			{
				laneIds = this.activeLaneIndexes[0] ? "1" : "0";
			}
			else if (this.laneCount > 0)
			{
				for (int i = 0; i < this.laneCount - 1; ++i)
				{
					laneIds += this.activeLaneIndexes[i] ? "1," : "0,";
				}

				laneIds += this.activeLaneIndexes[this.laneCount - 1] ? "1" : "0";
			}

			this.laneIdListBinding.Update(laneIds);
		}
	}
}
