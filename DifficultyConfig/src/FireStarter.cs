using Game.Common;
using Game.Events;
using Game.Objects;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine.Rendering;

namespace DifficultyConfig
{
	internal class FireStarter
	{
		private PrefabID prefabID = new PrefabID("EventPrefab", "Building Fire");
		private PrefabSystem m_PrefabSystem;
		private EntityManager EntityManager;

		public FireStarter(PrefabSystem prefabSystem, EntityManager entityManager)
		{
			this.m_PrefabSystem = prefabSystem;
			this.EntityManager = entityManager;
		}

		public void createFire(Entity target)
		{

			if (m_PrefabSystem.TryGetPrefab(prefabID, out PrefabBase prefabBase))
			{
				var onFire = new OnFire();
				onFire.m_Intensity = 10;
				EntityManager.AddComponent<OnFire>(target);

				Entity prefabEntity;
				m_PrefabSystem.TryGetEntity(prefabBase, out prefabEntity);
				EventData ed;
				Entity e = EntityManager.CreateEntity();
				EntityManager.AddComponent<PrefabRef>(e);
				EntityManager.SetComponentData(e, new PrefabRef(prefabEntity));
				onFire.m_Event = e;
				EntityManager.SetComponentData(target, onFire);

				var damaged = new Damaged();
				damaged.m_Damage = 1f;
				//EntityManager.AddComponentData(target, damaged);
				/*Entity e = EntityManager.CreateEntity();
				EntityManager.SetComponentData(e, new PrefabRef(prefabEntity));
				//m_CommandBuffer.SetBuffer<TargetElement>(jobIndex, e).Add(new TargetElement(targetEntity));

				onFire.m_Event = e;
				EntityManager.AddBuffer<TargetElement>(e).Add(new TargetElement(this.selectedEntity));
				EntityManager.AddComponent<BatchesUpdated>(this.selectedEntity);
				EntityManager.AddComponent<BatchesUpdated>(e);*/

				/*Entity destroyEvent = EntityManager.CreateEntity(destroyArchetype);
				EntityManager.SetComponentData(destroyEvent, new Destroy(target, onFire.m_Event));

				EntityManager.AddComponent<BatchesUpdated>(e);
				EntityManager.AddComponent<BatchesUpdated>(target);*/
			}


		}
	}
}
