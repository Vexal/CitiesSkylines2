using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace EmploymentTracker
{
	internal class BuildingSelectJob : IJobChunk
	{
		public EntityCommandBuffer.ParallelWriter buffer;
		public Entity selectedEntity;
        public EntityManager EntityManager;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			/*if (EntityManager.HasBuffer<Renter>(this.selectedEntity))
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
													
															ColorSet colorSet = new ColorSet(new UnityEngine.Color(1, 0, 0, 1));
															ColorVariation colorVariation = new ColorVariation();
															colorVariation.m_ColorSet = colorSet;
                                                            colorVariation.m_Probability = 255;
															color.Add(colorVariation);
														}

                                                        
                                                        if (EntityManager.HasBuffer<MeshColor>(householdPropertyRenter.m_Property))
                                                        {
                                                            // DynamicBuffer<MeshColor> meshColors = EntityManager.AddBuffer<MeshColor >(householdPropertyRenter.m_Property);
                                                            //var commander = World.GetExistingSystemManaged<EntityCommandBufferSystem>();
                                                            //var commandBuffer = commander.CreateCommandBuffer();
                                                            var commandBuffer = World.GetOrCreateSystemManaged<EndFrameBarrier>().CreateCommandBuffer().AsParallelWriter();
															DynamicBuffer<MeshColor> meshColors = commandBuffer.AddBuffer<MeshColor>(0, householdPropertyRenter.m_Property);
                                                            MeshColor meshColor = meshColors[0];
															Mod.log.Info("Property Rente propertyr: " + meshColor.m_ColorSet.m_Channel0);
															meshColor.m_ColorSet = new ColorSet();
															meshColor.m_ColorSet.m_Channel0 = new UnityEngine.Color(1, 0, 0);
															meshColor.m_ColorSet.m_Channel1 = new UnityEngine.Color(1, 0, 0);
															meshColor.m_ColorSet.m_Channel2 = new UnityEngine.Color(1, 0, 0);
                                                            meshColors[0] = meshColor;
                                                            meshColors.Add(meshColor);
                                                            //commandBuffer.add
															//Mod.log.Info("Property Rente property 2r: " + meshColor.m_ColorSet.m_Channel1);
															//EntityManager.GetBuffer<MeshColor>(householdPropertyRenter.m_Property)[0].m_ColorSet.m_Channel1);
														}

                                                    }
                                                }

                                            }
                                        }
                                    }

                                }
                            }
                            }

                        }*/
		}
	}
}
