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
using System.Runtime.Remoting.Lifetime;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityQuery healthyDiseaseQuery;
		private EntityQuery unhealthyDiseaseEntityQuery;
		private EntityQuery healthProblemEntityQuery;
		private EntityQuery diseaseEntityQuery;

		private PrefabSystem prefabSystem;
		private SimulationSystem simulationSystem;
		private TimeSystem timeSystem;

		private PrefabID sicknessEventPrefabId = new PrefabID("EventPrefab", "Generic Sickness");
		private PrefabID policyPrefabId = new PrefabID("PolicyTogglePrefab", "PreRelease Programs");
		private Entity sicknessEventPrefabEntity;
		private PrefabBase policyPrefabEntity;
		private EntityArchetype sickEventArchetype;

		private PrefabID suddenDeathPrefabId = new PrefabID("EventPrefab", "Sudden Death");
		private Entity suddenDeathPrefabEntity;
		private EntityArchetype deathEventArchetype;
		private EntityArchetype resetTripArchetype;
		private EntityArchetype diseaseArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
			this.timeSystem = World.GetOrCreateSystemManaged<TimeSystem>();
			this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());
			this.diseaseArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Disease>());

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


			this.healthyDiseaseQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<CurrentDisease>(),
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});

			this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
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
				ComponentType.ReadOnly<CurrentDisease>(),
				}
			});

			this.unhealthyDiseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<HealthProblem>(),
				ComponentType.ReadOnly<CurrentDisease>(),
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			PrefabBase pr = this.policyPrefabEntity.Clone("Mask Mandate");
			pr.Remove(typeof(CityModifiers));
			pr.Remove(typeof(Unlockable));
			pr.AddComponent<CityModifiers>();
			pr.GetComponent<CityModifiers>().m_Modifiers = new CityModifierInfo[1];
			pr.GetComponent<CityModifiers>().m_Modifiers[0] = new CityModifierInfo();
			pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Type = CityModifierType.Entertainment;
			pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Mode = ModifierValueMode.Relative;
			pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Range = new Colossal.Mathematics.Bounds1(new float2() {x = 15.5f, y = 15.5f });

			/*pr.GetComponent<CityModifiers>().m_Modifiers[1] = new CityModifierInfo();
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Type = CityModifierType.DiseaseProbability;
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Mode = ModifierValueMode.Relative;
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Range = new Colossal.Mathematics.Bounds1(new float2() {x = 100f, y = 100f });*/
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
			EntityManager.RemoveComponent<CurrentDisease>(this.healthyDiseaseQuery.ToEntityArray(Allocator.Temp));
			NativeArray<Entity> citizens = this.unhealthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
			for (int i = 0; i < citizens.Length; ++i)
			{
				if (!isSick(healthProblems[i].m_Flags))
				{
					EntityManager.RemoveComponent<CurrentDisease>(citizens[i]);
				}
			}
		}

		private void addDiseaseToSick()
		{
			NativeArray<Entity> citizens = this.healthProblemEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.healthProblemEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
			for (int i = 0; i < citizens.Length; ++i)
			{
				if (!isSick(healthProblems[i].m_Flags))
				{
					continue;
				}

				Entity disease = Entity.Null;
				Entity healthEvent = healthProblems[i].m_Event;

				if (EntityManager.Exists(healthEvent) && EntityManager.TryGetComponent<DiseaseRef>(healthEvent, out var diseaseRef))
				{
					disease = diseaseRef.disease;
				}

				disease = this.createOrMutateDisease(disease, out var diseaseDefinition);

				if (!EntityManager.TryGetComponent<LastDisease>(citizens[i], out var lastDisease))
				{
					lastDisease = new LastDisease();
					EntityManager.AddComponent<LastDisease>(citizens[i]);
				}

				switch (diseaseDefinition.type)
				{
					case 1:
						lastDisease.lastCold = disease;
						break;
					case 2:
						lastDisease.lastFlu = disease;
						break;
				}

				diseaseDefinition.infectionCount++;

				EntityManager.SetComponentData(disease, diseaseDefinition);

				EntityManager.SetComponentData(citizens[i], lastDisease);

				EntityManager.AddComponent<CurrentDisease>(citizens[i]);
				EntityManager.SetComponentData(citizens[i], new CurrentDisease() { disease = disease, progression = 0f });
				
			}
		}

		private void progressHealthProblems()
		{
			NativeArray<Entity> citizens = this.unhealthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<Citizen> citizenData = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<Citizen>(Allocator.Temp);

			if (citizens.Length > 0) {
				byte healthPenalty = this.getHealthDecreaseAmount();
				int suddenDeathChance = Mod.INSTANCE.m_Setting.suddenDeathChance;
				for (int i = 0; i < citizens.Length; ++i)
				{
					Citizen c = citizenData[i];
					if (this.isInHospital(citizens[i]))
					{
						continue;
					}

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

		public void makeCitizenSick(Entity targetCitizen, Entity disease)
		{
			Entity eventEntity = EntityManager.CreateEntity(this.sickEventArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(this.sicknessEventPrefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = targetCitizen });
			EntityManager.AddComponent<DiseaseRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new DiseaseRef() { disease = disease });

		}

		private static DateTime EPOCH = new DateTime(1970, 1, 1);

		public Entity createOrMutateDisease(Entity prev, out Disease disease)
		{
			if (prev == Entity.Null)
			{
				return this.getOrCreateRandomDisease(out disease);
			}
			else
			{
				disease = EntityManager.GetComponentData<Disease>(prev);
				return prev;
			}
		}

		private Entity getOrCreateRandomDisease(out Disease disease)
		{
			NativeArray<Entity> diseases = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
			if (diseases.Length > 0)
			{
				disease = EntityManager.GetComponentData<Disease>(diseases[0]);
				return diseases[0];
			}
			else
			{
				Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
				DateTime date = this.timeSystem.GetCurrentDateTime();
				long unixTs = (long)date.Subtract(EPOCH).TotalMilliseconds;
				disease = new Disease()
				{
					type = 1,
					baseDeathChance = (Mod.settings.suddenDeathChance / 100f),
					baseHealthPenalty = this.getHealthDecreaseAmount(),
					baseSpreadChance = Mod.settings.diseaseSpreadChance,
					baseSpreadRadius = Mod.settings.diseaseSpreadRadius,
					maxDeathHealth = MAX_DEATH_HEALTH,
					createYear = date.Year,
					createMonth = date.Month,
					createHour = date.Hour,
					createMinute = date.Minute,
					ts = unixTs
				};

				EntityManager.SetComponentData(newDisease, disease);
				return newDisease;
			}
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

		private void resetTrip(Entity citizen)
		{
			if (EntityManager.TryGetComponent<CurrentTransport>(citizen, out var transport))
			{
				EntityManager.AddComponentData(EntityManager.CreateEntity(this.resetTripArchetype), new ResetTrip
				{
					m_Creature = transport.m_CurrentTransport,
					m_Target = Entity.Null,
					//m_DivertPurpose = Purpose.Hospital
					//m_DivertPurpose = Purpose.Hospital
				});
			}
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

		protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);
			EntityManager.AddComponent<Deleted>(this.diseaseEntityQuery);
		}
	}
}
