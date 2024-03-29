using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Input;
using Game.Pathfind;
using Game.Settings;
using Game.Tools;
using Game.UI.Menu;
using Game.Vehicles;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine.InputSystem;
using static Colossal.Animations.Animation;
using static Unity.Burst.Intrinsics.X86.Avx;
using Student = Game.Citizens.Student;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem : GameSystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        Entity selectedEntity;
		HashSet<Entity> highlightedEntities = new HashSet<Entity>();
        private InputAction action;
        ToolSystem toolSystem;

		protected override void OnCreate()
        {
            base.OnCreate();
            this.action = new InputAction("shiftEmployment", InputActionType.Button);
            this.action.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
        }

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
            this.action.Enable();
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
            this.action.Disable();
		}

		private void reset()
		{
			this.clearHighlight();
			this.prevPathIndex = -1;
			this.highlightedPathEntities.Clear();
			this.selectedEntity = default(Entity);
		}

		private bool toggled = true;
        private HashSet<Entity> highlightedPathEntities = new HashSet<Entity>();
		private int prevPathIndex = -1;

        protected override void OnUpdate()
        {
            Entity selected = this.getSelected();

			//only compute most highlighting when object selection changes (except for dynamic pathfinding)
			bool updatedSelection = this.toggled && selected != this.selectedEntity;
			if (updatedSelection)
			{
				this.reset();
				this.selectedEntity = selected;
			}

			//check if hot key disable/enable highlighting was pressed
			if (this.action.WasPressedThisFrame())
            {
				this.reset();
				this.toggled = !this.toggled;
				if (!this.toggled)
				{
					return;
				}
            }

			if (this.selectedEntity == null)
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

			this.highlightPathingRoute(this.selectedEntity);

				
            
                 
        }

		private HashSet<Entity> expectedPathElements = new HashSet<Entity>();

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
			HashSet<Entity> toRemove = new HashSet<Entity>();
			HashSet<Entity> toAdd = new HashSet<Entity>();
			if (EntityManager.HasBuffer<PathElement>(selected) && EntityManager.HasComponent<PathOwner>(selected))
			{
				//A single entity is in charge of the path of an object -- the PathOwner
				PathOwner pathOwner = EntityManager.GetComponentData<PathOwner>(selected);
				DynamicBuffer<PathElement> pathElements = EntityManager.GetBuffer<PathElement>(selected);

				if (pathOwner.m_State == PathFlags.Updated || this.prevPathIndex != pathOwner.m_ElementIndex)
				{
					toRemove = new HashSet<Entity>(this.highlightedPathEntities);

					this.prevPathIndex = -1;
					

					Mod.log.Info("Path index " + this.prevPathIndex + "; " + pathOwner.m_ElementIndex);
					for (int i = 0; i < pathElements.Length; ++i)
					{
						PathElement element = pathElements[i];
						if (element.m_Target != null && EntityManager.HasComponent<Owner>(element.m_Target))
						{
							Owner owner = EntityManager.GetComponentData<Owner>(element.m_Target);
							if (owner.m_Owner != null)
							{
								if (i < pathOwner.m_ElementIndex)
								{
									//toRemove.Add(owner.m_Owner);
								}
								else
								{
									toAdd.Add(owner.m_Owner);
									toRemove.Remove(owner.m_Owner);
								}
							}
						}
					}

					this.prevPathIndex = pathOwner.m_ElementIndex;
				}
			}

			

			if (EntityManager.HasBuffer<CarNavigationLane>(selected))
			{
				DynamicBuffer<CarNavigationLane> pathElements = EntityManager.GetBuffer<CarNavigationLane>(selected);		

				foreach (var element in pathElements)
				{
					if (element.m_Lane != null && EntityManager.HasComponent<Owner>(element.m_Lane))
					{
						Owner owner = EntityManager.GetComponentData<Owner>(element.m_Lane);
						if (owner.m_Owner != null)
						{
							toAdd.Add(owner.m_Owner);
							toRemove.Remove(owner.m_Owner);
						}						
					}
				}
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
				this.applyHighlight(entity, false);
				this.highlightedPathEntities.Add(entity);
			}
		}

        private void highlightEmployerAndResidences()
        {
			if (EntityManager.HasBuffer<Renter>(this.selectedEntity))
			{
				//Employer/Resident list
				DynamicBuffer<Renter> renters = EntityManager.GetBuffer<Renter>(this.selectedEntity);
				for (int i = 0; i < renters.Length; i++)
				{
					Entity renter = renters[i].m_Renter;
					if (renter != null)
					{
						if (EntityManager.HasBuffer<Employee>(renter))
						{
							DynamicBuffer<Employee> employees = EntityManager.GetBuffer<Employee>(renter);

							for (int j = 0; j < employees.Length; j++)
							{
								Entity worker = employees[j].m_Worker;

								this.highlightResidence(worker);
                                //highlight commuters on the way to work
								this.highlightTransport(worker, true);
							}
						}
						if (EntityManager.HasBuffer<HouseholdCitizen>(renter))
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
		}

        private void highlightStudentResidences()
        {
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
            {
				Mod.log.Info("Has element " + element.ToString());
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
    }
}
