using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Events;
using Game.Objects;
using Game.Prefabs;
using Game.Vehicles;
using System;
using Unity.Entities;

namespace Pandemic
{
	internal class SicknessStarter
	{
		private PrefabID sicknessEventPrefab = new PrefabID("EventPrefab", "Generic Sickness");
		private PrefabSystem prefabSystem;
		private EntityManager EntityManager;
		private EntityArchetype addHealthProblemArchetype;

		public SicknessStarter(PrefabSystem prefabSystem, EntityManager entityManager)
		{
			this.prefabSystem = prefabSystem;
			this.EntityManager = entityManager;
			this.addHealthProblemArchetype = this.EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<AddHealthProblem>());
		}

		public void makeSick(Entity target)
		{
			Entity addSicknessProblem = EntityManager.CreateEntity(this.addHealthProblemArchetype);
			EntityManager.SetComponentData(addSicknessProblem, new AddHealthProblem()
			{
				m_Target = target,
				m_Event = Entity.Null,
				m_Flags = HealthProblemFlags.Sick | HealthProblemFlags.RequireTransport
			});

			EntityManager.AddComponent<BatchesUpdated>(target);
			EntityManager.AddComponent<EffectsUpdated>(target);
			EntityManager.AddComponent<BatchesUpdated>(addSicknessProblem);
			EntityManager.AddComponent<EffectsUpdated>(addSicknessProblem);
		}

		public void makeDanger(Entity target, uint currentFrame)
		{
			Entity sicknessEvent = EntityManager.CreateEntity();
			//EntityManager.AddComponent<PrefabRef>(sicknessEvent);
			EntityManager.AddComponent<Game.Events.Event>(sicknessEvent);
			EntityManager.AddComponent<Duration>(sicknessEvent);
			EntityManager.AddComponent<DangerLevel>(sicknessEvent);
			EntityManager.AddComponent<Simulate>(sicknessEvent);
			EntityManager.AddComponent<SicknessEventData>(sicknessEvent);

			Duration duration = new Duration() { m_StartFrame = currentFrame, m_EndFrame = currentFrame + 1000 };
			EntityManager.SetComponentData(sicknessEvent, duration);
			EntityManager.SetComponentData(sicknessEvent, new SicknessEventData() { duration = duration});
			EntityManager.SetComponentData(sicknessEvent, new DangerLevel() {m_DangerLevel=1f});

			EntityManager.AddComponent<InDanger>(target);
			EntityManager.SetComponentData<InDanger>(target, new InDanger()
			{
				m_EvacuationRequest = Entity.Null,
				m_Event = sicknessEvent,
				m_Flags = DangerFlags.Evacuate,
				m_EndFrame = currentFrame + 1000
			});

			EntityManager.AddComponent<BatchesUpdated>(sicknessEvent);
			EntityManager.AddComponent<BatchesUpdated>(target);
			EntityManager.AddComponent<EffectsUpdated>(target);
			EntityManager.AddComponent<Updated>(target);
		}

		/*private void CreateHealthEvent(int jobIndex, ref Random random, Entity targetEntity, Entity eventPrefab, Entity household, Citizen citizen, EventData eventData, HealthEventData healthData)
		{
			
			if (healthData.m_RequireTracking)
			{
				EntityManager.CreateEntity
				Entity e = m_CommandBuffer.CreateEntity(jobIndex, eventData.m_Archetype);
				m_CommandBuffer.SetComponent(jobIndex, e, new PrefabRef(eventPrefab));
				m_CommandBuffer.SetBuffer<TargetElement>(jobIndex, e).Add(new TargetElement(targetEntity));
				return;
			}

			HealthProblemFlags healthProblemFlags = HealthProblemFlags.None;
			switch (healthData.m_HealthEventType)
			{
				case HealthEventType.Disease:
					healthProblemFlags |= HealthProblemFlags.Sick;
					break;
				case HealthEventType.Injury:
					healthProblemFlags |= HealthProblemFlags.Injured;
					break;
				case HealthEventType.Death:
					healthProblemFlags |= HealthProblemFlags.Dead;
					break;
			}

			float num = math.lerp(healthData.m_TransportProbability.max, healthData.m_TransportProbability.min, (float)(int)citizen.m_Health * 0.01f);
			if (random.NextFloat(100f) < num)
			{
				healthProblemFlags |= HealthProblemFlags.RequireTransport;
			}

			float fee = ServiceFeeSystem.GetFee(PlayerResource.Healthcare, m_Fees[m_City]);
			int num2 = 0;
			if (m_CitizenBuffers.HasBuffer(household))
			{
				num2 = EconomyUtils.GetHouseholdIncome(m_CitizenBuffers[household], ref m_Workers, ref m_CitizenDatas, ref m_HealthProblems, ref m_EconomyParameters, m_TaxRates);
			}

			float num3 = 10f / (float)(int)citizen.m_Health - fee / 2f * (float)num2;
			if (random.NextFloat() < num3)
			{
				healthProblemFlags |= HealthProblemFlags.NoHealthcare;
			}

			Entity e2 = m_CommandBuffer.CreateEntity(jobIndex, m_AddProblemArchetype);
			m_CommandBuffer.SetComponent(jobIndex, e2, new AddHealthProblem
			{
				m_Event = Entity.Null,
				m_Target = targetEntity,
				m_Flags = healthProblemFlags
			});
		}*/
	}
}
