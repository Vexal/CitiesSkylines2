using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.Settings;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine.InputSystem;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityQuery healthyDiseaseEntityQuery;
		private EntityQuery healthProblemEntityQuery;
		private EntityQuery diseaseEntityQuery;

		private PrefabSystem prefabSystem;
		private SimulationSystem simulationSystem;

		private PrefabID sicknessEventPrefabId = new PrefabID("EventPrefab", "Generic Sickness");
		private Entity sicknessEventPrefabEntity;
		private EntityArchetype sickEventArchetype;

		private PrefabID suddenDeathPrefabId = new PrefabID("EventPrefab", "Sudden Death");
		private Entity suddenDeathPrefabEntity;
		private EntityArchetype deathEventArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

			if (this.prefabSystem.TryGetPrefab(sicknessEventPrefabId, out PrefabBase prefabBase))
			{
				this.prefabSystem.TryGetEntity(prefabBase, out this.sicknessEventPrefabEntity);
				this.sickEventArchetype = EntityManager.GetComponentData<EventData>(this.sicknessEventPrefabEntity).m_Archetype;
			}

			if (this.prefabSystem.TryGetPrefab(this.suddenDeathPrefabId, out PrefabBase prefabBase2))
			{
				this.prefabSystem.TryGetEntity(prefabBase2, out this.suddenDeathPrefabEntity);
				this.deathEventArchetype = EntityManager.GetComponentData<EventData>(this.suddenDeathPrefabEntity).m_Archetype;
			}

			this.healthyDiseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<HealthProblem>(),
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
				ComponentType.ReadOnly<Disease>(),
				}
			});

			this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<HealthProblem>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				}
			});
		}

		private const uint PROGRESSION_FRAME_COUNT = 64;
		private const byte MAX_DEATH_HEALTH = 15;

		protected override void OnUpdate()
		{
			this.applyHealthProblems();
			if (this.shouldProgressDisease())
			{
				this.progressHealthProblems();
			}
		}

		private void applyHealthProblems()
		{
			NativeArray<Entity> citizens = this.healthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity citizen in citizens)
			{
				this.makeCitizenSick(citizen);
			}
		}

		private void progressHealthProblems()
		{
			NativeArray<Entity> citizens = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<Citizen> citizenData = this.diseaseEntityQuery.ToComponentDataArray<Citizen>(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.diseaseEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);

			Random rnd = new Random();

			if (citizens.Length > 0) {
				byte healthPenalty = this.getHealthDecreaseAmount();
				int suddenDeathChance = Mod.INSTANCE.m_Setting.suddenDeathChance;
				for (int i = 0; i < citizens.Length; ++i)
				{
					if (healthProblems[i].m_Flags.HasFlag(HealthProblemFlags.Dead))
					{
						continue;
					}

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
						rnd.Next(100) <= suddenDeathChance)
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
			Entity eventEntity = EntityManager.CreateEntity(this.sickEventArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(this.sicknessEventPrefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = targetCitizen });
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
	}
}
