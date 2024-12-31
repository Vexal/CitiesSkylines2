using Colossal.Entities;
using Game;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Objects;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pandemic
{
	internal partial class PandemicSpreadSystem : GameSystemBase
	{
		private SimulationSystem simulationSystem;
		private EntityQuery diseaseCitizenHumanEntityQuery;
		private EntityQuery healthyCitizenQuery;
		private EntityArchetype resetTripArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
			this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());

			this.diseaseCitizenHumanEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentTransport>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Unspawned>(),
				}
			});


			this.healthyCitizenQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentTransport>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Unspawned>(),
				ComponentType.ReadOnly<Disease>()
				}
			});
		}
		protected override void OnUpdate()
		{
			this.handleDiseaseSpread();
			
		}

		private void handleDiseaseSpread()
		{
			if (this.simulationSystem.frameIndex % Mod.INSTANCE.m_Setting.diseaseSpreadInterval != 0)
			{
				return;
			}

			NativeArray<CurrentTransport> diseasedTransports = this.diseaseCitizenHumanEntityQuery.ToComponentDataArray<CurrentTransport>(Allocator.Temp);
			NativeList<float3> diseasePositions = new NativeList<float3>(Allocator.TempJob);

			foreach (CurrentTransport t in diseasedTransports)
			{
				if (!EntityManager.HasComponent<CurrentVehicle>(t.m_CurrentTransport) &&
					EntityManager.TryGetComponent<Transform>(t.m_CurrentTransport, out var transform))
				{
					diseasePositions.Add(transform.m_Position);
				}
			}

			if (diseasePositions.Length > 0)
			{
				NativeArray<CurrentTransport> citizenTransports = this.healthyCitizenQuery.ToComponentDataArray<CurrentTransport>(Allocator.Temp);
				NativeArray<Entity> citizens = this.healthyCitizenQuery.ToEntityArray(Allocator.Temp);

				NativeList<float3> citizenPositions = new NativeList<float3>(Allocator.TempJob);

				for (int i = 0; i < citizens.Length; ++i)
				{
					CurrentTransport t = citizenTransports[i];
					if ((!EntityManager.TryGetComponent<TravelPurpose>(citizens[i]) || !EntityManager.HasComponent<CurrentVehicle>(t.m_CurrentTransport) &&
						EntityManager.TryGetComponent<Transform>(t.m_CurrentTransport, out var transform))
					{
						citizenPositions.Add(transform.m_Position);
					}
				}

				if (citizenPositions.Length > 0)
				{
					SpreadDiseaseJob job = new SpreadDiseaseJob();
					job.diseasePositions = diseasePositions.AsArray();
					job.citizenPositions = citizenPositions.AsArray();
					job.spreadRadius = Mod.INSTANCE.m_Setting.diseaseSpreadRadius;
					job.fleeRadius = job.spreadRadius + Mod.INSTANCE.m_Setting.diseaseFleeRadius;
					job.spreadChance = Mod.INSTANCE.m_Setting.diseaseSpreadChance;
					job.spread = new NativeArray<bool>(citizenPositions.Length, Allocator.TempJob);
					job.flee = new NativeArray<bool>(citizenPositions.Length, Allocator.TempJob);
					var jobHandle = job.ScheduleBatch(citizenPositions.Length, 100);

					diseasePositions.Dispose(jobHandle);
					citizenPositions.Dispose(jobHandle);
					jobHandle.Complete();

					int spreadCount = 0;
					for (int i = 0; i < job.spread.Length; ++i)
					{
						if (job.spread[i] && spreadCount++ < Mod.INSTANCE.m_Setting.maxDiseaseSpreadPerFrame)
						{
							EntityManager.AddComponent<Disease>(citizens[i]);
						}
						else if (job.flee[i] && (travelPurpose[i].m_Purpose & Purpose.GoingHome) == 0)
						{
							Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
							EntityManager.AddComponentData(e, new ResetTrip
							{
								m_Creature = citizenTransports[i].m_CurrentTransport,
								m_Target = Entity.Null,
								//m_DivertPurpose = Purpose.Hospital
							});
						}
					}

					job.spread.Dispose();
					job.flee.Dispose();
				}
				else
				{
					citizenPositions.Dispose();
				}
			}
			else
			{
				diseasePositions.Dispose();
			}
		}
	}
}
