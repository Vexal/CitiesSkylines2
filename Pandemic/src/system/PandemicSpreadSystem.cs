using Colossal.Entities;
using Game;
using Game.Citizens;
using Game.City;
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
		private DiseaseProgressionSystem diseaseProgressionSystem;
		private EntityQuery diseaseCitizenHumanEntityQuery;
		private EntityQuery healthyCitizenQuery;
		private EntityQuery cityQuery;
		private EntityArchetype resetTripArchetype;

		public float maskModifier;
		public const uint MASK_MANDATE_MASK = 1024;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
			this.diseaseProgressionSystem = World.GetOrCreateSystemManaged<DiseaseProgressionSystem>();
			this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());
			this.diseaseCitizenHumanEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Contagious>(),
				ComponentType.ReadOnly<HealthProblem>(),
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
				ComponentType.ReadOnly<Contagious>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});

			this.cityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<City>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.refreshMaskModifier();
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

			this.refreshMaskModifier();

			NativeArray<CurrentTransport> diseasedTransports = this.diseaseCitizenHumanEntityQuery.ToComponentDataArray<CurrentTransport>(Allocator.Temp);
			NativeList<float3> diseasePositions = new NativeList<float3>(Allocator.TempJob);
			NativeList<float> diseaseRadius = new NativeList<float>(Allocator.TempJob);

			float baseRadius = Mod.INSTANCE.m_Setting.diseaseSpreadRadius;
			for (int i = 0; i < diseasedTransports.Length; ++i)
			{
				CurrentTransport t = diseasedTransports[i];
				if (!EntityManager.HasComponent<CurrentVehicle>(t.m_CurrentTransport) &&
					EntityManager.TryGetComponent<Transform>(t.m_CurrentTransport, out var transform))
				{
					diseasePositions.Add(transform.m_Position);
					//diseaseRadius.Add(this.diseaseProgressionSystem.)
				}
			}

			if (diseasePositions.Length > 0)
			{
				NativeArray<CurrentTransport> citizenTransports = this.healthyCitizenQuery.ToComponentDataArray<CurrentTransport>(Allocator.Temp);
				NativeArray<Entity> citizens = this.healthyCitizenQuery.ToEntityArray(Allocator.Temp);

				NativeList<float3> citizenPositions = new NativeList<float3>(Allocator.TempJob);
				NativeList<int> citizenIndexes = new NativeList<int>(Allocator.Temp);

				for (int i = 0; i < citizens.Length; ++i)
				{
					CurrentTransport t = citizenTransports[i];
					if (
						!EntityManager.HasComponent<CurrentVehicle>(t.m_CurrentTransport) &&
						EntityManager.TryGetComponent<Transform>(t.m_CurrentTransport, out var transform))
					{
						citizenPositions.Add(transform.m_Position);
						citizenIndexes.Add(i);
					}
				}

				if (citizenPositions.Length > 0)
				{
					SpreadDiseaseJob job = new SpreadDiseaseJob();
					job.diseasePositions = diseasePositions.AsArray();
					job.citizenPositions = citizenPositions.AsArray();
					job.spreadRadius = Mod.INSTANCE.m_Setting.diseaseSpreadRadius * 2;
					job.fleeRadius = job.spreadRadius + Mod.INSTANCE.m_Setting.diseaseFleeRadius;
					job.spreadChance = Mod.INSTANCE.m_Setting.diseaseSpreadChance;
					job.spread = new NativeArray<int>(citizenPositions.Length, Allocator.TempJob);
					job.flee = new NativeArray<bool>(citizenPositions.Length, Allocator.TempJob);
					var jobHandle = job.ScheduleBatch(citizenPositions.Length, 100);

					jobHandle.Complete();

					int spreadCount = 0;
					for (int i = 0; i < job.spread.Length; ++i)
					{
						int citizenIndex = citizenIndexes[i];
						if (job.spread[i] > 0 && spreadCount++ < Mod.INSTANCE.m_Setting.maxDiseaseSpreadPerFrame)
						{
							//Mod.log.Info("Spread to " + job.diseasePositions[i].ToString() + " entity " + citizens[citizenIndex].ToString() + " from position " + diseasePositions[job.spread[i] - 1].ToString() + " r: " + job.diseaseRandom[i].ToString());
							//EntityManager.AddComponent<Disease>(citizens[citizenIndexes[i]]);
							this.diseaseProgressionSystem.makeCitizenSick(citizens[citizenIndex]);
							/*Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
							EntityManager.AddComponentData(e, new ResetTrip
							{
								m_Creature = citizenTransports[citizenIndex].m_CurrentTransport,
								m_Target = Entity.Null,
								m_NextPurpose = Purpose.Hospital
								//m_DivertPurpose = Purpose.Hospital
							});*/
						}
						else if (job.flee[i] && (!EntityManager.TryGetComponent<TravelPurpose>(citizens[citizenIndex], out var travelPurpose) || 
							(travelPurpose.m_Purpose & Purpose.GoingHome) == 0))
						{
							Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
							EntityManager.AddComponentData(e, new ResetTrip
							{
								m_Creature = citizenTransports[citizenIndex].m_CurrentTransport,
								m_Target = Entity.Null,
								//m_DivertPurpose = Purpose.Hospital
							});
						}
					}

					diseasePositions.Dispose();
					citizenPositions.Dispose();
					citizenIndexes.Dispose();

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

		private void refreshMaskModifier()
		{
			if (this.areMasksRequired())
			{
				this.maskModifier = (100 - Mod.INSTANCE.m_Setting.maskEffectiveness) / 100f;
			}
			else
			{
				this.maskModifier = 1;
			}
		}

		private bool areMasksRequired()
		{
			NativeArray<City> city = this.cityQuery.ToComponentDataArray<City>(Allocator.Temp);
			if (city.Length > 0)
			{
				return (city[0].m_OptionMask & MASK_MANDATE_MASK) > 0;
			}

			return true;
		}
	}
}
