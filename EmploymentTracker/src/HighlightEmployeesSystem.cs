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

		protected override void OnCreate()
        {
            base.OnCreate();
            var menu = World.GetExistingSystemManaged<OptionsUISystem>();
            menu.RegisterSetting(Mod.Settings, "isEnabled");
            Mod.Settings.onSettingsApplied += new OnSettingsAppliedHandler(async setting =>
            {
                Mod.log.Info("setting is enabled: " + Mod.Settings.enabled);
				await AssetDatabase.global.SaveSettings();
			});
            Mod.Settings.enabled = true;
            //InputManager.instance.GetComposites();
            this.action = new InputAction("shiftEmployment", InputActionType.Button);
            //this.action.AddBinding(new InputBinding("<keyboard>/w"));
            this.action.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");

            //Mod.Settings.onSettingsApplied += new OnSettingsAppliedHandler();
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
                Mod.log.Info("Toggled: " + this.toggled);
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
				//Employer list
				DynamicBuffer<Renter> renters = EntityManager.GetBuffer<Renter>(this.selectedEntity);
				for (int i = 0; i < renters.Length; i++)
				{
					if (renters[i].m_Renter != null)
					{
						Entity employerRenter = renters[i].m_Renter;
						if (EntityManager.HasBuffer<Employee>(employerRenter))
						{
							DynamicBuffer<Employee> employees = EntityManager.GetBuffer<Employee>(employerRenter);

							for (int j = 0; j < employees.Length; j++)
							{
								Entity worker = employees[j].m_Worker;

								this.highlightResidence(worker);
								this.highlightTransport(worker, true);
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
                    Mod.log.Info("STudent: " + i);
                    this.highlightResidence(students[i].m_Student);
                    this.highlightTransport(students[i], true);
					/*if (students[i].m_Student != null)
					{
						Entity student = students[i].m_Student;
						if (EntityManager.HasBuffer<Employee>(employerRenter))
						{
							DynamicBuffer<Employee> employees = EntityManager.GetBuffer<Employee>(employerRenter);

							for (int j = 0; j < employees.Length; j++)
							{
								Entity worker = employees[j].m_Worker;

								this.highlightResidence(worker);
								this.highlightTransport(worker, true);
							}
						}
					}*/
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

				if (householdMember.m_Household != null)
				{
					if (EntityManager.HasComponent<PropertyRenter>(householdMember.m_Household))
					{
						PropertyRenter householdPropertyRenter = EntityManager.GetComponentData<PropertyRenter>(householdMember.m_Household);
						this.applyHighlight(householdPropertyRenter.m_Property);

					}
				}
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
