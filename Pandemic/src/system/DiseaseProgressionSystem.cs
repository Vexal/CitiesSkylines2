using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityQuery diseaseEntityQuery;
		private EntityQuery healthProblemEntityQuery;

		private PrefabSystem prefabSystem;

		private PrefabID sicknessEventPrefabId = new PrefabID("EventPrefab", "Generic Sickness");
		private EntityArchetype sickEventArchetype;
		private Entity sicknessEventPrefabEntity;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

			if (this.prefabSystem.TryGetPrefab(sicknessEventPrefabId, out PrefabBase prefabBase))
			{
				this.prefabSystem.TryGetEntity(prefabBase, out this.sicknessEventPrefabEntity);
				this.sickEventArchetype = EntityManager.GetComponentData<EventData>(this.sicknessEventPrefabEntity).m_Archetype;
			}

			this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
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
				ComponentType.ReadOnly<Temp>()
				}
			});
		}

		protected override void OnUpdate()
		{
			this.applyHealthProblems();
		}

		private void applyHealthProblems()
		{
			NativeArray<Entity> citizens = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity citizen in citizens)
			{
				this.makeCitizenSick(citizen);
			}
		}

		private void progressHealthProblems()
		{
			NativeArray<Entity> citizens = this.healthProblemEntityQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity citizen in citizens)
			{
				this.makeCitizenSick(citizen);
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
	}
}
