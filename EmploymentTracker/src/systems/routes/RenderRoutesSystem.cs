using Colossal.Entities;
using Colossal.Logging;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Pathfind;
using Game.Rendering;
using Game.Routes;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.InputSystem;

namespace EmploymentTracker
{
	internal partial class RenderRoutesSystem : UISystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        private Entity selectedEntity = default;
		private Entity selectionLeader = default;
		private SelectionType selectionType;
        private InputAction toggleSystemAction;
        private InputAction togglePathDisplayAction;
        private InputAction printFrameAction;
		private OverlayRenderSystem overlayRenderSystem;
        private ToolSystem toolSystem;
		private EmploymentTrackerSettings settings;
		private EntityQuery pathUpdatedQuery;
		private EntityQuery hasTargetQuery;
		private bool printFrame = false;

		private HighlightFeatures highlightFeatures = new HighlightFeatures();
		private RouteOptions routeHighlightOptions = new RouteOptions();


		private ValueBinding<bool> debugActiveBinding;
		private ValueBinding<bool> refreshTransitingEntitiesBinding;

		private ValueBinding<int> trackedEntityCount;
		private ValueBinding<int> undupedEntityCount;
		private ValueBinding<int> uniqueSegmentCount;
		private ValueBinding<int> totalSegmentCount;
		private ValueBinding<string> routeTimeMs;
		private ValueBinding<string> selectionTypeBinding;

		private ValueBinding<bool> incomingRoutes;
		private ValueBinding<bool> incomingRoutesTransit;
		private ValueBinding<bool> highlightSelected;
		private ValueBinding<bool> highlightPassengerRoutes;

		protected override void OnCreate()
        {
            base.OnCreate();
            this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
            this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
			this.togglePathDisplayAction = new InputAction("shiftPathing", InputActionType.Button);
			this.togglePathDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/v").With("Modifier", "<keyboard>/shift");

			this.printFrameAction = new InputAction("shiftFrame", InputActionType.Button);
			this.printFrameAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/g").With("Modifier", "<keyboard>/shift");
			//this.hasTargetQuery = GetEntityQuery(ComponentType.ReadOnly<Target>(), ComponentType.Exclude<Temp>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Unspawned>());
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
				ComponentType.ReadOnly<Building>(),
				//ComponentType.ReadOnly<Unspawned>()
				}
			});

			this.settings = Mod.INSTANCE.getSettings();

			//route toggles
			this.incomingRoutes = new ValueBinding<bool>("EmploymentTracker", "highlightEnroute", this.settings.incomingRoutes);
			this.highlightSelected = new ValueBinding<bool>("EmploymentTracker", "highlightSelectedRoute", this.settings.highlightSelected);
			this.incomingRoutesTransit = new ValueBinding<bool>("EmploymentTracker", "highlightEnrouteTransit", this.settings.incomingRoutesTransit);
			this.highlightPassengerRoutes = new ValueBinding<bool>("EmploymentTracker", "highlightPassengerRoutes", this.settings.highlightSelectedTransitVehiclePassengerRoutes);
			AddBinding(this.incomingRoutes);
			AddBinding(this.highlightSelected);
			AddBinding(this.incomingRoutesTransit);
			AddBinding(this.highlightPassengerRoutes);

			//toggles 

			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightEnroute", s => { this.incomingRoutes.Update(s); this.settings.incomingRoutes = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightSelectedRoute", s => { this.highlightSelected.Update(s); this.settings.highlightSelected = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightEnrouteTransit", s => { this.incomingRoutesTransit.Update(s); this.settings.incomingRoutesTransit = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightPassengerRoutes", s => { this.highlightPassengerRoutes.Update(s); this.settings.highlightSelectedTransitVehiclePassengerRoutes = s; this.saveSettings(); }));


			//options
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleAutoRefresh", this.toggleAutoRefresh));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleDebug", this.toggleDebug));

			this.debugActiveBinding = new ValueBinding<bool>("EmploymentTracker", "DebugActive", false);
			this.refreshTransitingEntitiesBinding = new ValueBinding<bool>("EmploymentTracker", "AutoRefreshTransitingEntitiesActive", true);

			AddBinding(this.debugActiveBinding);
			AddBinding(this.refreshTransitingEntitiesBinding);

			//stats
			this.trackedEntityCount = new ValueBinding<int>("EmploymentTracker", "TrackedEntityCount", 0);
			AddBinding(this.trackedEntityCount);
			this.uniqueSegmentCount = new ValueBinding<int>("EmploymentTracker", "UniqueSegmentCount", 0);
			AddBinding(this.uniqueSegmentCount);
			this.totalSegmentCount = new ValueBinding<int>("EmploymentTracker", "TotalSegmentCount", 0);
			AddBinding(this.totalSegmentCount);
			this.undupedEntityCount = new ValueBinding<int>("EmploymentTracker", "UndupedEntityCount", 0);
			AddBinding(this.undupedEntityCount);
			this.routeTimeMs = new ValueBinding<string>("EmploymentTracker", "RouteTimeMs", "");
			AddBinding(this.routeTimeMs);
			this.selectionTypeBinding = new ValueBinding<string>("EmploymentTracker", "selectionType", "");
			AddBinding(this.selectionTypeBinding);
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
            this.toggleSystemAction.Enable();
			this.togglePathDisplayAction.Enable();
			this.printFrameAction.Enable();
			this.overlayRenderSystem = World.GetExistingSystemManaged<OverlayRenderSystem>();

			this.highlightFeatures = new HighlightFeatures(settings);
			this.routeHighlightOptions = new RouteOptions(settings);

			this.settings.onSettingsApplied += gameSettings =>
			{
				if (gameSettings.GetType() == typeof(EmploymentTrackerSettings))
				{
					EmploymentTrackerSettings changedSettings = (EmploymentTrackerSettings) gameSettings;
					info("Settings thread: " + Thread.CurrentThread.ManagedThreadId);
					this.highlightFeatures = new HighlightFeatures(settings);
					this.routeHighlightOptions = new RouteOptions(settings);
				}
			};
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
            this.toggleSystemAction.Disable();
            this.togglePathDisplayAction.Disable();
            this.printFrameAction.Disable();
			this.reset();
		}

		private bool toggled = true;
		private bool pathingToggled = true;
		private NativeHashSet<Entity> commutingEntities;

		long frameCount = 0;

		protected override void OnUpdate()
		{
			var clock = new Stopwatch();
			clock.Start();

			if (this.highlightFeatures.dirty)
			{
				this.reset();
				this.highlightFeatures.dirty = false;
			}

			if (!this.highlightFeatures.highlightAnything()) {
				this.endFrame(clock);
				return;
			}

			Entity selected = this.getSelected();
			SelectionType newSelectionType = this.getEntityRouteType(selected);
			
			//only compute most highlighting when object selection changes (except for dynamic pathfinding)
			bool updatedSelection = this.toggled && (selected != this.selectedEntity || newSelectionType != this.selectionType);
			if (updatedSelection)
			{
				this.reset();
				this.selectedEntity = selected;
				this.selectionType = newSelectionType;
				if (this.debugActiveBinding.value)
				{
					this.selectionTypeBinding.Update(this.selectionType.ToString());
				}
			}

			//check if hot key disable/enable highlighting was pressed
			if (this.toggleSystemAction.WasPressedThisFrame())
			{
				this.reset();
				this.toggled = !this.toggled;
				if (!this.toggled)
				{
					this.pathingToggled = true;
				this.endFrame(clock);
					return;
				}
			}

			if (!this.toggled)
			{
				this.endFrame(clock);
				return;
			}

			if (this.togglePathDisplayAction.WasPressedThisFrame())
			{
				this.pathingToggled = !this.pathingToggled;
				if (!this.pathingToggled)
				{
					this.reset();
					this.endFrame(clock);
					return;
				}
			}

			if (this.printFrameAction.WasPressedThisFrame())
			{
				this.printFrame = true;
			}

			if (this.selectedEntity == null || this.selectedEntity == default(Entity) || this.selectionType == SelectionType.UNKNOWN)
			{
				this.endFrame(clock);
				return;
			}

			//only need to update building/target highlights when selection changes
			if (updatedSelection || (this.refreshTransitingEntitiesBinding.value && (++this.frameCount % 64 == 0)))
			{	
				var searchTimer = new Stopwatch();
				searchTimer.Start();

				this.populateRouteEntities(out int undupedCount);

				searchTimer.Stop();

				if (this.debugActiveBinding.value)
				{
					this.undupedEntityCount.Update(undupedCount);
					this.trackedEntityCount.Update(this.commutingEntities.Count);
					this.bindings["Search Time (ms)"] = searchTimer.ElapsedMilliseconds.ToString();
				}

				/*if (this.commutingEntities.IsCreated)
				{
					this.commutingEntities.Dispose();
				}

				var entitiesWithTargets = this.hasTargetQuery.ToEntityArray(Allocator.Temp);

				this.commutingEntities = new NativeHashSet<Entity>(128, Allocator.Persistent);

				int count = 0;
				//info("Starting selection");
				foreach (var e in entitiesWithTargets)
				{
					if (EntityManager.Exists(e) && EntityManager.TryGetComponent(e, out Target target))
					{
						if (target.m_Target == this.selectedEntity)
						{
							if (EntityManager.TryGetComponent(e, out CurrentVehicle vehicle))
							{
								this.commutingEntities.Add(vehicle.m_Vehicle);
							}
							else if (EntityManager.TryGetComponent(e, out CurrentTransport currentTransport))
							{
								this.commutingEntities.Add(currentTransport.m_CurrentTransport);
							}
							else
							{
								this.commutingEntities.Add(e);
							}

							++count;
							if (printFrame)
							{
								info("entity " + (count) + " with target: " + e.ToString());

							}
						}
					}
				}

				entitiesWithTargets.Dispose();

				if (this.debugActiveBinding.value)
				{
					this.undupedEntityCount.Update(count);
				}*/
			}

			if (this.commutingEntities.IsCreated)
			{
				this.doRouteJobs();
			}

			/*NativeList<Entity> tmp = new NativeList<Entity>(Allocator.TempJob);

			if (this.commutingEntities.IsCreated)
			{
				foreach (Entity e in this.commutingEntities)
				{
					if (this.isValidEntity(e) && EntityManager.TryGetComponent(e, out Target target))
					{
						if (target.m_Target == this.selectedEntity)
						{
							tmp.Add(e);
						}
					}
				}
			}

			if (this.routeHighlightOptions.highlightSelected && !EntityManager.HasComponent<Building>(this.selectedEntity))
			{
				tmp.Add(this.selectedEntity);
			}


			if (this.debugActiveBinding.value)
			{
				this.trackedEntityCount.Update(tmp.Length);
			}
			*/
			/*
			if (tmp.Length > 0)
			{
				if (printFrame)
				{
					info("Printing frame: commuter count: " + tmp.Length);
				}
				CalculateRoutesJob calculateRoutesJob = new CalculateRoutesJob();
				calculateRoutesJob.input = tmp;
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
				calculateRoutesJob.pathElementLookup = GetBufferLookup<PathElement>(true);
				calculateRoutesJob.routeSegmentLookup = GetBufferLookup<RouteSegment>(true);
				calculateRoutesJob.carNavigationLaneSegmentLookup = GetBufferLookup<CarNavigationLane>(true);
				calculateRoutesJob.leader = this.selectionLeader;
				calculateRoutesJob.selectionType = this.selectionType;

				int routeBatchSize = 16;

				calculateRoutesJob.batchSize = routeBatchSize;

				var resultStream = new NativeStream(JobsUtility.MaxJobThreadCount, Allocator.TempJob);

				calculateRoutesJob.results = resultStream.AsWriter();
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				JobHandle routeJob = calculateRoutesJob.ScheduleBatch(tmp.Length, routeBatchSize);

				tmp.Dispose(routeJob);

				routeJob.Complete();
				stopwatch.Stop();
				var elapsed_time = stopwatch.ElapsedMilliseconds;
				//info("Is job completed: " + routeJob.IsCompleted);
				//info("rs: " + resultStream.ForEachCount);
				NativeStream.Reader resultReader = resultStream.AsReader();
					
				stopwatch.Restart();
				NativeHashMap<CurveDef, int> resultCurves = new NativeHashMap<CurveDef, int>(1500, Allocator.Temp);

				int totalCount = 0;
				for (int i = 0; i < resultReader.ForEachCount; ++i)
				{
					resultReader.BeginForEachIndex(i);
					//info("Reader " + i + " remaining count: " + resultReader.RemainingItemCount + " rs count: " + resultStream.Count());
					while (resultReader.RemainingItemCount > 0)
					{
						++totalCount;
						CurveDef resultCurve = resultReader.Read<CurveDef>();
						if (resultCurves.ContainsKey(resultCurve))
						{
							++resultCurves[resultCurve];
						}
						else
						{
							resultCurves[resultCurve] = 1;
						}

						//resultCurves.Add(resultCurve);
					}

					resultReader.EndForEachIndex();
				}

				stopwatch.Stop();
				var streamReadTime = stopwatch.ElapsedMilliseconds;
				if (this.debugActiveBinding.value)
				{
					this.totalSegmentCount.Update(totalCount);
					this.uniqueSegmentCount.Update(resultCurves.Count);
					//this.routeTimeMs.Update((int)elapsed_time);
					this.bindings["Route Time (ms)"] = elapsed_time.ToString();
				}

				resultStream.Dispose();

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

					RouteRenderJob job = new RouteRenderJob();
					job.curveDefs = curveArray;
					job.curveCounts = curveCount;
					job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
					job.routeHighlightOptions = this.routeHighlightOptions;
					JobHandle routeJobHandle = job.Schedule(dependencies);

					curveArray.Dispose(routeJobHandle);
					curveCount.Dispose(routeJobHandle);
					this.overlayRenderSystem.AddBufferWriter(routeJobHandle);

					//routeJobHandle.Complete();

				}

				stopwatch.Stop();
				var renderTime = stopwatch.ElapsedMilliseconds;
				if (printFrame)
				{
					info("Printing frame: raw curve count " + totalCount + "; weighted count: " + resultCurves.Count + " route time ms: " + elapsed_time + "; stream read time ms: " + streamReadTime + "; render time ms: " + renderTime);

				}

				if (this.debugActiveBinding.value)
				{
					this.bindings["Render Time (ms)"] = renderTime.ToString();
					this.bindings["Stream Time (ms)"] = streamReadTime.ToString();
				}

				resultCurves.Dispose();
			}
			else
			{
				tmp.Dispose();
			}
			*/
			this.endFrame(clock);
		}

		private void doRouteJobs()
		{
			if (printFrame)
			{
				info("Printing frame: commuter count: " + this.commutingEntities.Count);
			}

			if (this.commutingEntities.IsEmpty)
			{
				return;
			}

			/*
			NativeArray<Entity> input = new NativeArray<Entity>(this.commutingEntities.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			int inexd = 0;
			foreach (Entity entity in this.commutingEntities)
			{
				input[inexd++] = entity;
			}
			*/
			CalculateRoutesJob calculateRoutesJob = new CalculateRoutesJob();
			calculateRoutesJob.input = new NativeList<Entity>(this.commutingEntities.Count, Allocator.TempJob);
			int inexd = 0;
			foreach (Entity entity in this.commutingEntities)
			{
				//calculateRoutesJob.input[inexd++] = entity;
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
			calculateRoutesJob.incomingRoutesTransit = this.incomingRoutesTransit.value;
			calculateRoutesJob.leader = this.selectionLeader;
			calculateRoutesJob.selectionType = this.selectionType;

			int routeBatchSize = 16;

			calculateRoutesJob.batchSize = routeBatchSize;

			var resultStream = new NativeStream(JobsUtility.MaxJobThreadCount, Allocator.TempJob);

			calculateRoutesJob.results = resultStream.AsWriter();
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			JobHandle routeJob = calculateRoutesJob.ScheduleBatch(calculateRoutesJob.input.Length, routeBatchSize);

			calculateRoutesJob.input.Dispose(routeJob);

			routeJob.Complete();
			stopwatch.Stop();
			var elapsed_time = stopwatch.ElapsedMilliseconds;
			//info("Is job completed: " + routeJob.IsCompleted);
			//info("rs: " + resultStream.ForEachCount);
			NativeStream.Reader resultReader = resultStream.AsReader();

			stopwatch.Restart();
			int totalCount = 0;
			NativeHashMap<CurveDef, int> resultCurves = new NativeHashMap<CurveDef, int>(1500, Allocator.Temp);

			for (int i = 0; i < resultReader.ForEachCount; ++i)
			{
				resultReader.BeginForEachIndex(i);
				while (resultReader.RemainingItemCount > 0)
				{
					++totalCount;
					CurveDef resultCurve = resultReader.Read<CurveDef>();
					if (resultCurves.ContainsKey(resultCurve))
					{
						++resultCurves[resultCurve];
					}
					else
					{
						resultCurves[resultCurve] = 1;
					}
				}

				resultReader.EndForEachIndex();
			}
			/*NativeHashSet<CurveDef> resultCurves = new NativeHashSet<CurveDef>(1500, Allocator.Temp);

			for (int i = 0; i < resultReader.ForEachCount; ++i)
			{
				resultReader.BeginForEachIndex(i);
				while (resultReader.RemainingItemCount > 0)
				{
					++totalCount;
					resultCurves.Add(resultReader.Read<CurveDef>());
				}

				resultReader.EndForEachIndex();
			}*/

			stopwatch.Stop();
			var streamReadTime = stopwatch.ElapsedMilliseconds;
			if (this.debugActiveBinding.value)
			{
				this.totalSegmentCount.Update(totalCount);
				this.uniqueSegmentCount.Update(resultCurves.Count);
				//this.routeTimeMs.Update((int)elapsed_time);
				this.bindings["Route Time (ms)"] = elapsed_time.ToString();
			}

			resultStream.Dispose();

			stopwatch.Restart();
			if (resultCurves.Count > 0)
			{
				//NativeList<CurveDef> curveArray = new NativeList<CurveDef>(resultCurves.Count, Allocator.TempJob);
				NativeArray<CurveDef> curveArray = new NativeArray<CurveDef>(resultCurves.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				NativeArray<int> curveCount = new NativeArray<int>(resultCurves.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				//NativeList<int> curveCount = new NativeList<int>(resultCurves.Count, Allocator.TempJob);

				int ind = 0;
				foreach (var curve in resultCurves)
				{
					curveArray[ind] = curve.Key;
					curveCount[ind] = curve.Value;
					//curveArray.Add(curve.Key);
					//curveCount.Add(curve.Value);
					//curveArray[ind] = curve;
					++ind;
				}

				RouteRenderJob job = new RouteRenderJob();
				job.curveDefs = curveArray;
				job.curveCounts = curveCount;
				job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
				job.routeHighlightOptions = this.routeHighlightOptions;
				JobHandle routeJobHandle = job.Schedule(dependencies);

				curveArray.Dispose(routeJobHandle);
				curveCount.Dispose(routeJobHandle);
				this.overlayRenderSystem.AddBufferWriter(routeJobHandle);

				routeJobHandle.Complete();

			}

			stopwatch.Stop();
			var renderTime = stopwatch.ElapsedMilliseconds;
			if (printFrame)
			{
				info("Printing frame: raw curve count " + totalCount + "; weighted count: " + resultCurves.Count + " route time ms: " + elapsed_time + "; stream read time ms: " + streamReadTime + "; render time ms: " + renderTime);

			}

			if (this.debugActiveBinding.value)
			{
				this.bindings["Render Time (ms)"] = renderTime.ToString();
				this.bindings["Stream Time (ms)"] = streamReadTime.ToString();
			}

			resultCurves.Dispose();
		}

		private Entity getSelected() 
        {
			ToolSystem toolSystem = World.GetExistingSystemManaged<ToolSystem>();
			return toolSystem.selected;
		}

		private void populateRouteEntities(out int undupedCount)
		{
			if (this.commutingEntities.IsCreated)
			{
				this.commutingEntities.Dispose();
			}

			this.commutingEntities = new NativeHashSet<Entity>(128, Allocator.Persistent);

			undupedCount = 0;
			if (this.selectionType == SelectionType.BUILDING)
			{
				NativeArray<Entity> entitiesWithTargets = this.hasTargetQuery.ToEntityArray(Allocator.Temp);

				if (this.printFrame)
				{
					info("Starting selection");
				}

				for (int i = 0; i < entitiesWithTargets.Length; i++)
				{
					Entity e = entitiesWithTargets[i];
					if (EntityManager.Exists(e) && EntityManager.TryGetComponent(e, out Target target))
					{
						if (target.m_Target == this.selectedEntity)
						{
							SelectionType entityRouteType = this.getEntityRouteType(e);
							if (entityRouteType != SelectionType.CAR_OCCUPANT)
							{
								this.commutingEntities.Add(e);
							}

							++undupedCount;
							if (printFrame)
							{
								info("entity " + (undupedCount) + " with target: " + e.ToString());

							}
						}
					}
				}

				entitiesWithTargets.Dispose();				
			}
			else
			{
				EntitySelectJob entitySelectJob = new EntitySelectJob();
				entitySelectJob.input = this.selectedEntity;
				entitySelectJob.results = new NativeList<Entity>(Allocator.TempJob);
				entitySelectJob.targetLookup = GetComponentLookup<Target>();
				entitySelectJob.controllerLookup = GetComponentLookup<Controller>();
				entitySelectJob.animalLookup = GetComponentLookup<Animal>();
				entitySelectJob.passengerLookup = GetBufferLookup<Passenger>();
				entitySelectJob.layoutElementLookup = GetBufferLookup<LayoutElement>();

				JobHandle jobHandle = entitySelectJob.Schedule();

				jobHandle.Complete();

				foreach (Entity result in entitySelectJob.results)
				{
					this.commutingEntities.Add(result);
				}

				entitySelectJob.results.Dispose();
				/*if (this.selectionType == SelectionType.HUMAN ||
					this.selectionType == SelectionType.CAR)
				{
					this.commutingEntities.Add(this.selectedEntity);
					this.selectionLeader = this.selectedEntity;
					++undupedCount;
				}
				else if (this.selectionType == SelectionType.BUS)
				{
					this.commutingEntities.Add(this.selectedEntity);
					this.selectionLeader = this.selectedEntity;
					++undupedCount;
				}
				else
				{
					this.selectionLeader = this.selectedEntity;
					this.commutingEntities.Add(this.selectedEntity);
					++undupedCount;
				}*/
				/*else if (this.selectionType == SelectionType.TRAIN)
				{

				}*/
			}
		}

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {          
            return 1;
		}

		private int updateTrackedEntitiesFromBuildingSelection()
		{
			NativeArray<Entity> entitiesWithTargets = this.hasTargetQuery.ToEntityArray(Allocator.Temp);

			if (this.printFrame)
			{
				info("Starting selection");
			}

			int undupedCount = 0;

			for (int i = 0; i < entitiesWithTargets.Length; i++)
			{
				Entity e = entitiesWithTargets[i];
				if (EntityManager.Exists(e) && EntityManager.TryGetComponent(e, out Target target))
				{
					if (target.m_Target == this.selectedEntity)
					{
						SelectionType entityRouteType = this.getEntityRouteType(e);
						if (entityRouteType != SelectionType.CAR_OCCUPANT)
						{
							this.commutingEntities.Add(e);
						}

						++undupedCount;
						if (printFrame)
						{
							info("entity " + (undupedCount) + " with target: " + e.ToString());

						}
					}
				}
			}

			entitiesWithTargets.Dispose();

			return undupedCount;
		}

		private SelectionType getEntityRouteType(Entity e)
		{
			if (EntityManager.HasComponent<PublicTransport>(e))
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

			return SelectionType.UNKNOWN;
		}

		private void info(string message)
		{
			if (Mod.log != null && message != null)
			{
				Mod.log.Info(message);
			}
		}

		private bool isValidEntity(Entity e)
		{
			return EntityManager.Exists(e) && !EntityManager.HasComponent<Deleted>(e) && !EntityManager.HasComponent<Temp>(e);
		}

		private void reset()
		{
			this.selectedEntity = default;
			this.selectionLeader = default;
			if (this.commutingEntities.IsCreated)
			{ 
				this.commutingEntities.Dispose();
			}

			if (this.debugActiveBinding.value)
			{
				this.trackedEntityCount.Update(0);
			}
		}

		private void toggleAutoRefresh(bool active) 
		{
			info("Toggling autorefresh: " + active); 
			this.refreshTransitingEntitiesBinding.Update(active);  
		}

		private void toggleDebug(bool active)
		{
			this.debugActiveBinding.Update(active);
		}

		private Dictionary<string, string> bindings = new Dictionary<string, string>();

		private void updateBindings()
		{
			List<string> bindingList = new List<string>(this.bindings.Count);
			foreach (var b in this.bindings)
			{
				bindingList.Add(b.Key + "," +  b.Value);
			}

			this.routeTimeMs.Update(string.Join(":", bindingList));
		}

		private void endFrame(Stopwatch startTime)
		{
			if (this.debugActiveBinding.value)
			{
				startTime.Stop();
				this.bindings["Total Time (ms)"] = startTime.ElapsedMilliseconds.ToString();

				this.updateBindings();
			}

			this.printFrame = false;
		}

		[BurstCompile]
		public static void readRouteStream(NativeStream.Reader resultReader, out NativeArray<CurveDef> curveArray, out NativeArray<int> curveCount, out int totalCount)
		{
			NativeHashMap<CurveDef, int> curveMap = new NativeHashMap<CurveDef, int>(1500, Allocator.Temp);

			totalCount = 0;
			for (int i = 0; i < resultReader.ForEachCount; ++i)
			{
				resultReader.BeginForEachIndex(i);
				//info("Reader " + i + " remaining count: " + resultReader.RemainingItemCount + " rs count: " + resultStream.Count());
				while (resultReader.RemainingItemCount > 0)
				{
					++totalCount;
					CurveDef resultCurve = resultReader.Read<CurveDef>();
					if (curveMap.ContainsKey(resultCurve))
					{
						++curveMap[resultCurve];
					}
					else
					{
						curveMap[resultCurve] = 1;
					}

					//resultCurves.Add(resultCurve);
				}

				resultReader.EndForEachIndex();
			}

			if (curveMap.Count > 0)
			{
				curveArray = new NativeArray<CurveDef>(curveMap.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				curveCount = new NativeArray<int>(curveMap.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

				int ind = 0;
				foreach (var curve in curveMap)
				{
					curveArray[ind] = curve.Key;
					curveCount[ind] = curve.Value;

					++ind;
				}
			} 
			else
			{
				curveArray = default;
				curveCount = default;
			}
		}

		private void saveSettings()
		{
			this.settings.ApplyAndSave();
		}
	}
}
