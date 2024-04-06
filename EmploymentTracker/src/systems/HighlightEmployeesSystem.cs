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
using System.Threading;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem : GameSystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        Entity selectedEntity;
		HashSet<Entity> highlightedEntities = new HashSet<Entity>();
        private InputAction toggleSystemAction;
        private InputAction togglePathDisplayAction;
		private OverlayRenderSystem overlayRenderSystem;
        ToolSystem toolSystem;
		EmploymentTrackerSettings settings;
		private EntityQuery pathUpdatedQuery;
		private EntityQuery hasTargetQuery;
		private EntityQuery timerQuery;

		private HighlightFeatures highlightFeatures = new HighlightFeatures();
		private RouteOptions routeHighlightOptions = new RouteOptions();
		protected override void OnCreate()
        {
            base.OnCreate();
            this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
            this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
			this.togglePathDisplayAction = new InputAction("shiftPathing", InputActionType.Button);
			this.togglePathDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/v").With("Modifier", "<keyboard>/shift");
			//this.pathUpdatedQuery = GetEntityQuery(ComponentType.ReadWrite<Updated>(), ComponentType.ReadWrite<PathUpdated>());
			this.pathUpdatedQuery = GetEntityQuery(ComponentType.ReadWrite<PathfindUpdated>());
			this.hasTargetQuery = GetEntityQuery(ComponentType.ReadOnly<Target>());

			this.timerQuery = GetEntityQuery(ComponentType.ReadWrite<DeleteTimer>());

		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
            this.toggleSystemAction.Enable();
			this.togglePathDisplayAction.Enable();
			this.overlayRenderSystem = World.GetExistingSystemManaged<OverlayRenderSystem>();
			this.settings = Mod.INSTANCE.getSettings();

			this.highlightFeatures = new HighlightFeatures(settings);
			this.routeHighlightOptions = new RouteOptions(settings);

			this.settings.onSettingsApplied += gameSettings =>
			{
				if (gameSettings.GetType() == typeof(EmploymentTrackerSettings))
				{
					EmploymentTrackerSettings changedSettings = (EmploymentTrackerSettings)this.settings;
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
			this.reset();
		}

		private bool toggled = true;
		private bool pathingToggled = true;
        private HashSet<Entity> highlightedPathEntities = new HashSet<Entity>();
		private long frameCount = 0;
		//private NativeArray<Entity> commutingEntities;

		protected override void OnUpdate()
		{
			++this.frameCount;

			if (this.highlightFeatures.dirty)
			{
				this.reset();
				this.highlightFeatures.dirty = false;
			}

			if (!this.highlightFeatures.highlightAnything()) {
				return;
			}


			NativeArray<Entity> timerEntities = this.timerQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity e in timerEntities)
			{
				DeleteTimer timer = EntityManager.GetComponentData<DeleteTimer>(e);
				if (this.frameCount >= timer.endFrame)
				{
					switch (timer.componentType)
					{
						case ComponentTypeSelector.HIGHLIGHT:
							EntityManager.RemoveComponent<Highlighted>(e);
							EntityManager.AddComponent<BatchesUpdated>(e);
							break;
					}

					EntityManager.RemoveComponent<DeleteTimer>(e);
				}
			}

			/*NativeArray<Entity> entities = this.pathUpdatedQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity entity in entities)
			{
				EntityManager.AddComponent<DeleteTimer>(entity);
				EntityManager.SetComponentData(entity, new DeleteTimer(this.frameCount, 300, ComponentTypeSelector.HIGHLIGHT));
				EntityManager.AddComponent<Highlighted>(entity);
				EntityManager.AddComponent<BatchesUpdated>(entity);

				info("Path updated for " + entity.ToString());
			}*/

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
				this.resetPathing();
				this.pathingToggled = !this.pathingToggled;
			}

			if (this.selectedEntity == null || this.selectedEntity == default(Entity))
			{
				return;
			}

			//only need to update building/target highlights when selection changes
			if (updatedSelection)
			{	
				this.highlightEmployerAndResidences();
				this.highlightStudentResidences();
				this.highlightPassengerDestinations();

				/*if (this.commutingEntities.IsCreated)
				{
					this.commutingEntities.Dispose();
				}

				this.commutingEntities = this.hasTargetQuery.ToEntityArray(Allocator.Persistent);*/
			}

			/*if (this.pathingToggled)
			{
				CurveSet curvesToHighlight = new CurveSet();

				this.highlightPathingRoute(this.selectedEntity, curvesToHighlight);

				if (this.commutingEntities.IsCreated)
				{
					foreach (Entity e in this.commutingEntities)
					{
						if (EntityManager.Exists(e) && EntityManager.TryGetComponent<Target>(e, out Target target))
						{
							if (target.m_Target == this.selectedEntity)
							{
								this.highlightPathingRoute(e, curvesToHighlight);
							}
						}
					}
				}

				if (curvesToHighlight.curve2Count.Count > 0)
				{
					JobHandle jHandle; //TODO: learn why this needs to reference a job; also, learn how to use jobs
					var overlayBuffer = this.overlayRenderSystem.GetBuffer(out jHandle);

					foreach (var curve in curvesToHighlight.curve2Count)
					{
						//overlayBuffer.DrawCurve(this.getCurveColor(curve.Key.type, curve.Value), curve.Key.curve, this.getCurveWidth(curve.Key.type), new float2() { x = 1, y = 1 });
					}

					jHandle.Complete();
				}
			}*/
		}

		public class CurveSet
		{
			public Dictionary<CurveDef, int> curve2Count = new Dictionary<CurveDef, int>();

			public void add(CurveDef curve)
			{
				if (this.curve2Count.TryGetValue(curve, out int currentCount))
				{
					++this.curve2Count[curve];
				} else
				{
					this.curve2Count[curve] = 1;
				}
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
			
				//toRemove = new HashSet<Entity>(this.highlightedPathEntities);
					
				for (int i = 0; i < pathElements.Length; ++i)
				{
					PathElement element = pathElements[i];
					if (EntityManager.TryGetComponent(element.m_Target, out Curve curve))
					{
						if (i >= pathOwner.m_ElementIndex)
						{
							results.add(this.getCurveDef(element.m_Target, curve, element.m_TargetDelta));									
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
						/*else
						{
							if (i >= pathOwner.m_ElementIndex)
							{
								toAdd.Add(owner.m_Owner);
								toRemove.Remove(owner.m_Owner);
							}
							else
							{
								toRemove.Add(owner.m_Owner);
							}
						}	*/	
					}
				}			
			}

			this.getRouteNavigationCurves(selected, results);

			/*foreach (Entity entity in toRemove)
			{
				if (!this.highlightedEntities.Contains(entity))
				{
					this.removeHighlight(entity);
				}

				this.highlightedPathEntities.Remove(entity);
			}

			foreach (Entity entity in toAdd)
			{
				this.highlightedPathEntities.Add(entity);
			}

			foreach (Entity entity in this.highlightedPathEntities)
			{
				this.applyHighlight(entity, false);
			}*/
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
							results.add(new CurveDef(curve.m_Bezier, type));
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
						results.add(this.getCurveDef(element.m_Lane, curve, element.m_CurvePosition));
					}
					/*else if (EntityManager.TryGetComponent(element.m_Lane, out Owner owner))
					{
						toAdd.Add(owner.m_Owner);
						toRemove.Remove(owner.m_Owner);
					}*/
				}
			}
		}

        private void highlightEmployerAndResidences()
        {
	        if (!(this.highlightFeatures.employeeResidences || this.highlightFeatures.workplaces))
	        {
		        return;
	        }

			if (EntityManager.HasBuffer<Renter>(this.selectedEntity))
			{
				//Employer/Resident list
				DynamicBuffer<Renter> renters = EntityManager.GetBuffer<Renter>(this.selectedEntity);
				for (int i = 0; i < renters.Length; i++)
				{
					Entity renter = renters[i].m_Renter;
					if (this.highlightFeatures.employeeResidences && EntityManager.HasBuffer<Employee>(renter))
					{
						DynamicBuffer<Employee> employees = EntityManager.GetBuffer<Employee>(renter);

						for (int j = 0; j < employees.Length; j++)
						{
							Entity worker = employees[j].m_Worker;

							this.highlightResidence(worker);
							if (this.highlightFeatures.employeeCommuters)
							{
								//highlight commuters on the way to work
								this.highlightTransport(worker, true);
							}
						}
					}
					if (this.highlightFeatures.workplaces && EntityManager.HasBuffer<HouseholdCitizen>(renter))
					{
						DynamicBuffer<HouseholdCitizen> citizens = EntityManager.GetBuffer<HouseholdCitizen>(renter);

						foreach (HouseholdCitizen householdMember in citizens)
						{
							this.highlightWorkplace(householdMember.m_Citizen);
						}
					}
				}
			}
		}

        private void highlightStudentResidences()
        {
			if (!this.highlightFeatures.studentResidences)
			{
				return;
			}

            if (EntityManager.HasBuffer<Game.Buildings.Student>(this.selectedEntity))
            {
				DynamicBuffer<Game.Buildings.Student> students = EntityManager.GetBuffer<Game.Buildings.Student>(this.selectedEntity);
				for (int i = 0; i < students.Length; i++)
				{
                    this.highlightResidence(students[i].m_Student);
                    this.highlightTransport(students[i], true);
				}
			}
        }

        private void highlightPassengerDestinations()
        {
			if (!this.highlightFeatures.destinations)
			{
				return;
			}

            if (EntityManager.HasBuffer<Passenger>(this.selectedEntity))
            {
                //Vehicle has multiple cars (such as a train)
                if (EntityManager.HasBuffer<LayoutElement>(this.selectedEntity))
                {
					//selected car is the controller
					this.handleForVehicleElements(EntityManager.GetBuffer<LayoutElement>(this.selectedEntity));
				}
				else if (EntityManager.HasComponent<Controller>(this.selectedEntity))
                {
                    //selected a car not controlling the overall vehicle
                    Controller controller = EntityManager.GetComponentData<Controller>(this.selectedEntity);
                    if (controller.m_Controller != null && EntityManager.HasBuffer<LayoutElement>(controller.m_Controller))
                    {
                        this.handleForVehicleElements(EntityManager.GetBuffer<LayoutElement>(controller.m_Controller));
                    }
                }
                else
                {
                    //vehicle only has one element
                    this.handleForPassengers(this.selectedEntity);
                }
			} else
            {
                //not in a vehicle
                this.highlightDsetination(this.selectedEntity);
            }
        }

        private void handleForVehicleElements(DynamicBuffer<LayoutElement> subObjects)
        {
            foreach(LayoutElement element in subObjects)
            {;
				if (element.m_Vehicle != null)
                {
                    this.handleForPassengers(element.m_Vehicle);
                }
            }
        }

        private void handleForPassengers(Entity entity)
        {
            if (EntityManager.HasBuffer<Passenger>(entity))
            {
				DynamicBuffer<Passenger> passengers = EntityManager.GetBuffer<Passenger>(entity);
				foreach (Passenger passenger in passengers)
				{
                    this.highlightDsetination(passenger.m_Passenger);
				}
			}
        }

        private void highlightResidence(Entity worker)
        {
			if (worker == null)
            {
                return;
            }

			if (EntityManager.HasComponent<HouseholdMember>(worker))
			{
				HouseholdMember householdMember = EntityManager.GetComponentData<HouseholdMember>(worker);
                this.highlightRentedProperty(householdMember.m_Household);				
			}
		}

        private void highlightDsetination(Entity traveler)
        {
            if (traveler == null)
            {
                return;
            }

			if (EntityManager.HasComponent<Target>(traveler))
            {
				Target destination = EntityManager.GetComponentData<Target>(traveler);
                this.applyHighlight(destination.m_Target);
			}
		}

        private void highlightTransport(Entity worker, bool onlyHighlightForwardTrips)
        {
            if (worker == null)
            {
                return;
            }

            if (EntityManager.HasComponent<CurrentTransport>(worker))
            {
                if (!onlyHighlightForwardTrips)
                {
                    if (EntityManager.HasComponent<TravelPurpose>(worker))
                    {
                        Purpose purpose = EntityManager.GetComponentData<TravelPurpose>(worker).m_Purpose;

						if (!(purpose == Purpose.GoingToWork || purpose == Purpose.GoingToSchool))
                        {
                           // return;
                        }
                    }
				}

				CurrentTransport transport = EntityManager.GetComponentData<CurrentTransport>(worker);
				this.applyHighlight(transport.m_CurrentTransport);

                if (transport.m_CurrentTransport != null)
                {
                    if (EntityManager.HasComponent<CurrentVehicle>(transport.m_CurrentTransport))
                    {
                        CurrentVehicle vehicle = EntityManager.GetComponentData<CurrentVehicle>(transport.m_CurrentTransport);
                        this.applyHighlight(vehicle.m_Vehicle);
                    }
                }
			}
        }

        private void highlightWorkplace(Entity citizen)
        {
			if (citizen != null && EntityManager.HasComponent<Worker>(citizen))
			{
                this.highlightRentedProperty(EntityManager.GetComponentData<Worker>(citizen).m_Workplace);
			}
		}

        private void highlightRentedProperty(Entity propertyRenter)
        {
            if (propertyRenter == null)
            {
                return;
            }

            if (EntityManager.HasComponent<PropertyRenter>(propertyRenter))
            {
				PropertyRenter renter = EntityManager.GetComponentData<PropertyRenter>(propertyRenter);
				this.applyHighlight(renter.m_Property);
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

		private void clearHighlight(bool pathingOnly=false)
        {
			if (!pathingOnly)
			{
				foreach (Entity entity in this.highlightedEntities)
				{
					this.removeHighlight(entity);
				}

				this.highlightedEntities.Clear();
			}

			foreach (Entity entity in this.highlightedPathEntities)
			{
				this.removeHighlight(entity);
			}

			this.highlightedPathEntities.Clear();
		}

        private void removeHighlight(Entity entity)
        {
            if (EntityManager.Exists(entity) && EntityManager.HasComponent<Highlighted>(entity))
            {
                EntityManager.RemoveComponent<Highlighted>(entity);
                EntityManager.AddComponent<BatchesUpdated>(entity);
            }
        }

        private bool applyHighlight(Entity entity, bool store = true)
        {
            if (entity == null || !EntityManager.Exists(entity) || EntityManager.HasComponent<Highlighted>(entity))
            {
                return false;
            }

			if (store)
			{
				this.highlightedEntities.Add(entity);
			}
				
			EntityManager.AddComponent<Highlighted>(entity);
			EntityManager.AddComponent<BatchesUpdated>(entity);
            return true;
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
			this.clearHighlight();
			this.resetPathing();
			this.selectedEntity = default(Entity);
			//if (this.commutingEntities.IsCreated)
			{
			//	this.commutingEntities.Dispose();
			}
		}

		private void resetPathing()
		{
			this.clearHighlight(true);
			this.highlightedPathEntities.Clear();
		}
	}

	class CurveComparator : IEqualityComparer<Curve>
	{
		public bool Equals(Curve x, Curve y)
		{
			return x.m_Bezier.Equals(y.m_Bezier);
		}

		public int GetHashCode(Curve obj)
		{
			return obj.m_Bezier.GetHashCode();
		}
	}
}
