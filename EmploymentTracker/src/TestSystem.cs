using Colossal.Entities;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Objects;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using static Game.Rendering.OverlayRenderSystem;
using Surface = Game.Objects.Surface;

namespace EmploymentTracker
{
    internal partial class TestSystem : GameSystemBase
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);


        EntityQuery tstQuery;
        private BufferLookup<Renter> renterLookup;
        int prevWorkerCount = 0;
        Entity selectedEntity;
		private EndFrameBarrier endFrameBarrier;

		protected override void OnCreate()
        {
            base.OnCreate();
            this.endFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();

			try
            {
                this.tstQuery = GetEntityQuery(ComponentType.ReadOnly<Worker>());
                this.renterLookup = GetBufferLookup<Renter>(true);
                Mod.log.Info("Succeeded entity query");
            }
            catch (Exception ex)
            {
                Mod.log.Info(ex, "Failed to get entity query");
            }
        }

        byte count = 0;
        protected override void OnUpdate()
        {
			try
            {
                ToolSystem toolSystem = World.GetExistingSystemManaged<ToolSystem>();
                Entity selected = ((ToolSystem)toolSystem).selected;
                if (selected != null && (this.selectedEntity == null || !selected.Equals(this.selectedEntity)))
                {
                    this.selectedEntity = selected;

                    var buffer = World.GetExistingSystemManaged<EntityCommandBufferSystem>().CreateCommandBuffer();

                    Mod.log.Info("Selected entity " + this.selectedEntity.ToString());
					if (EntityManager.HasBuffer<Renter>(this.selectedEntity))
					{
						DynamicBuffer<Renter> renters = EntityManager.GetBuffer<Renter>(this.selectedEntity);
                        for (int i = 0; i < renters.Length; i++)
                        {
                            if (renters[i].m_Renter != null)
                            {
                                Entity employerRenter = renters[i].m_Renter;
                                if (EntityManager.HasBuffer<Employee>(employerRenter))
                                {
                                    DynamicBuffer<Employee> employees = EntityManager.GetBuffer<Employee>(employerRenter);
                                    Mod.log.Info("Renter " + employerRenter.ToString());

                                    for (int j = 0; j < employees.Length; j++)
                                    {
                                        if (employees[j].m_Worker != null)
                                        {
                                            Mod.log.Info("Employee: " + employees[j].m_Worker.ToString());
                                            if (EntityManager.HasComponent<HouseholdMember>(employees[j].m_Worker))
                                            {
                                                HouseholdMember householdMember = EntityManager.GetComponentData<HouseholdMember>(employees[j].m_Worker);
                                                
                                                if (householdMember.m_Household != null)
                                                {
                                                    Mod.log.Info("Household: " + householdMember.m_Household.ToString());
                                                    if (EntityManager.HasComponent<PropertyRenter>(householdMember.m_Household))
                                                    {
                                                        PropertyRenter householdPropertyRenter =
                                                        EntityManager.GetComponentData<PropertyRenter>(householdMember.m_Household);
                                                        Mod.log.Info("Property Rente propertyr: " + householdPropertyRenter.m_Property.ToString());

                                                        if (!EntityManager.HasBuffer<ColorVariation>(householdPropertyRenter.m_Property))
                                                        {
															DynamicBuffer<ColorVariation> color = EntityManager.AddBuffer<ColorVariation>(householdPropertyRenter.m_Property);
													
															ColorSet colorSet = new ColorSet(new UnityEngine.Color(.5f, 0, 0, 1));
															ColorVariation colorVariation = new ColorVariation();
															colorVariation.m_ColorSet = colorSet;
                                                            colorVariation.m_Probability = 255;
                                                            colorVariation.m_ValueRange = 100;
															color.Add(colorVariation);
															color.Add(colorVariation);
															EntityManager.AddComponent<BatchesUpdated>(householdPropertyRenter.m_Property);
															EntityManager.AddComponent<EffectsUpdated>(householdPropertyRenter.m_Property);
														}

                                                        
                                                        if (EntityManager.HasBuffer<MeshColor>(householdPropertyRenter.m_Property))
                                                        {
															NativeArray<MeshColor> meshColors = EntityManager.AddBuffer<MeshColor>(householdPropertyRenter.m_Property).AsNativeArray();
                                                            MeshColor meshColor = meshColors[0];
															Mod.log.Info("Property Rente propertyr: " + meshColor.m_ColorSet.m_Channel0);
															meshColor.m_ColorSet = new ColorSet();
															meshColor.m_ColorSet.m_Channel0 = new UnityEngine.Color(1, 0, 0);
															meshColor.m_ColorSet.m_Channel1 = new UnityEngine.Color(1, 0, 0);
															meshColor.m_ColorSet.m_Channel2 = new UnityEngine.Color(1, 0, 0);
                                                            meshColors[0] = meshColor;
                                                            //meshColors

                                                            EntityManager.AddComponent<BatchesUpdated>(householdPropertyRenter.m_Property);
															EntityManager.AddComponent<EffectsUpdated>(householdPropertyRenter.m_Property);
                                                          
														}
                                                    }
                                                }

                                            }
                                        }
                                    }

                                }
                            }
                            }

                        }
                }
                /*NativeArray<Entity> selectedEntities = this.selectionQuery.ToEntityArray(Allocator.Temp);

                for (int i = 0; i < selectedEntities.Length; i++)
                {
                    if (this.selectedEntity == null || !this.selectedEntity.Equals(selectedEntities[i]))
                    {
                        this.selectedEntity = selectedEntities[i];
                        Mod.log.Info("Selected entity:" + this.selectedEntity.ToString());
                    }

                    Mod.log.Info("Selected entities " + selectedEntities[i].ToString());
                }*/
            }
            catch (Exception ex)
            {
                Mod.log.Info(ex, "Failed to query entity");
            }

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            
            return 1;
        }

        public override int GetUpdateOffset(SystemUpdatePhase phase)
        {
            return base.GetUpdateOffset(phase);
        }

        public override string ToString()
        {
            return base.ToString();
        }
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnFocusChanged(bool hasFocus)
        {
            base.OnFocusChanged(hasFocus);
        }

        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);
        }

        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
        }

        protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }
    }
}
