using Colossal.Entities;
using Game;
using Game.Buildings;
using Game.Common;
using Game.Events;
using Game.Objects;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;

namespace DifficultyConfig
{
	internal partial class CollapseCitySystem : GameSystemBase
	{
		private DifficultySettings settings;
		private CitySystem citySystem;
		private EntityQuery buildingQuery;
		private EntityQuery destroyedQuery;
		private EntityArchetype destroyArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.buildingQuery = GetEntityQuery(ComponentType.ReadWrite<Building>(), ComponentType.Exclude<OnFire>(), ComponentType.Exclude<Destroyed>(), ComponentType.Exclude<Native>());
			this.destroyedQuery = GetEntityQuery(ComponentType.ReadWrite<Destroyed>(), ComponentType.Exclude<Deleted>());
			this.destroyArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<Destroy>());
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			this.settings = Mod.INSTANCE.settings();
			this.citySystem = World.GetExistingSystemManaged<CitySystem>();
		}

		long frameCount = 0;

		protected override void OnUpdate()
		{
			if (EntityManager.TryGetComponent(this.citySystem.City, out CollapsingCity collapsingCity))
			{
				if (frameCount++ % collapsingCity.frameInterval == 0)
				{
					NativeArray<Entity> entities = this.buildingQuery.ToEntityArray(Allocator.Temp);

					for (int i = 0; i < 12 && i < entities.Length; i++)
					{
						Entity target = entities[i];
						Entity destroyEvent = EntityManager.CreateEntity(this.destroyArchetype);
						EntityManager.SetComponentData(destroyEvent, new Destroy(target, EntityManager.CreateEntity()));
						EntityManager.AddComponent<BatchesUpdated>(target);
						
					}

					entities.Dispose();
				}

				if (this.frameCount % 200 == 0)
				{
					NativeArray<Entity> maybeDeletes = this.destroyedQuery.ToEntityArray(Allocator.Temp);

					for (int i = 0; i < 150 && i < maybeDeletes.Length; ++i)
					{
						EntityManager.AddComponent<Deleted>(maybeDeletes[i]);
					}

					maybeDeletes.Dispose();
				}
			}	
		}
	}
}
