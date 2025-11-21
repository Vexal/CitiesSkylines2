using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
    public struct DiseaseBase : IComponentData, IQueryTypeParameter
    {
        public float baseSpreadRadius;
        public float baseSpreadChance;
        public float baseDeathChance;
        public byte baseHealthPenalty;
        public byte maxDeathHealth;
        public float mutationChance;
        public float mutationMagnitude;
        public float progressionSpeed;
        /*
         * baseDeathChance = (Mod.settings.ccDeathChance),
				baseHealthPenalty = (byte)Mod.settings.ccHealthImpact,
				baseSpreadChance = Mod.settings.ccSpreadChance,
				baseSpreadRadius = Mod.settings.ccSpreadRadius,
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = Mod.settings.ccMutationChance,
				mutationMagnitude = Mod.settings.ccMutationMagnitude,
				progressionSpeed = Mod.settings.ccProgressionSpeed,
         */
    }
}
