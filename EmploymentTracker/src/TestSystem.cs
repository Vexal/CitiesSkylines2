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

        Entity selectedEntity;
		private EndFrameBarrier endFrameBarrier;

		protected override void OnCreate()
        {
            base.OnCreate();
           // this.endFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();

        }

        byte count = 0;
        protected override void OnUpdate()
        {
			try
            {
                Mod.log.Info("Started here");
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
															DynamicBuffer<ColorVariation> color = buffer.SetBuffer<ColorVariation>(householdPropertyRenter.m_Property);
													
															ColorSet colorSet = new ColorSet(new UnityEngine.Color(.5f, 0, 0, 1));
															ColorVariation colorVariation = new ColorVariation();
															colorVariation.m_ColorSet = colorSet;
                                                            colorVariation.m_Probability = 255;
                                                            colorVariation.m_ValueRange = 100;
															color.Add(colorVariation);
															color.Add(colorVariation);
														}

                                                        Mod.log.Info("Logged rent");
                                                        
                                                        if (EntityManager.HasBuffer<MeshColor>(householdPropertyRenter.m_Property))
                                                        {
															DynamicBuffer<MeshColor> meshColors = buffer.SetBuffer<MeshColor>(householdPropertyRenter.m_Property);
                                                            MeshColor meshColor = new MeshColor();
															Mod.log.Info("Property Rente propertyr: " + meshColor.m_ColorSet.m_Channel0);
															meshColor.m_ColorSet = new ColorSet();
															meshColor.m_ColorSet.m_Channel0 = new UnityEngine.Color(1, 0, 0);
															meshColor.m_ColorSet.m_Channel1 = new UnityEngine.Color(1, 0, 0);
															meshColor.m_ColorSet.m_Channel2 = new UnityEngine.Color(1, 0, 0);
                                                            meshColors.Add(meshColor);
															//meshColors

														}

														buffer.AddComponent<BatchesUpdated>(householdPropertyRenter.m_Property);
														buffer.AddComponent<EffectsUpdated>(householdPropertyRenter.m_Property);
														buffer.AddComponent<Updated>(householdPropertyRenter.m_Property);
													}

                                                    Mod.log.Info("Got here");
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
