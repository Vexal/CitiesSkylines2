using Colossal;
using Colossal.Entities;
using Colossal.Logging;
using Colossal.Mathematics;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Events;
using Game.Net;
using Game.Pathfind;
using Game.Rendering;
using Game.Routes;
using Game.Tools;
using Game.Tutorials;
using Game.Vehicles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static Colossal.Json.DiffUtility;
using static EmploymentTracker.RenderRoutesSystem;

namespace EmploymentTracker
{
	internal partial class RenderRoutesSystem : GameSystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        Entity selectedEntity;
        private InputAction toggleSystemAction;
        private InputAction togglePathDisplayAction;
        private InputAction printFrameAction;
        private InputAction addMapAction;
		private OverlayRenderSystem overlayRenderSystem;
        ToolSystem toolSystem;
		EmploymentTrackerSettings settings;
		private EntityQuery pathUpdatedQuery;
		private EntityQuery hasTargetQuery;

		private HighlightFeatures highlightFeatures = new HighlightFeatures();
		private RouteOptions routeHighlightOptions = new RouteOptions();
		protected override void OnCreate()
        {
            base.OnCreate();
            this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
            this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
			this.togglePathDisplayAction = new InputAction("shiftPathing", InputActionType.Button);
			this.togglePathDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/v").With("Modifier", "<keyboard>/shift");

			this.printFrameAction = new InputAction("shiftFrame", InputActionType.Button);
			this.printFrameAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/g").With("Modifier", "<keyboard>/shift");
			this.addMapAction = new InputAction("shifMap", InputActionType.Button);
			this.addMapAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/m").With("Modifier", "<keyboard>/shift");
			this.hasTargetQuery = GetEntityQuery(ComponentType.ReadOnly<Target>());

		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
            this.toggleSystemAction.Enable();
			this.togglePathDisplayAction.Enable();
			this.printFrameAction.Enable();
			this.addMapAction.Enable();
			this.overlayRenderSystem = World.GetExistingSystemManaged<OverlayRenderSystem>();
			this.settings = Mod.INSTANCE.getSettings();

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
            this.addMapAction.Disable();
			this.reset();
		}

		private bool toggled = true;
		private bool pathingToggled = true;
		private NativeList<Entity> commutingEntities;

		long frameCount = 0;

		protected override void OnUpdate()
		{
			if (this.highlightFeatures.dirty)
			{
				this.reset();
				this.highlightFeatures.dirty = false;
			}

			if (!this.highlightFeatures.highlightAnything()) {
				return;
			}

			Entity selected = this.getSelected();
			
			//only compute most highlighting when object selection changes (except for dynamic pathfinding)
			bool updatedSelection = this.toggled && selected != this.selectedEntity;
			if (updatedSelection)
			{
				this.reset();
				this.selectedEntity = selected;
			}

			//check if hot key disable/enable highlighting was pressed
			if (this.toggleSystemAction.WasPressedThisFrame())
			{
				this.reset();
				this.toggled = !this.toggled;
				if (!this.toggled)
				{
					this.pathingToggled = true;
					return;
				}
			}

			if (!this.toggled)
			{
				return;
			}

			if (this.togglePathDisplayAction.WasPressedThisFrame())
			{
				this.pathingToggled = !this.pathingToggled;
				if (!this.pathingToggled)
				{
					this.reset();
					return;
				}
			}

			bool printFrame = false;
			if (this.printFrameAction.WasPressedThisFrame())
			{
				printFrame = true;
			}

			if (this.selectedEntity == null || this.selectedEntity == default(Entity))
			{
				return;
			}

			//only need to update building/target highlights when selection changes
			if (updatedSelection) // || (++this.frameCount % 64 == 0))
			{	
				if (this.commutingEntities.IsCreated)
				{
					this.commutingEntities.Dispose();
				}

				var tmp = this.hasTargetQuery.ToEntityArray(Allocator.Temp);
				if (tmp.Length > 0)
				{
					this.commutingEntities = new NativeList<Entity>(Allocator.Persistent);

					foreach (var e in tmp)
					{
						if (EntityManager.Exists(e) && EntityManager.TryGetComponent(e, out Target target))
						{
							if (target.m_Target == this.selectedEntity)
							{
								this.commutingEntities.Add(e);
							}
						}
					}
				}
			}

			if (this.pathingToggled)
			{
				NativeList<Entity> tmp = new NativeList<Entity>(Allocator.TempJob);
				tmp.Add(this.selectedEntity);

				if (this.commutingEntities.IsCreated)
				{

					foreach (Entity e in this.commutingEntities)
					{
						if (EntityManager.Exists(e) && EntityManager.TryGetComponent(e, out Target target))
						{
							if (target.m_Target == this.selectedEntity)
							{
								tmp.Add(e);
							}
						}
					}
				}

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

					var resultStream = new NativeStream(JobsUtility.MaxJobThreadCount, Allocator.TempJob);

					calculateRoutesJob.results = resultStream.AsWriter();

					JobHandle routeJob = calculateRoutesJob.Schedule(tmp.Length, 16);

					tmp.Dispose(routeJob);

					routeJob.Complete();
					NativeStream.Reader resultReader = resultStream.AsReader();
					//NativeList<CurveDef> resultCurves = new NativeList<CurveDef>(Allocator.TempJob);
					

					NativeHashMap<CurveDef, int> resultCurves = new NativeHashMap<CurveDef, int>(1500, Allocator.Temp);

					int totalCount = 0;
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

							//resultCurves.Add(resultCurve);
						}

						resultReader.EndForEachIndex();
					}

					resultStream.Dispose();

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

						if (printFrame)
						{
							info("Printing frame: raw curve count " + totalCount + "; weighted count: " + resultCurves.Count);
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

					resultCurves.Dispose();
				}
				else
				{
					tmp.Dispose();
				}
			}
		}

        private Entity getSelected() 
        {
			ToolSystem toolSystem = World.GetExistingSystemManaged<ToolSystem>();
			return toolSystem.selected;
		}

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {          
            return 1;
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
			this.selectedEntity = default(Entity);
			if (this.commutingEntities.IsCreated)
			{
				this.commutingEntities.Dispose();
			}
		}
	}
}
