using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
    [ComponentMenu("Health/", new Type[] { })]
    public class DiseaseBasePrefab : PrefabBase
    {
        /*
         * this.ccMutationChance = .005f;
			this.ccMutationMagnitude = .15f;
			this.ccProgressionSpeed = .015f;
			this.ccHealthImpact = 0;
			this.ccDeathChance = 0;
			this.ccSpreadChance = .02f;
			this.ccSpreadRadius = 10f;
         */

        public float baseSpreadRadius = 10f;
        public float baseSpreadChance = .02f;
        public float baseDeathChance = 0;
        public byte baseHealthPenalty = 0;
        public byte maxDeathHealth = 15;
        public float mutationChance = .005f;
        public float mutationMagnitude = .15f;
        public float progressionSpeed = .015f;
        public float baseSpontaneousChance = 100;
		public string diseaseName = "New Disease";

		public override string ToString()
        {
            return base.ToString() + ": Disease: " + this.diseaseName + " spread: " + this.baseSpreadRadius.ToString();
        }



        /* public override void GetPrefabComponents(HashSet<ComponentType> components)
         {
             components.Add(ComponentType.ReadWrite<DiseaseBasePrefabParameters>());
         }

         public override void Initialize(EntityManager entityManager, Entity entity)
         {
             base.Initialize(entityManager, entity);
             DiseaseBasePrefabParameters componentData = default(DiseaseBasePrefabParameters);
             componentData.baseSpreadRadius = baseSpreadRadius;
             componentData.baseSpreadChance = baseSpreadChance;
             componentData.baseDeathChance = baseDeathChance;
             componentData.baseHealthPenalty = baseHealthPenalty;
             componentData.maxDeathHealth = maxDeathHealth;
             componentData.mutationChance = mutationChance;
             componentData.mutationMagnitude = mutationMagnitude;
             componentData.progressionSpeed = progressionSpeed;
             componentData.baseSpontaneousChance = baseSpontaneousChance;
             entityManager.SetComponentData(entity, componentData);
         }*/
    }
}
