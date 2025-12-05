using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
    public class DiseaseBasePrefabParameters : IComponentData, IQueryTypeParameter
    {

        public float baseSpreadRadius;
        public float baseSpreadChance;
        public float baseDeathChance;
        public byte baseHealthPenalty;
        public byte maxDeathHealth;
        public float mutationChance;
        public float mutationMagnitude;
        public float progressionSpeed;
        public float baseSpontaneousChance;
    }
}
