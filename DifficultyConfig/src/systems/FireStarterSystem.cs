using Colossal.Entities;
using Game;
using Game.Buildings;
using Game.City;
using Game.Common;
using Game.Events;
using Game.Objects;
using Game.Prefabs;
using Game.Simulation;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Rendering;

namespace DifficultyConfig
{
	internal partial class FireStarterSystem : GameSystemBase
	{
		private DifficultySettings settings;
		private FireStarter fireStarter;
		private CitySystem citySystem;
		private EntityQuery flammableQuery;
		private EntityQuery onFireQuery;

		protected override void OnCreate()
		{
			base.OnCreate();
			var m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
			this.fireStarter = new FireStarter(m_PrefabSystem, EntityManager);

			this.citySystem = World.GetExistingSystemManaged<CitySystem>();
			this.flammableQuery = GetEntityQuery(ComponentType.ReadWrite<Building>(), ComponentType.Exclude<OnFire>(), ComponentType.Exclude<Native>());
			this.onFireQuery = GetEntityQuery(ComponentType.ReadOnly<OnFire>());
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			this.settings = Mod.INSTANCE.settings();
		}
		long frameCount = 0;

		protected override void OnUpdate()
		{
			if (this.citySystem != null && EntityManager.Exists(this.citySystem.City) && EntityManager.TryGetComponent(this.citySystem.City, out BurningCity burningCity))
			{
				if (this.frameCount++ % 60 != 0)
				{
					return;
				}

				if (this.onFireQuery.CalculateEntityCount() < burningCity.maxBuildingCount)
				{
					NativeArray<Entity> entities = this.flammableQuery.ToEntityArray(Allocator.Temp);

					for (int i = 0; i < burningCity.maxBuildingCount && i < entities.Length; i++)
					{
						this.fireStarter.createFire(entities[i]);
					}

					entities.Dispose();
				}
			}	
		}
	}
}
