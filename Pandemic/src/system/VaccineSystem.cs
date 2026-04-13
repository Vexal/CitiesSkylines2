using Colossal.Entities;
using Game;
using Game.City;
using Game.Common;
using Game.Simulation;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Pandemic
{
	public partial class VaccineSystem : GameSystemBase
	{
        private CitySystem citySystem;
        private EntityQuery diseaseEntityQuery;
        private EntityQuery vaccineResearchCenterEntityQuery;
        public bool hasVaccineResearchCenter { get; private set; } = false;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.citySystem = World.GetOrCreateSystemManaged<CitySystem>();

            this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadWrite<Disease>(),
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>(),
                ComponentType.ReadOnly<VaccinatedDisease>()
                }
            });

            this.vaccineResearchCenterEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadOnly<VaccineFacility>(),
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
                }
            });
        }

        private ulong frameCounter = 0;

        protected override void OnUpdate()
		{
            if (this.frameCounter++ % 60 != 0)
            {
                return;
            }
            if (this.vaccineResearchCenterEntityQuery.IsEmpty)
            {
                //vaccine research does not progress without a research center
                this.hasVaccineResearchCenter = false;
                return;
            }

            this.hasVaccineResearchCenter = true;

            NativeArray<Disease> diseases = this.diseaseEntityQuery.ToComponentDataArray<Disease>(Allocator.Temp);
            NativeArray<Entity> diseaseEntities = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < diseases.Length; i++)
            {
                Disease disease = diseases[i];
                if (disease.vaccineProgress < 1.0f)
                {
                    disease.vaccineProgress += 0.0001f * disease.vaccineEffectiveness; //increase vaccine progress by 1% every second
                    if (disease.vaccineProgress > 1.0f)
                    {
                        disease.vaccineProgress = 1.0f;
                        EntityManager.AddComponent<VaccinatedDisease>(diseaseEntities[i]);
                    }

                    EntityManager.SetComponentData(diseaseEntities[i], disease);
                }
            }
		}



        private static readonly int TOTAL_VACCINE_COST = 1000000;

        public void fundVaccine(Entity diseaseEntity, float maxFundAmount)
        {

            if (!EntityManager.TryGetComponent<Disease>(diseaseEntity, out var disease))
            {
                return;
            }

            if (disease.vaccineProgress >= 1f)
            {
                return;
            }

            // int currentMoney = this.cityStatisticsSystem.GetStatisticValue(Game.City.StatisticType.Money);
            if (!EntityManager.TryGetComponent<PlayerMoney>(this.citySystem.City, out var playerMoney))
            {
                return;
            }

            float progressDelta;
            if (1 - disease.vaccineProgress < maxFundAmount)
            {
                progressDelta = 1 - disease.vaccineProgress;
            }
            else
            {
                progressDelta = maxFundAmount;
            }

            int progressCost = (int)(TOTAL_VACCINE_COST * progressDelta);
            Mod.log.Info("Current money: " + playerMoney.money.ToString() + "; research cost: " + progressCost);
            disease.vaccineProgress += progressDelta;
            EntityManager.SetComponentData(diseaseEntity, disease);

            playerMoney.Subtract(progressCost);
            EntityManager.SetComponentData(this.citySystem.City, playerMoney);
        }

        private bool hasVaccineResearchCenter()
        {

        }
    }
}
