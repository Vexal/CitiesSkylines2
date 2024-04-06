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
		private bool addMap = true;
		private NativeArray<Entity> commutingEntities;

		public struct RouteJob : IJobParallelFor
		{
			//public NativeArray<Entity> commutingEntities;
			public OverlayRenderSystem.Buffer overlayBuffer;
			[ReadOnly]
			public NativeList<CurveDef> curveDefs;
			[ReadOnly]
			public RouteOptions routeHighlightOptions;

			public void Execute(int index)
			{
				if (index >= this.curveDefs.Length)
				{
					return;
				}

				CurveDef curve = this.curveDefs[index];
				overlayBuffer.DrawCurve(this.getCurveColor(curve.type, 1), curve.curve, this.getCurveWidth(curve.type),this.routeHighlightOptions.routeRoundness);
				/*for (int i = startIndex; i < batchSize && i < this.curveDefs.Length; ++i)
				{
					CurveDef curve = this.curveDefs[i];
					overlayBuffer.DrawCurve(this.getCurveColor(curve.type, 1), curve.curve, this.getCurveWidth(curve.type), new float2() { x = 1, y = 1 });
				}*
				/*foreach (var curve in this.curveDefs)
				{
					overlayBuffer.DrawCurve(this.getCurveColor(curve.type, 1), curve.curve, this.getCurveWidth(curve.type), new float2() { x = 1, y = 1 });
				}*/
			}

			public float getCurveWidth(byte type)
			{
				switch (type)
				{
					case 1:
						return this.routeHighlightOptions.vehicleLineWidth;
					case 2:
						return this.routeHighlightOptions.pedestrianLineWidth;
					case 3:
						return this.routeHighlightOptions.vehicleLineWidth;
					default:
						return 1f;
				}
			}

			public UnityEngine.Color getCurveColor(byte type, float weight)
			{
				UnityEngine.Color color;
				switch (type)
				{
					case 1:
						color = this.routeHighlightOptions.vehicleLineColor;
						break;
					case 2:
						color = this.routeHighlightOptions.pedestrianLineColor;
						break;
					case 3:
						color = this.routeHighlightOptions.subwayLineColor;
						break;
					default:
						color = this.routeHighlightOptions.vehicleLineColor;
						break;
				}

				color.a = .5f + weight * .05f;// math.max(.5f, weight * .1f);
				return color;
			}
		}


		[BurstCompile]
		public struct RouteRenderJobTest : IJob
		{
			public OverlayRenderSystem.Buffer overlayBuffer;
			[ReadOnly]
			public NativeArray<Bezier4x3> curves;
			public int curveCount;

			public void Execute()
			{
				//for (int i = 0; i < this.curves.Length; ++i)
				for (int i = 0; i < this.curveCount && i < 5; ++i)
				{
					Bezier4x3 curve = this.curves[i];
					//overlayBuffer.DrawCurve(new UnityEngine.Color(0, 1, 0), curve, 1f, new float2 { x = 1f, y = 1f });
					//overlayBuffer.DrawCurve(new UnityEngine.Color(0, 1, 0), new Bezier4x3(new float3()), 1f, new float2 { x = 1f, y = 1f });
					//overlayBuffer.DrawCircle(new UnityEngine.Color(0, 1, 0), new float3 { x=0, y=0, z=0}, 100f);
				}
			}
		}

		public struct RouteRenderJob : IJob
		{
			public OverlayRenderSystem.Buffer overlayBuffer;
			[ReadOnly]
			public NativeList<CurveDef> curveDefs;
			[ReadOnly]
			public RouteOptions routeHighlightOptions;

			public void Execute()
			{
				for (int i = 0; i < this.curveDefs.Length; ++i)
				{
					CurveDef curve = this.curveDefs[i];
					overlayBuffer.DrawCurve(this.getCurveColor(curve.type, 1), curve.curve, this.getCurveWidth(curve.type), this.routeHighlightOptions.routeRoundness);
				}
			}

			public float getCurveWidth(byte type)
			{
				switch (type)
				{
					case 1:
						return this.routeHighlightOptions.vehicleLineWidth;
					case 2:
						return this.routeHighlightOptions.pedestrianLineWidth;
					case 3:
						return this.routeHighlightOptions.vehicleLineWidth;
					default:
						return 1f;
				}
			}

			public UnityEngine.Color getCurveColor(byte type, float weight)
			{
				UnityEngine.Color color;
				switch (type)
				{
					case 1:
						color = this.routeHighlightOptions.vehicleLineColor;
						break;
					case 2:
						color = this.routeHighlightOptions.pedestrianLineColor;
						break;
					case 3:
						color = this.routeHighlightOptions.subwayLineColor;
						break;
					default:
						color = this.routeHighlightOptions.vehicleLineColor;
						break;
				}

				color.a = .5f + weight * .05f;// math.max(.5f, weight * .1f);
				return color;
			}
		}

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
			}

			if (this.addMapAction.WasPressedThisFrame())
			{
				this.addMap = !this.addMap;
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
			if (updatedSelection)
			{	
				if (this.commutingEntities.IsCreated)
				{
					this.commutingEntities.Dispose();
				}

				this.commutingEntities = this.hasTargetQuery.ToEntityArray(Allocator.Persistent);
			}

			/*if (this.commutingEntities.IsCreated && this.commutingEntities.Length > 0)
			{
				RouteJob job = new RouteJob();
				//job.commutingEntities = this.commutingEntities;
				job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
				JobHandle routeJobHandle = job.Schedule();
				this.overlayRenderSystem.AddBufferWriter(routeJobHandle);



			}*/

			if (this.pathingToggled)
			{
				CurveSet curvesToHighlight = new CurveSet();

				this.highlightPathingRoute(this.selectedEntity, curvesToHighlight);

				if (this.commutingEntities.IsCreated)
				{
					foreach (Entity e in this.commutingEntities)
					{
						if (EntityManager.Exists(e) && EntityManager.TryGetComponent(e, out Target target))
						{
							if (target.m_Target == this.selectedEntity)
							{
								this.highlightPathingRoute(e, curvesToHighlight);
							}
						}
					}
				}

				if (curvesToHighlight.curveDefs.IsCreated && !curvesToHighlight.curveDefs.IsEmpty)
				{
					RouteRenderJob job = new RouteRenderJob();
					job.curveDefs = curvesToHighlight.curveDefs;
					job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
					job.routeHighlightOptions = this.routeHighlightOptions;
					JobHandle routeJobHandle = job.Schedule(dependencies);

					this.overlayRenderSystem.AddBufferWriter(routeJobHandle);

					job.curveDefs.Dispose(routeJobHandle);

					/*RouteJob job = new RouteJob();
					//job.commutingEntities = this.commutingEntities;
					//curvesToHighlight.curveDefs.native
					job.curveDefs = curvesToHighlight.curveDefs;	
					job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
					job.routeHighlightOptions = this.routeHighlightOptions;
					//JobHandle routeJobHandle = job.Schedule();
					JobHandle routeJobHandle = job.Schedule(curvesToHighlight.curveDefs.Length, 32);
					this.overlayRenderSystem.AddBufferWriter(routeJobHandle);*/

					//RouteRenderJobTest job = new RouteRenderJobTest();
					//job.commutingEntities = this.commutingEntities;
					//curvesToHighlight.curveDefs.native
					//job.curveDefs = curvesToHighlight.curveDefs;
					//job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
					//job.routeHighlightOptions = this.routeHighlightOptions;
					//JobHandle routeJobHandle = job.Schedule();

					/*var curves = new NativeArray<Bezier4x3>(curvesToHighlight.curveDefs.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

					for (int i = 0; i < curvesToHighlight.curveDefs.Length; i++)
					{
						curves[i] = curvesToHighlight.curveDefs[i].curve;
					}

					job.curves = curves;
					job.curveCount = curvesToHighlight.curveDefs.Length;

					if (printFrame)
					{
						info("Printing frame; Add Map: " + this.addMap + "; count: " + curvesToHighlight.curveDefs.Length + "; add count: " + job.curveCount);
					}
						
					JobHandle routeJobHandle = job.Schedule(dependencies);
					this.overlayRenderSystem.AddBufferWriter(routeJobHandle);

					routeJobHandle.Complete();
					curves.Dispose(routeJobHandle);
					curvesToHighlight.curveDefs.Dispose(routeJobHandle);*/
					//job.curveDefs.Dispose(routeJobHandle);
					//curvesToHighlight.curveDefs.Dispose();
				}
				else if (curvesToHighlight.curveDefs.IsCreated)
				{
					curvesToHighlight.curveDefs.Dispose();
				}

				if (printFrame)
				{
					/*info("Printing frame; Add Map: " + this.addMap + "; count: " + curvesToHighlight.curve2Count.Count + "; add count: " + curvesToHighlight.addCount);
					foreach (var curve in curvesToHighlight.curve2Count)
					{
						if (printFrame)
						{
							info("count: " + curve.Value);
						}
					}*/

					info("Printing frame; Add Map: " + this.addMap + "; count: " + curvesToHighlight.curveDefs.Length + "; add count: " + curvesToHighlight.addCount);
					/*foreach (var curve in curvesToHighlight.curveDefs)
					{
						if (printFrame)
						{
							info("count: " + curve.Value);
						}
					}*/
				}

				/*if (false && curvesToHighlight.curve2Count.Count > 0)
				{
					JobHandle jHandle; //TODO: learn why this needs to reference a job; also, learn how to use jobs
					var overlayBuffer = this.overlayRenderSystem.GetBuffer(out jHandle);
					foreach (var curve in curvesToHighlight.curve2Count)
					{
						overlayBuffer.DrawCurve(this.getCurveColor(curve.Key.type, curve.Value), curve.Key.curve, this.getCurveWidth(curve.Key.type), new float2() { x = 1, y = 1 });
					}

					//jHandle.GetAwaiter().GetResult();
				}*/

				//curvesToHighlight.cleanup();
			}
		}

		public struct CurveElement
		{

		}
		public class CurveSet
		{
			//public Dictionary<CurveDef, int> curve2Count = new Dictionary<CurveDef, int>();
			//public NativeHashMap<CurveDef, int> curve2Count = new NativeHashMap<CurveDef, int>(1000, Allocator.Temp);
			public NativeList<CurveDef> curveDefs = new NativeList<CurveDef>(1000, Allocator.TempJob);
			//public NativeHashMap<Entity, int> curve2Count = new NativeHashMap<Entity, int>(1000, Allocator.Temp);
			public int addCount = 0;
			public CurveSet()
			{

			}

			public void add(CurveDef curve, bool addMap)
			{
				++this.addCount;
				/*if (this.curve2Count.TryGetValue(curve, out int currentCount))
				{
					if (addMap)
					{
						++this.curve2Count[curve];

					}
				}
				else
				{
					if (addMap)
						this.curve2Count[curve] = 1;
				}*/

				if (addMap)
				{
					this.curveDefs.Add(curve);
				}
				
			}

			public void cleanup()
			{
				//this.curve2Count.Dispose();
				this.curveDefs.Dispose();
			}
		}

		/**
		 * Hypothesis: an estimate of the fully planned route is contained in PathElement buffer.
		 * PathOwner.m_elementIndex is an index into PathElement[] (a path element is not removed from the buffer even
		 * after the entity has passed it)
		 * 
		 * Entities also have a CarNavigationLane buffer, which appears to contain up to 8 near-term pathing decisions,
		 * to be performed prior to the nearest PathElement; Combining both of these buffers appears to highlight a route.
		 * 
		 * TODO: support train tracks
		 */
		private void highlightPathingRoute(Entity selected, CurveSet results)
		{
			if (!this.highlightFeatures.routes || selected == null)
			{
				return;
			}

			//Highlight the path of a selected citizen inside a vehicle
			if (EntityManager.TryGetComponent(selected, out CurrentVehicle vehicle))
			{
				this.highlightPathingRoute(vehicle.m_Vehicle, results);
				return;
			}
			else if (EntityManager.TryGetComponent(selected, out CurrentTransport currentTransport))
			{
				this.highlightPathingRoute(currentTransport.m_CurrentTransport, results);
				return;
			}

			HashSet<Entity> toRemove = new HashSet<Entity>();
			HashSet<Entity> toAdd = new HashSet<Entity>();

			if (EntityManager.HasComponent<PathOwner>(selected) && EntityManager.TryGetBuffer(selected, true, out DynamicBuffer<PathElement> pathElements))
			{
				//A single entity is in charge of the path of an object -- the PathOwner
				PathOwner pathOwner = EntityManager.GetComponentData<PathOwner>(selected);
					
				for (int i = 0; i < pathElements.Length; ++i)
				{
					PathElement element = pathElements[i];
					if (EntityManager.TryGetComponent(element.m_Target, out Curve curve))
					{
						if (i >= pathOwner.m_ElementIndex)
						{
							results.add(this.getCurveDef(element.m_Target, curve, element.m_TargetDelta), this.addMap);									
						}
					}
					else if (EntityManager.TryGetComponent(element.m_Target, out Owner owner))
					{
						if (EntityManager.HasComponent<RouteLane>(element.m_Target) &&
							i < pathElements.Length - 1 &&
							EntityManager.TryGetComponent(element.m_Target, out Waypoint waypoint1) &&
							EntityManager.TryGetComponent(pathElements[i + 1].m_Target, out Waypoint waypoint2))
						{
							if (i >= pathOwner.m_ElementIndex)
							{
								if (EntityManager.TryGetBuffer(owner.m_Owner, true, out DynamicBuffer<RouteSegment> routeSegmentBuffer))
								{
									bool wrapAround = waypoint1.m_Index > waypoint2.m_Index;

									if (wrapAround)
									{
										this.getTrackRouteCurves(waypoint1.m_Index, routeSegmentBuffer.Length, routeSegmentBuffer, results, 3);
										this.getTrackRouteCurves(0, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, results, 3);
									}
									else
									{
										this.getTrackRouteCurves(waypoint1.m_Index, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, results, 3);
									}
								}
							}
						}
					}
				}			
			}

			this.getRouteNavigationCurves(selected, results);
		}

		private void getTrackRouteCurves(int startSegment, int endSegment, DynamicBuffer<RouteSegment> routeSegmentBuffer, CurveSet results, byte type = 3)
		{
			for (int trackInd = startSegment; trackInd < endSegment; trackInd++)
			{
				RouteSegment routeSegment = routeSegmentBuffer[trackInd];
				if (EntityManager.TryGetBuffer(routeSegment.m_Segment, true, out DynamicBuffer<PathElement> trackCurves))
				{
					foreach (PathElement curveElement in trackCurves)
					{
						if (EntityManager.TryGetComponent(curveElement.m_Target, out Curve curve))
						{
							results.add(new CurveDef(curve.m_Bezier, type), this.addMap);
						}
					}
				}
			}
		}

		private void getRouteNavigationCurves(Entity entity, CurveSet results)
		{
			if (EntityManager.TryGetBuffer(entity, true, out DynamicBuffer<CarNavigationLane> pathElements) && !pathElements.IsEmpty)
			{
				foreach (CarNavigationLane element in pathElements)
				{
					if (EntityManager.TryGetComponent(element.m_Lane, out Curve curve))
					{
						results.add(this.getCurveDef(element.m_Lane, curve, element.m_CurvePosition), this.addMap);
					}
				}
			}
		}

		private CurveDef getCurveDef(Entity entity, float2 delta)
		{
			Curve curve = EntityManager.GetComponentData<Curve>(entity);
			byte type = 1;
			if (EntityManager.HasComponent<PedestrianLane>(entity))
			{
				type = 2;
			}
			else if (EntityManager.HasComponent<TrackLane>(entity))
			{
				type = 3;
			}
			else if (EntityManager.HasComponent<SecondaryLane>(entity))
			{
				type = 0;
			}

			return new CurveDef(MathUtils.Cut(curve.m_Bezier, delta), type);
		}

		private CurveDef getCurveDef(Entity entity, Curve curve, float2 delta)
		{
			byte type = 1;
			if (EntityManager.HasComponent<PedestrianLane>(entity))
			{
				type = 2;
			}
			else if (EntityManager.HasComponent<TrackLane>(entity))
			{
				type = 3;
			}
			else if (EntityManager.HasComponent<SecondaryLane>(entity))
			{
				type = 0;
			}
			
			return new CurveDef(MathUtils.Cut(curve.m_Bezier, delta), type);
			//return new CurveDef(curve.m_Bezier, type);
		}

		public float getCurveWidth(byte type)
		{
			switch (type)
			{
				case 1:
					return this.routeHighlightOptions.vehicleLineWidth;
				case 2:
					return this.routeHighlightOptions.pedestrianLineWidth;
				case 3:
					return this.routeHighlightOptions.vehicleLineWidth;
				default:
					return 1f;
			}
		}

		public UnityEngine.Color getCurveColor(byte type, float weight)
		{
			UnityEngine.Color color;
			switch (type)
			{
				case 1:
					color = this.routeHighlightOptions.vehicleLineColor;
					break;
				case 2:
					color = this.routeHighlightOptions.pedestrianLineColor;
					break;
				case 3:
					color = this.routeHighlightOptions.subwayLineColor;
					break;
				default:
					color = this.routeHighlightOptions.vehicleLineColor;
					break;
			}

			color.a = .5f + weight * .05f;// math.max(.5f, weight * .1f);
			return color;
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
