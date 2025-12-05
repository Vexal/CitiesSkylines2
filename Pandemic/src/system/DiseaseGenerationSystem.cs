using Colossal.Entities;
using Game;
using Game.Common;
using Game.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

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

		private Entity chooseNewDiseaseType()
		{
            NativeArray<DiseaseBase> diseaseBases = this.diseaseBaseEntityQuery.ToComponentDataArray<DiseaseBase>(Allocator.Temp);
            float totalWeight = 0;
			for (int i = 0; i < diseaseBases.Length; ++i)
            {
                totalWeight += diseaseBases[i].baseSpontaneousChance;
            }
			float rnd = UnityEngine.Random.Range(0f, totalWeight);

            for (int i = 0; i < diseaseBases.Length; ++i)
            {
                if (rnd < diseaseBases[i].baseSpontaneousChance)
                {
                    return diseaseBases[i].entity;
                }

                rnd -= diseaseBases[i].baseSpontaneousChance;
            }

            return diseaseBases[0].entity;
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
			Entity diseaseType = this.chooseNewDiseaseType();
			//if (!this.shouldCreateNewDisease(diseaseType))
			{
				NativeArray<Entity> diseases = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
				if (diseases.Length > 0)
				{
					int startInd = UnityEngine.Random.Range(0, diseases.Length - 1);
					for (int i = startInd; i < diseases.Length; ++i)
					{
						disease = EntityManager.GetComponentData<Disease>(diseases[i]);
						if (!disease.preventSpontaneously && disease.diseaseBase == diseaseType)
						{
							return disease.entity;
						}
					}

					if (startInd > 0)
					{
						for (int i = 0; i < startInd; ++i)
						{
							disease = EntityManager.GetComponentData<Disease>(diseases[i]);
							if (!disease.preventSpontaneously && disease.diseaseBase == diseaseType)
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

                disease = EntityManager.GetComponentData<Disease>(diseases[0]);
                return disease.entity;
			}
		}

		public Entity instantiateDiseaseEntity(ref Disease disease)
		{
			Entity diseaseEntity = EntityManager.CreateEntity(this.diseaseArchetype);
			disease.initMetadata(this.timeSystem.GetCurrentDateTime(), diseaseEntity);
			EntityManager.SetComponentData(diseaseEntity, disease);

			this.lastMutationFrame = this.simulationSystem.frameIndex;
            if (this.nameSystem.TryGetCustomName(disease.diseaseBase, out string name))
            {
                this.nameSystem.SetCustomName(diseaseEntity, name);
            }

            this.nameSystem.TryGetCustomName(diseaseEntity, out string newName);
            Mod.log.Info("using name " + name + " for disease " + diseaseEntity.ToString() + " with new name " + newName + " for strain " + disease.getStrainName());
			return diseaseEntity;
		}

		public Disease createDisease(DiseaseBase diseaseBase)
		{
			Disease disease = new Disease()
			{
				baseDeathChance = diseaseBase.baseDeathChance,
				baseHealthPenalty = diseaseBase.baseHealthPenalty,
				baseSpreadChance = diseaseBase.baseSpreadChance,
				baseSpreadRadius = diseaseBase.baseSpreadRadius,
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = diseaseBase.mutationChance,
				mutationMagnitude = diseaseBase.mutationMagnitude,
				progressionSpeed = diseaseBase.progressionSpeed,
                spontaneousProbability = diseaseBase.baseSpontaneousChance,
                diseaseBase = diseaseBase.entity
            };

			return disease;
		}

		public Disease createCustomDisease(DiseaseCreateInput inp)
		{
			Disease disease = new()
			{
				baseSpreadChance = inp.baseSpreadChance,
				baseDeathChance = inp.baseDeathChance,
				baseHealthPenalty = inp.baseHealthPenalty,
				baseSpreadRadius = inp.baseSpreadRadius,
				maxDeathHealth = MAX_DEATH_HEALTH,
				mutationChance = inp.mutationChance,
				mutationMagnitude = inp.mutationMagnitude,
				progressionSpeed = inp.progressionSpeed,
				spontaneousProbability = inp.spontaneousProbability,
                diseaseBase = inp.getBaseEntity()
            };

			Entity newDisease = EntityManager.CreateEntity(this.diseaseArchetype);
			disease.initMetadata(this.timeSystem.GetCurrentDateTime(), newDisease);

			EntityManager.SetComponentData(newDisease, disease);
			if (inp.name != "")
			{
				this.nameSystem.SetCustomName(newDisease, inp.name);
			}
            else if (this.nameSystem.TryGetCustomName(inp.getBaseEntity(), out string baseName))
            {
                this.nameSystem.SetCustomName(newDisease, baseName);
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
