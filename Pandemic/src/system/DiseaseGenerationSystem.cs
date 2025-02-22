using Colossal.Entities;
using Game;
using Game.Common;
using Game.UI;
using Unity.Collections;
using Unity.Entities;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityArchetype diseaseArchetype;
		private NameSystem nameSystem;

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
					disease = disease.mutate();
					/*Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
					mutation.initMetadata(this.timeSystem.GetCurrentDateTime(), newDisease);
					disease = mutation;
					this.lastMutationFrame[disease.type] = this.simulationSystem.frameIndex;
					this.frameDiseases[disease.type] = mutation;
					return newDisease;*/
					return this.instantiateDiseaseEntity(ref disease);
				}
				else
				{
					return prev;
				}
			}
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
			uint diseaseType = this.chooseNewDiseaseType();
			//if (!this.shouldCreateNewDisease(diseaseType))
			{
				NativeArray<Entity> diseases = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
				if (diseases.Length > 0)
				{
					int startInd = UnityEngine.Random.Range(0, diseases.Length - 1);
					for (int i = startInd; i < diseases.Length; ++i)
					{
						disease = EntityManager.GetComponentData<Disease>(diseases[i]);
						if (!disease.preventSpontaneously && disease.type == diseaseType)
						{
							return disease.entity;
						}
					}

					if (startInd > 0)
					{
						for (int i = 0; i < startInd; ++i)
						{
							disease = EntityManager.GetComponentData<Disease>(diseases[i]);
							if (!disease.preventSpontaneously && disease.type == diseaseType)
							{
								return disease.entity;
							}
						}
					}
				}

				/*if (this.frameDiseases[diseaseType].id != 0)
				{
					disease = this.frameDiseases[diseaseType];
					return disease.entity;
				}*/
			}

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

			/*Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
			disease.initMetadata(this.timeSystem.GetCurrentDateTime(), newDisease);

			EntityManager.SetComponentData(newDisease, disease);
			this.lastMutationFrame[diseaseType] = this.simulationSystem.frameIndex;
			this.frameDiseases[diseaseType] = disease;
			return newDisease;*/
			return this.instantiateDiseaseEntity(ref disease);
		}

		public Entity instantiateDiseaseEntity(ref Disease disease)
		{
			Entity diseaseEntity = EntityManager.CreateEntity(this.diseaseArchetype);
			disease.initMetadata(this.timeSystem.GetCurrentDateTime(), diseaseEntity);
			EntityManager.SetComponentData(diseaseEntity, disease);

			this.lastMutationFrame = this.simulationSystem.frameIndex;
			return diseaseEntity;
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

			return disease;
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

			return disease;
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

			return disease;
		}

		public Disease createCustomDisease(DiseaseCreateInput inp)
		{
			Disease disease = new()
			{
				type = inp.type,
				baseSpreadChance = inp.baseSpreadChance,
				baseDeathChance = inp.baseDeathChance,
				baseHealthPenalty = inp.baseHealthPenalty,
				baseSpreadRadius = inp.baseSpreadRadius,
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = inp.mutationChance,
				mutationMagnitude = inp.mutationMagnitude,
				progressionSpeed = inp.progressionSpeed,
				spontaneousProbability = inp.spontaneousProbability,
			};

			Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
			disease.initMetadata(this.timeSystem.GetCurrentDateTime(), newDisease);

			EntityManager.SetComponentData(newDisease, disease);
			if (inp.name != "")
			{
				this.nameSystem.SetCustomName(newDisease, inp.name);
			}

			return disease;
		}

		public Disease editDisease(DiseaseCreateInput inp)
		{
			Entity diseaseEntity = new Entity { Index = inp.entityIndex, Version = inp.entityVersion };
			if (EntityManager.Exists(diseaseEntity) && EntityManager.TryGetComponent<Disease>(diseaseEntity, out var currentDisease))
			{
				currentDisease.baseSpreadChance = inp.baseSpreadChance;
				currentDisease.baseDeathChance = inp.baseDeathChance;
				currentDisease.baseHealthPenalty = inp.baseHealthPenalty;
				currentDisease.baseSpreadRadius = inp.baseSpreadRadius;
				currentDisease.maxDeathHealth = MAX_DEATH_HEALTH;
				currentDisease.mutationChance = inp.mutationChance;
				currentDisease.mutationMagnitude = inp.mutationMagnitude;
				currentDisease.progressionSpeed = inp.progressionSpeed;
				currentDisease.preventSpontaneously = inp.preventSpontaneously;
				currentDisease.spontaneousProbability = inp.spontaneousProbability;
				EntityManager.SetComponentData(diseaseEntity, currentDisease);
				if (inp.name != "")
				{
					this.nameSystem.SetCustomName(diseaseEntity, inp.name);
				}

				return currentDisease;
			}
			else
			{
				return new Disease();
			}
		}

		public void deleteDisease(Entity disease)
		{
			this.cureDisease(disease);
			EntityManager.AddComponent<Deleted>(disease);
		}
	}
}
