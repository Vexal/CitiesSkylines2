using Colossal.Logging;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Tools;
using System.Collections.Generic;
using Unity.Entities;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem : GameSystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        Entity selectedEntity;
		List<Entity> highlightedEntities = new List<Entity>();

		protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {			
            Entity selected = this.getSelected();
            if (selected != null && !selected.Equals(this.selectedEntity))
			{
                this.clearHighlight();
                    
				this.selectedEntity = selected;

                //Mod.log.Info("Selected entity " + this.selectedEntity.ToString());

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
                                //Mod.log.Info("Renter " + employerRenter.ToString());

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
            else if (selected == null && this.selectedEntity != null)
            {
                this.clearHighlight();
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
