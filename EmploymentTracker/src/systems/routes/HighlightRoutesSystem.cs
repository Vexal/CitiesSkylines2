﻿using Colossal.Entities;
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
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.InputSystem;

namespace EmploymentTracker
{
	internal partial class HighlightRoutesSystem : UISystemBase
    {
        private Entity selectedEntity = default;
		private SelectionType selectionType;
        private InputAction toggleSystemAction;
        private InputAction togglePathDisplayAction;
        private InputAction printFrameAction;
		private OverlayRenderSystem overlayRenderSystem;
		private EmploymentTrackerSettings settings;
		private EntityQuery hasTargetQuery;
		private bool printFrame = false;

		private HighlightFeatures highlightFeatures = new HighlightFeatures();
		private RouteOptions routeHighlightOptions = new RouteOptions();


		private ValueBinding<bool> debugActiveBinding;
		private ValueBinding<bool> refreshTransitingEntitiesBinding;

		private ValueBinding<int> trackedEntityCount;
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

			//Init UI and IO
			this.settings = Mod.INSTANCE.getSettings();
			this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
            this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
			this.togglePathDisplayAction = new InputAction("shiftPathing", InputActionType.Button);
			this.togglePathDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/v").With("Modifier", "<keyboard>/shift");

			this.printFrameAction = new InputAction("shiftFrame", InputActionType.Button);
			this.printFrameAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/g").With("Modifier", "<keyboard>/shift");

	
			//route toggles
			this.incomingRoutes = new ValueBinding<bool>("EmploymentTracker", "highlightEnroute", this.settings.incomingRoutes);
			this.highlightSelected = new ValueBinding<bool>("EmploymentTracker", "highlightSelectedRoute", this.settings.highlightSelected);
			this.incomingRoutesTransit = new ValueBinding<bool>("EmploymentTracker", "highlightEnrouteTransit", this.settings.incomingRoutesTransit);
			this.highlightPassengerRoutes = new ValueBinding<bool>("EmploymentTracker", "highlightPassengerRoutes", this.settings.highlightSelectedTransitVehiclePassengerRoutes);
			AddBinding(this.incomingRoutes);
			AddBinding(this.highlightSelected);
			AddBinding(this.incomingRoutesTransit);
			AddBinding(this.highlightPassengerRoutes);

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
					this.bindings["Selected Entity"] = this.selectedEntity.Index.ToString() + "-" + this.selectedEntity.Version.ToString();
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
			if (printFrame)
			{
				info("Printing frame: commuter count: " + this.commutingEntities.Count);
			}

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

			int routeBatchSize = 16;

			calculateRoutesJob.batchSize = routeBatchSize;

			int batchCount = (calculateRoutesJob.input.Length / routeBatchSize) + 1;

			var resultStream = new NativeStream(batchCount, Allocator.TempJob);

			calculateRoutesJob.results = resultStream.AsWriter();
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			JobHandle routeJob = calculateRoutesJob.ScheduleBatch(calculateRoutesJob.input.Length, routeBatchSize);

			calculateRoutesJob.input.Dispose(routeJob);

			routeJob.Complete();
			stopwatch.Stop();
			var elapsed_time = stopwatch.ElapsedMilliseconds;
			NativeStream.Reader resultReader = resultStream.AsReader();

			stopwatch.Restart();
			int totalCount = 0;

			//Weight segments with multiple entities passing over heavier
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

			stopwatch.Stop();
			var streamReadTime = stopwatch.ElapsedMilliseconds;
			if (this.debugActiveBinding.value)
			{
				this.totalSegmentCount.Update(totalCount);
				this.uniqueSegmentCount.Update(resultCurves.Count);
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

		private void populateRouteEntities()
		{
			if (this.commutingEntities.IsCreated)
			{
				this.commutingEntities.Dispose();
			}

			this.commutingEntities = new NativeHashSet<Entity>(128, Allocator.Persistent);

			if (this.selectionType == SelectionType.BUILDING && this.routeHighlightOptions.incomingRoutes)
			{
				EntitySearchJob searchJob = new EntitySearchJob();
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
				searchJob.searchCounter = new Colossal.NativeCounter(Allocator.TempJob);
				var jobHandle = JobChunkExtensions.Schedule(searchJob, this.hasTargetQuery, default);

				
				jobHandle.Complete();

				if (this.debugActiveBinding.value)
				{
					this.bindings["Search Count"] = searchJob.searchCounter.Count.ToString();
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
			else
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

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {          
            return 1;
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

		private void saveSettings()
		{
			this.settings.ApplyAndSave();
		}
	}
}