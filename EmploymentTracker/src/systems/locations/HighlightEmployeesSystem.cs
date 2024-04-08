using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem : UISystemBase
    {
		private Entity selectedEntity;
		private HashSet<Entity> highlightedEntities = new HashSet<Entity>();
        private InputAction toggleSystemAction;
        private ToolSystem toolSystem;
		private EmploymentTrackerSettings settings;

		private HighlightFeatures highlightFeatures = new HighlightFeatures();

		private ValueBinding<bool> activateHighlightPassengerDestinations;
		private ValueBinding<bool> highlightEmployeeResidences;
		private ValueBinding<bool> highlightWorkplaces;
		private ValueBinding<bool> studentResidences;

		protected override void OnCreate()
        {
            base.OnCreate();
            this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
            this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");

			this.settings = Mod.INSTANCE.getSettings();

			this.highlightFeatures = new HighlightFeatures(settings);

			this.activateHighlightPassengerDestinations = new ValueBinding<bool>("EmploymentTracker", "highlightPassengerDestinations", this.settings.highlightDestinations);
			this.highlightWorkplaces = new ValueBinding<bool>("EmploymentTracker", "highlightResidentWorkplaces", this.settings.highlightWorkplaces);
			this.highlightEmployeeResidences = new ValueBinding<bool>("EmploymentTracker", "highlightEmployeeResidences", this.settings.highlightEmployeeResidences);
			this.studentResidences = new ValueBinding<bool>("EmploymentTracker", "highlightStudentResidences", this.settings.highlightStudentResidences);

			AddBinding(this.activateHighlightPassengerDestinations);
			AddBinding(this.highlightWorkplaces);
			AddBinding(this.highlightEmployeeResidences);
			AddBinding(this.studentResidences);

			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightPassengerDestinations", s => { this.activateHighlightPassengerDestinations.Update(s); this.settings.highlightDestinations = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightResidentWorkplaces", s => { this.highlightWorkplaces.Update(s); this.settings.highlightWorkplaces = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightEmployeeResidences", s => { this.highlightEmployeeResidences.Update(s); this.settings.highlightEmployeeResidences = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightStudentResidences", s => { this.studentResidences.Update(s); this.settings.highlightStudentResidences = s; this.saveSettings(); }));
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();

			this.settings.onSettingsApplied += gameSettings =>
			{
				if (gameSettings.GetType() == typeof(EmploymentTrackerSettings))
				{
					EmploymentTrackerSettings changedSettings = (EmploymentTrackerSettings)gameSettings;
					this.highlightFeatures = new HighlightFeatures(settings);
				}
			};

			this.toggleSystemAction.Enable();
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
            this.toggleSystemAction.Disable();
			this.reset();
		}

		private bool toggled = true;

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
					return;
				}
			}

			if (!this.toggled)
			{
				return;
			}

			if (this.selectedEntity == null || this.selectedEntity == default)
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
			this.selectedEntity = default(Entity);
		}

		private void saveSettings()
		{
			this.settings.ApplyAndSave();
		}
	}
}
