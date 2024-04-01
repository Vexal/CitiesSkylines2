using Colossal.Logging;
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
using Game.Tools;
using Game.Tutorials;
using Game.Vehicles;
using System.Collections.Generic;
using System.Threading;
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

		private HighlightFeatures highlightFeatures = new HighlightFeatures();
		private RouteOptions routeHighlightOptions = new RouteOptions();
		protected override void OnCreate()
        {
            base.OnCreate();
            this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
            this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
			this.togglePathDisplayAction = new InputAction("shiftPathing", InputActionType.Button);
			this.togglePathDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/v").With("Modifier", "<keyboard>/shift");
			
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
			}

			if (this.pathingToggled)
			{
				this.highlightPathingRoute(this.selectedEntity);
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
		private void highlightPathingRoute(Entity selected)
		{
			if (!this.highlightFeatures.routes || selected == null)
			{
				return;
			}

			//Highlight the path of a selected citizen inside a vehicle
			if (EntityManager.HasComponent<CurrentVehicle>(selected))
			{
				this.highlightPathingRoute(EntityManager.GetComponentData<CurrentVehicle>(selected).m_Vehicle);
				return;
			} else if (EntityManager.HasComponent<CurrentTransport>(selected))
			{
				this.highlightPathingRoute(EntityManager.GetComponentData<CurrentTransport>(selected).m_CurrentTransport);
				return;
			}

			HashSet<Entity> toRemove = new HashSet<Entity>();
			HashSet<Entity> toAdd = new HashSet<Entity>();
			List<CurveDef> curves = new List<CurveDef>();

			if (EntityManager.HasBuffer<PathElement>(selected) && EntityManager.HasComponent<PathOwner>(selected))
			{
				//A single entity is in charge of the path of an object -- the PathOwner
				PathOwner pathOwner = EntityManager.GetComponentData<PathOwner>(selected);
				DynamicBuffer<PathElement> pathElements = EntityManager.GetBuffer<PathElement>(selected);
			
				toRemove = new HashSet<Entity>(this.highlightedPathEntities);
					
				for (int i = 0; i < pathElements.Length; ++i)
				{
					PathElement element = pathElements[i];
					if (EntityManager.HasComponent<Curve>(element.m_Target))
					{
						if (i >= pathOwner.m_ElementIndex)
						{
							curves.Add(this.getCurveDef(element.m_Target));									
						}
					}
					else if (EntityManager.HasComponent<Owner>(element.m_Target))
					{
						Owner owner = EntityManager.GetComponentData<Owner>(element.m_Target);
						if (i >= pathOwner.m_ElementIndex)
						{
							toAdd.Add(owner.m_Owner);
							toRemove.Remove(owner.m_Owner);
						}
						else
						{
							toRemove.Add(owner.m_Owner);
						}
					}
				}			
			}			

			if (EntityManager.HasBuffer<CarNavigationLane>(selected))
			{
				DynamicBuffer<CarNavigationLane> pathElements = EntityManager.GetBuffer<CarNavigationLane>(selected);
				if (!pathElements.IsEmpty)
				{					
					foreach (CarNavigationLane element in pathElements)
					{						
						if (EntityManager.HasComponent<Curve>(element.m_Lane))
						{
							curves.Add(this.getCurveDef(element.m_Lane));
						}
						else if (EntityManager.HasComponent<Owner>(element.m_Lane))
						{
							Owner owner = EntityManager.GetComponentData<Owner>(element.m_Lane);
							toAdd.Add(owner.m_Owner);
							toRemove.Remove(owner.m_Owner);
						}
					}
				}
			}

			if (curves.Count > 0)
			{
				JobHandle jHandle; //TODO: learn why this needs to reference a job; also, learn how to use jobs
				var overlayBuffer = this.overlayRenderSystem.GetBuffer(out jHandle);

				foreach (var curve in curves)
				{
					overlayBuffer.DrawCurve(curve.color, curve.curve.m_Bezier, curve.width, new float2() { x = 1, y = 1 });
				}

				jHandle.Complete();
			}

			foreach (Entity entity in toRemove)
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
                            return;
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

		private CurveDef getCurveDef(Entity entity)
		{
			//TODO make this configurable
			Curve curve = EntityManager.GetComponentData<Curve>(entity);
			Color color = this.routeHighlightOptions.vehicleLineColor;
			float width = this.routeHighlightOptions.vehicleLineWidth;
			if (EntityManager.HasComponent<PedestrianLane>(entity))
			{
				color = this.routeHighlightOptions.pedestrianLineColor;
				width = this.routeHighlightOptions.pedestrianLineWidth;
			}
			else if (EntityManager.HasComponent<SecondaryLane>(entity))
			{
				width = 1f;
			}

			return new CurveDef(curve, color, width);
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
		}

		private void resetPathing()
		{
			this.clearHighlight(true);
			this.highlightedPathEntities.Clear();
		}
	}
}
