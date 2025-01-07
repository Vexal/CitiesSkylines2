using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityArchetype diseaseArchetype;

		public Entity createOrMutateDisease(Entity prev, out Disease disease)
		{
			if (prev == Entity.Null)
			{
				return this.getOrCreateRandomDisease(out disease);
			}
			else
			{
				disease = EntityManager.GetComponentData<Disease>(prev);
				if (this.isMutationCooldownActive() == 0 && disease.shouldMutate())
				{
					Disease mutation = disease.mutate();
					Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
					mutation.initMetadata(this.timeSystem.GetCurrentDateTime(), newDisease);
					disease = mutation;
					this.lastMutationFrame = this.simulationSystem.frameIndex;
					return newDisease;
				}
				else
				{
					return prev;
				}
			}
		}

		private bool shouldCreateNewDisease()
		{
			return this.isMutationCooldownActive() == 0 && UnityEngine.Random.Range(0f, 100f) < Mod.settings.newDiseaseChance;
		}

		private uint chooseNewDiseaseType()
		{
			float totalWeight = Mod.settings.ccChance + Mod.settings.flChance + Mod.settings.exChance;
			float rnd = UnityEngine.Random.Range(0f, totalWeight);

			if (rnd < Mod.settings.ccChance)
			{
				return 1;
			}

			rnd-= Mod.settings.ccChance;
			if (rnd < Mod.settings.flChance)
			{
				return 2;
			}

			return 3;
		}

		private uint lastMutationFrame = 0;
		private const uint MUTATION_COOLDOWN = 60 * 30;

		public uint isMutationCooldownActive()
		{
			uint d = this.simulationSystem.frameIndex - this.lastMutationFrame;
			if (d > MUTATION_COOLDOWN)
			{
				return 0;
			}

			return MUTATION_COOLDOWN - d;
		}

		private Entity getOrCreateRandomDisease(out Disease disease)
		{
			NativeArray<Entity> diseases = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
			if (diseases.Length > 0 && !this.shouldCreateNewDisease())
			{
				disease = EntityManager.GetComponentData<Disease>(diseases[UnityEngine.Random.Range(0, diseases.Length - 1)]);
				return disease.entity;
			}
			else
			{
				uint diseaseType = this.chooseNewDiseaseType();

				switch (diseaseType)
				{
					case 1:
						disease = this.createCommonCold();
						break;
					case 2:
						disease = this.createFlu();
						break;
					case 3:
						disease = this.createNovelVirus();
						break;
					default:
						disease = this.createCommonCold();
						break;
				}

				Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
				disease.initMetadata(this.timeSystem.GetCurrentDateTime(), newDisease);

				EntityManager.SetComponentData(newDisease, disease);
				this.lastMutationFrame = this.simulationSystem.frameIndex;
				return newDisease;
			}
		}

		public Disease createCommonCold()
		{
			Disease disease = new Disease()
			{
				type = 1,
				baseDeathChance = (Mod.settings.ccDeathChance),
				baseHealthPenalty = (byte)Mod.settings.ccHealthImpact,
				baseSpreadChance = Mod.settings.ccSpreadChance,
				baseSpreadRadius = Mod.settings.ccSpreadRadius,
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = Mod.settings.ccMutationChance,
				mutationMagnitude = Mod.settings.ccMutationMagnitude,
				progressionSpeed = Mod.settings.ccProgressionSpeed,
			};

			return disease.mutate(true);
		}

		public Disease createFlu()
		{
			Disease disease = new Disease()
			{
				type = 2,
				baseDeathChance = (Mod.settings.flDeathChance),
				baseHealthPenalty = (byte)Mod.settings.flHealthImpact,
				baseSpreadChance = Mod.settings.flSpreadChance,
				baseSpreadRadius = Mod.settings.flSpreadRadius,
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = Mod.settings.flMutationChance,
				mutationMagnitude = Mod.settings.flMutationMagnitude,
				progressionSpeed = Mod.settings.flProgressionSpeed,
			};

			return disease.mutate(true);
		}

		public Disease createNovelVirus()
		{
			Disease disease = new Disease()
			{
				type = 3,
				baseDeathChance = UnityEngine.Random.Range(0f, 100f),
				baseHealthPenalty = (byte)UnityEngine.Random.Range(0, 100),
				baseSpreadChance = UnityEngine.Random.Range(0f, 100f),
				baseSpreadRadius = UnityEngine.Random.Range(1, 100f),
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = UnityEngine.Random.Range(0f, 1f),
				mutationMagnitude = UnityEngine.Random.Range(0f, 1.99f),
				progressionSpeed = UnityEngine.Random.Range(.0001f, .99f),
			};

			return disease.mutate(true);
		}
	}
}
