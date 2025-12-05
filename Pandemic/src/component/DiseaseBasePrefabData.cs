using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
   /* [ComponentMenu("Health/", new Type[] { typeof(DiseaseBasePrefab) })]
    public class DiseaseBasePrefabData : ComponentBase
    {

        public float baseSpreadRadius = 5;
        public float baseSpreadChance = 5;
        public float baseDeathChance = 5;
        public byte baseHealthPenalty = 5;
        public byte maxDeathHealth;
        public float mutationChance;
        public float mutationMagnitude;
        public float progressionSpeed;
        public float baseSpontaneousChance;

        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {

        }

        public override void GetPrefabComponents(HashSet<ComponentType> components)
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
        }
    }*/
}
