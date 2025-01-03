using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Settings;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityQuery healthyDiseaseEntityQuery;
		private EntityQuery unhealthyDiseaseEntityQuery;
		private EntityQuery healthProblemEntityQuery;
		private EntityQuery diseaseEntityQuery;

		private PrefabSystem prefabSystem;
		private SimulationSystem simulationSystem;

		private PrefabID sicknessEventPrefabId = new PrefabID("EventPrefab", "Generic Sickness");
		private PrefabID policyPrefabId = new PrefabID("PolicyTogglePrefab", "PreRelease Programs");
		private Entity sicknessEventPrefabEntity;
		private PrefabBase policyPrefabEntity;
		private EntityArchetype sickEventArchetype;

		private PrefabID suddenDeathPrefabId = new PrefabID("EventPrefab", "Sudden Death");
		private Entity suddenDeathPrefabEntity;
		private EntityArchetype deathEventArchetype;
		private EntityArchetype resetTripArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
			this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());

			if (this.prefabSystem.TryGetPrefab(sicknessEventPrefabId, out PrefabBase prefabBase))
			{
				//this.prefabSystem.TryGetEntity(prefabBase, out this.sicknessEventPrefabEntity);
				//this.sickEventArchetype = EntityManager.GetComponentData<EventData>(this.sicknessEventPrefabEntity).m_Archetype;

				PrefabBase pandemicPrefab = prefabBase.Clone("Pandemic Sickness");
				Game.Prefabs.HealthEvent healthEvent = pandemicPrefab.GetComponent<Game.Prefabs.HealthEvent>();
				healthEvent.m_RequireTracking = true;
				/*pandemicPrefab.Remove(typeof(Game.Prefabs.HealthEvent));
				pandemicPrefab.AddComponent<Game.Prefabs.HealthEvent>();
				pandemicPrefab.AddComponentFrom<Game.Prefabs.HealthEvent>();*/
				prefabBase.AddComponentFrom(healthEvent);
				if (this.prefabSystem.AddPrefab(pandemicPrefab))
				{
					if (this.prefabSystem.TryGetEntity(pandemicPrefab, out this.sicknessEventPrefabEntity))
					{
						if (this.prefabSystem.TryGetEntity(prefabBase, out var e))
						{
							EventData eventData = EntityManager.GetComponentData<EventData>(e);
							this.sickEventArchetype = eventData.m_Archetype;
						}
					}
					else
					{
						Mod.log.Info("Failed got sick archetype 2");
					}
				}
				else
				{
					Mod.log.Info("Failed to get sick archetype");
				}
			}

			if (this.prefabSystem.TryGetPrefab(this.suddenDeathPrefabId, out PrefabBase prefabBase2))
			{
				this.prefabSystem.TryGetEntity(prefabBase2, out this.suddenDeathPrefabEntity);
				this.deathEventArchetype = EntityManager.GetComponentData<EventData>(this.suddenDeathPrefabEntity).m_Archetype;
			}

			this.prefabSystem.TryGetPrefab(this.policyPrefabId, out this.policyPrefabEntity);

			this.healthyDiseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Contagious>(),
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});

			this.unhealthyDiseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Contagious>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<HealthProblem>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.healthProblemEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<HealthProblem>(),
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Contagious>(),
				}
			});

			this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Contagious>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<HealthProblem>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				}
			});

			PrefabBase pr = this.policyPrefabEntity.Clone("Mask Mandate");
			pr.Remove(typeof(CityModifiers));
			pr.Remove(typeof(Unlockable));
			pr.AddComponent<CityModifiers>();
			pr.GetComponent<CityModifiers>().m_Modifiers = new CityModifierInfo[2];
			pr.GetComponent<CityModifiers>().m_Modifiers[0] = new CityModifierInfo();
			pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Type = CityModifierType.Entertainment;
			pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Mode = ModifierValueMode.Relative;
			pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Range = new Colossal.Mathematics.Bounds1(new float2() {x = 15.5f, y = 15.5f });

			pr.GetComponent<CityModifiers>().m_Modifiers[1] = new CityModifierInfo();
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Type = CityModifierType.DiseaseProbability;
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Mode = ModifierValueMode.Relative;
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Range = new Colossal.Mathematics.Bounds1(new float2() {x = 100f, y = 100f });
			this.prefabSystem.AddPrefab(pr);
			Mod.log.Info("asset: " + pr.ToString() + " ; " + pr.asset?.name + " ; " + pr.asset?.ToString() + " ; " + pr.asset?.uniqueName);
			Mod.log.Info("original asset: " + this.policyPrefabEntity.asset?.name + " ; " + this.policyPrefabEntity.asset?.ToString() + " ; " + this.policyPrefabEntity.asset?.uniqueName);
			this.prefabSystem.AddComponentData(pr, new CityOptionData() { m_OptionMask = PandemicSpreadSystem.MASK_MANDATE_MASK });
			foreach (string s in GameManager.instance.localizationManager.activeDictionary.entryIDs)
			{
				if (s.Contains("Policy."))
				{
					Mod.log.Info("policy: " + s);
				}
			}
		}

		private const uint PROGRESSION_FRAME_COUNT = 300;
		private const byte MAX_DEATH_HEALTH = 15;

		protected override void OnUpdate()
		{
			this.removeDiseaseFromHealthy();
			this.addDiseaseToSick();
			if (this.shouldProgressDisease())
			{
				this.progressHealthProblems();
			}
		}

		private void removeDiseaseFromHealthy()
		{
			EntityManager.RemoveComponent<Contagious>(this.healthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp));
			NativeArray<Entity> citizens = this.unhealthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
			for (int i = 0; i < citizens.Length; ++i)
			{
				if (!isSick(healthProblems[i].m_Flags) || this.isInHospital(citizens[i]))
				{
					EntityManager.RemoveComponent<Contagious>(citizens[i]);
				}
			}
		}

		private void addDiseaseToSick()
		{
			NativeArray<Entity> citizens = this.healthProblemEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.healthProblemEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
			for (int i = 0; i < citizens.Length; ++i)
			{
				//this.makeCitizenSick(citizen);
				if (isSick(healthProblems[i].m_Flags) && !this.isInHospital(citizens[i]))
				{
					EntityManager.AddComponent<Contagious>(citizens[i]);
					if (EntityManager.TryGetComponent<CurrentTransport>(citizens[i], out var transport))
					{

						/*EntityManager.AddComponentData(EntityManager.CreateEntity(this.resetTripArchetype), new ResetTrip
						{
							m_Creature = transport.m_CurrentTransport,
							m_Target = Entity.Null,
							m_DivertPurpose = Purpose.Hospital
							//m_DivertPurpose = Purpose.Hospital
						});*/
					}
				}
			}
		}

		private void progressHealthProblems()
		{
			NativeArray<Entity> citizens = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<Citizen> citizenData = this.diseaseEntityQuery.ToComponentDataArray<Citizen>(Allocator.Temp);

			if (citizens.Length > 0) {
				byte healthPenalty = this.getHealthDecreaseAmount();
				int suddenDeathChance = Mod.INSTANCE.m_Setting.suddenDeathChance;
				for (int i = 0; i < citizens.Length; ++i)
				{
					Citizen c = citizenData[i];
					if (healthPenalty > 0)
					{
						if (c.m_Health <= healthPenalty)
						{
							c.m_Health = 0;
						}
						else
						{
							c.m_Health -= healthPenalty;
							EntityManager.SetComponentData(citizens[i], c);
						}
					}

					if (suddenDeathChance > 0 && c.m_Health <= MAX_DEATH_HEALTH &&
						UnityEngine.Random.Range(0, 100) <= suddenDeathChance)
					{
						this.killCitizen(citizens[i]);
					}
				}
			}
		}

		private void decreaseCitizenHealth(Entity citizenEntity, Citizen citizenData, byte amount)
		{
			if (citizenData.m_Health <= amount)
			{
				citizenData.m_Health = 0;
			}
			else
			{
				citizenData.m_Health -= amount;
				EntityManager.SetComponentData(citizenEntity, citizenData);
			}
		}

		public void makeCitizenSick(Entity targetCitizen)
		{
			//Mod.log.Info("sick archetype " + this.sickEventArchetype.ToString());
			Entity eventEntity = EntityManager.CreateEntity(this.sickEventArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(this.sicknessEventPrefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = targetCitizen });
			EntityManager.AddComponent<DiseaseId>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new DiseaseId() { diseaseId = 1 });

		}

		public byte getHealthDecreaseAmount()
		{
			switch (Mod.INSTANCE.m_Setting.diseaseProgressionSpeed)
			{
				case PandemicSettings.DiseaseProgression.Minor:
					return 5;
				case PandemicSettings.DiseaseProgression.Moderate:
					return 10;
				case PandemicSettings.DiseaseProgression.Severe:
					return 15;
				case PandemicSettings.DiseaseProgression.Extreme:
					return 25;
				default:
					return 0;
			}
		}

		private void killCitizen(Entity target)
		{
			Entity eventEntity = EntityManager.CreateEntity(this.deathEventArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(this.suddenDeathPrefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = target });
		}

		private bool shouldProgressDisease()
		{
			return this.simulationSystem.frameIndex % PROGRESSION_FRAME_COUNT == 0;
		}

		public static bool isSick(HealthProblemFlags flags)
		{
			return (flags & HealthProblemFlags.Sick) > 0 && (flags & HealthProblemFlags.Dead) == 0;
		}

		private bool isInHospital(Entity citizen)
		{
			return EntityManager.TryGetComponent<CurrentBuilding>(citizen, out var building) && EntityManager.HasComponent<Game.Buildings.Hospital>(building.m_CurrentBuilding);
		}
	}
}
