using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Input;
using Game.Settings;
using Game.Tools;
using Game.UI.Menu;
using Game.Vehicles;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.InputSystem;
using Student = Game.Citizens.Student;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem : GameSystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        Entity selectedEntity;
		List<Entity> highlightedEntities = new List<Entity>();
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

		private bool toggled = true;
        protected override void OnUpdate()
        {
            Entity selected = this.getSelected();
            if (this.action.WasPressedThisFrame())
            {
                if (this.toggled)
                {
                    this.clearHighlight();
                    this.selectedEntity = default;
                }

                this.toggled = !this.toggled;
            }

            if (this.toggled && selected != null && !selected.Equals(this.selectedEntity))
			{
                this.clearHighlight();
                    
				this.selectedEntity = selected;

                this.handleForEmployers();
                this.handleForStudents();
                this.handleForPassengers();
            }
            else if (selected == null && this.selectedEntity != null)
            {
                this.clearHighlight();
            }                      
        }

        private void handleForEmployers()
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

        private void handleForStudents()
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

        private void handleForPassengers()
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

        private void clearHighlight()
        {
			for (int i = 0; i < this.highlightedEntities.Count; i++)
			{
				EntityManager.RemoveComponent<Highlighted>(this.highlightedEntities[i]);
				EntityManager.AddComponent<BatchesUpdated>(this.highlightedEntities[i]);
			}

            this.highlightedEntities.Clear();
		}

        private void applyHighlight(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            EntityManager.AddComponent<Highlighted>(entity);         
            this.highlightedEntities.Add(entity);
			EntityManager.AddComponent<BatchesUpdated>(entity);
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
