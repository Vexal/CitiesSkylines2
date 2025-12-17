using Game;
using Game.Common;
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
        private EntityQuery diseaseEntityQuery;
        private EntityQuery vaccineResearchCenterEntityQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

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
            if (this.vaccineResearchCenterEntityQuery.CalculateEntityCount() == 0)
            {
                //vaccine research does not progress without a research center
                return;
            }

            NativeArray<Disease> diseases = this.diseaseEntityQuery.ToComponentDataArray<Disease>(Allocator.Temp);
            NativeArray<Entity> diseaseEntities = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < diseases.Length; i++)
            {
                Disease disease = diseases[i];
                if (disease.vaccineProgress < 1.0f)
                {
                    disease.vaccineProgress += 0.01f * disease.vaccineEffectiveness; //increase vaccine progress by 1% every second
                    if (disease.vaccineProgress > 1.0f)
                    {
                        disease.vaccineProgress = 1.0f;
                    }

                    EntityManager.SetComponentData(diseaseEntities[i], disease);
                }
            }
		}
	}
}
