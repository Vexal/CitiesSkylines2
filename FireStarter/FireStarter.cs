using Game.Buildings;
using Game.Common;
using Game.Events;
using Game.Objects;
using Game.Prefabs;
using Game.Vehicles;
using Unity.Entities;

namespace FireStarter
{
	internal class FireStarter
	{
		private PrefabID buildingFirePrefab = new PrefabID("EventPrefab", "Building Fire");
		private PrefabID forestFirePrefab = new PrefabID("EventPrefab", "Forest Fire"); 
		private PrefabSystem prefabSystem;
		private EntityManager EntityManager;

		public FireStarter(PrefabSystem prefabSystem, EntityManager entityManager)
		{
			this.prefabSystem = prefabSystem;
			this.EntityManager = entityManager; 
		}

		public void createFire(Entity target)
		{
			var onFire = new OnFire();
			onFire.m_Intensity = 1000;
			EntityManager.AddComponent<OnFire>(target);
			 

			Entity prefabEntity;
			if (EntityManager.HasComponent<Tree>(target))
			{
				if (this.prefabSystem.TryGetPrefab(forestFirePrefab, out PrefabBase prefabBase))
				{
					this.prefabSystem.TryGetEntity(prefabBase, out prefabEntity);
				}
				else
				{
					return;
				}
			} 
			else if (EntityManager.HasComponent<Building>(target) || EntityManager.HasComponent<Vehicle>(target))
			{
				if (this.prefabSystem.TryGetPrefab(buildingFirePrefab, out PrefabBase prefabBase))
				{
					this.prefabSystem.TryGetEntity(prefabBase, out prefabEntity);
				}
				else
				{
					return;
				}
			}
            else
            {
				return;
            }

            Entity e = EntityManager.CreateEntity();
			EntityManager.AddComponent<PrefabRef>(e);
			EntityManager.AddComponent<Game.Events.Event>(e);
			EntityManager.AddComponent<Game.Events.Fire>(e);
			EntityManager.SetComponentData(e, new PrefabRef(prefabEntity));
			onFire.m_Event = e;
			EntityManager.SetComponentData<OnFire>(target, onFire);
			EntityManager.AddBuffer<TargetElement>(e).Add(new TargetElement(target));
			EntityManager.AddComponent<BatchesUpdated>(e);
			EntityManager.AddComponent<BatchesUpdated>(target);
			EntityManager.AddComponent<EffectsUpdated>(target);
			EntityManager.AddComponent<Updated>(target);
		}
	}
}
