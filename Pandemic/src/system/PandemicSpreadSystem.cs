using Colossal;
using Colossal.Entities;
using Game;
using Game.Buildings;
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
		public bool masksRequired;
		public int missingEducationModifier;

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
				ComponentType.ReadOnly<HealthProblem>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentTransport>(),
				ComponentType.ReadOnly<CurrentDisease>(),
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
				ComponentType.ReadOnly<CurrentDisease>(),
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

			this.refreshSettings();
			this.refreshMaskModifier();
			Mod.INSTANCE.m_Setting.onSettingsApplied += setting =>
			{
				this.refreshSettings();
			};
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

			ComputeDiseaseSpreadParametersJob spreadParametersJob = this.initDiseaseSpreadParamsJob(this.diseaseCitizenHumanEntityQuery);

			JobHandle spreadJobHandle = spreadParametersJob.ScheduleParallel(this.diseaseCitizenHumanEntityQuery, default);
			spreadJobHandle.Complete();

			if (spreadParametersJob.rc.Count > 0)
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
					job.diseasePositions = spreadParametersJob.diseasePositions;
					job.diseaseRadiusSq = spreadParametersJob.diseaseRadiusSq;
					job.citizenPositions = citizenPositions.AsArray();
					job.fleeRadius = 0;// job.spreadRadius + Mod.INSTANCE.m_Setting.diseaseFleeRadius;
					job.spreadChance = Mod.INSTANCE.m_Setting.diseaseSpreadChance;
					job.spread = new NativeArray<int>(citizenPositions.Length, Allocator.TempJob);
					job.flee = new NativeArray<bool>(citizenPositions.Length, Allocator.TempJob);
					var jobHandle = job.ScheduleBatch(citizenPositions.Length, 100);

					jobHandle.Complete();

					int spreadCount = 0;
					for (int i = 0; i < job.spread.Length; ++i)
					{
						int citizenIndex = citizenIndexes[i];
						if (job.spread[i] > 0 && !this.isImmuneToDisease(citizens[citizenIndex], spreadParametersJob.diseases[job.spread[i] - 1]) && spreadCount++ < Mod.INSTANCE.m_Setting.maxDiseaseSpreadPerFrame)
						{
							
							//Mod.log.Info("Spread to " + job.diseasePositions[i].ToString() + " entity " + citizens[citizenIndex].ToString() + " from position " + diseasePositions[job.spread[i] - 1].ToString() + " r: " + job.diseaseRandom[i].ToString());
							//EntityManager.AddComponent<Disease>(citizens[citizenIndexes[i]]);
							this.diseaseProgressionSystem.makeCitizenSick(citizens[citizenIndex], spreadParametersJob.diseases[job.spread[i] - 1]);
							
						}
						/*else if (job.flee[i] && (!EntityManager.TryGetComponent<TravelPurpose>(citizens[citizenIndex], out var travelPurpose) || 
							(travelPurpose.m_Purpose & Purpose.GoingHome) == 0))
						{
							Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
							EntityManager.AddComponentData(e, new ResetTrip
							{
								m_Creature = citizenTransports[citizenIndex].m_CurrentTransport,
								m_Target = Entity.Null,
								//m_DivertPurpose = Purpose.Hospital
							});
						}*/
					}

					citizenIndexes.Dispose();

					job.spread.Dispose();
					job.flee.Dispose();
				}

				citizenPositions.Dispose();
			}

			spreadParametersJob.cleanup();
		}

		private bool refreshMaskModifier()
		{
			if (this.areMasksRequired())
			{
				this.maskModifier = (100 - Mod.INSTANCE.m_Setting.maskEffectiveness) / 100f;
				this.masksRequired = true;			
				return true;
			}
			else
			{
				this.maskModifier = 1;
				this.masksRequired = false;
				return false;
			}
		}

		private bool isImmuneToDisease(Entity citizen, Entity diseaseEntity)
		{
			if (!EntityManager.TryGetComponent<LastDisease>(citizen, out var lastDisease))
			{
				return false;
			}

			if (!EntityManager.TryGetComponent<Disease>(diseaseEntity, out var disease))
			{
				return false;
			}

			Entity lastRelevantDisease = lastDisease.getLastOfType(disease.type);
			if (lastRelevantDisease == Entity.Null)
			{
				return false;
			}

			if (!EntityManager.TryGetComponent<Disease>(lastRelevantDisease, out var mostRecentDiseaseDefinition))
			{
				return false;
			}

			return mostRecentDiseaseDefinition.ts >= disease.ts;
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

		public static bool citizenWearsMask(Citizen citizen, int missingEducationModifier, bool isStudent)
		{
			int undereducated = isCitizenUnderEducated(citizen, isStudent);
			if (undereducated <= 0)
			{
				return true;
			}

			undereducated = undereducated + missingEducationModifier;

			return undereducated == 0 || citizen.m_PseudoRandom % undereducated == 0;
		}

		public static int isCitizenUnderEducated(Citizen citizen, bool isStudent)
		{
			CitizenAge citizenAge = citizen.GetAge();
			switch (citizenAge)
			{
				case CitizenAge.Adult:
				case CitizenAge.Elderly:
					int educationLevel = citizen.GetEducationLevel();
					if (educationLevel == 4)
					{
						return 0;
					}
					
					return (4 - educationLevel) - (isStudent ? 1 : 0);
				default:
					return 0;
			}
		}

		public ComputeDiseaseSpreadParametersJob initDiseaseSpreadParamsJob(EntityQuery diseaseQuery)
		{
			int maxResultSize = diseaseQuery.CalculateEntityCountWithoutFiltering();

			ComputeDiseaseSpreadParametersJob spreadParametersJob = new ComputeDiseaseSpreadParametersJob();
			spreadParametersJob.diseasePositions = new NativeArray<float3>(maxResultSize, Allocator.TempJob);
			spreadParametersJob.diseaseRadiusSq = new NativeArray<float>(maxResultSize, Allocator.TempJob);
			spreadParametersJob.diseases = new NativeArray<Entity>(maxResultSize, Allocator.TempJob);
			spreadParametersJob.citizenHandle = SystemAPI.GetComponentTypeHandle<Citizen>();
			spreadParametersJob.currentTransportHandle = SystemAPI.GetComponentTypeHandle<CurrentTransport>();
			spreadParametersJob.currentDiseaseHandle = SystemAPI.GetComponentTypeHandle<CurrentDisease>();
			spreadParametersJob.currentBuildingHandle = SystemAPI.GetComponentTypeHandle<CurrentBuilding>();
			spreadParametersJob.currentVehicleLookup = SystemAPI.GetComponentLookup<CurrentVehicle>();
			spreadParametersJob.transformLookup = SystemAPI.GetComponentLookup<Transform>();
			spreadParametersJob.diseaseLookup = SystemAPI.GetComponentLookup<Disease>();
			spreadParametersJob.hospitalLookup = SystemAPI.GetComponentLookup<Hospital>();
			spreadParametersJob.rc = new NativeCounter(Allocator.TempJob);
			spreadParametersJob.resultCounter = spreadParametersJob.rc.ToConcurrent();
			spreadParametersJob.masksRequired = this.masksRequired;
			spreadParametersJob.maskSpreadModifier = this.maskModifier;
			spreadParametersJob.maskAversionModifier = this.missingEducationModifier;

			return spreadParametersJob;
		}

		private void refreshSettings()
		{
			switch (Mod.INSTANCE.m_Setting.underEducatedModifier)
			{
				case PandemicSettings.UnderEducatedPolicyAdherenceModifier.None:
					this.missingEducationModifier = -4;
					break;
				case PandemicSettings.UnderEducatedPolicyAdherenceModifier.Minor:
					this.missingEducationModifier = -3;
					break;
				case PandemicSettings.UnderEducatedPolicyAdherenceModifier.Moderate:
					this.missingEducationModifier = 0;
					break;
				case PandemicSettings.UnderEducatedPolicyAdherenceModifier.Severe:
					this.missingEducationModifier = 2;
					break;
				case PandemicSettings.UnderEducatedPolicyAdherenceModifier.Extreme:
					this.missingEducationModifier = 4;
					break;
			}
		}
	}
}
