using Colossal.Entities;
using Colossal.Logging;
using Game;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Pandemic
{
	internal partial class DiseaseProgressionSystem : GameSystemBase
	{
		private EntityQuery healthyDiseaseQuery;
		private EntityQuery unhealthyDiseaseEntityQuery;
		private EntityQuery healthProblemEntityQuery;
		private EntityQuery diseaseEntityQuery;
		private EntityQuery diseaseBaseEntityQuery;

		private PrefabSystem prefabSystem;
		private SimulationSystem simulationSystem;
		private TimeSystem timeSystem;

		private PrefabID sicknessEventPrefabId = new PrefabID("EventPrefab", "Generic Sickness");
		private PrefabID policyPrefabId = new PrefabID("PolicyTogglePrefab", "PreRelease Programs");
		private Entity sicknessEventPrefabEntity;
		private PrefabBase policyPrefabEntity;
		private EntityArchetype sickEventArchetype;

		private PrefabID suddenDeathPrefabId = new PrefabID("EventPrefab", "Sudden Death");
		private Entity suddenDeathPrefabEntity;
		private EntityArchetype deathEventArchetype;
		private EntityArchetype resetTripArchetype;
        private EntityArchetype diseaseBaseArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

            this.initializeDependentSystems();
			
			this.initializeCustomArchetypes();

            this.initializeEntityQueries();
            this.initializePolicyPrefabs();
		}

		private const uint PROGRESSION_FRAME_COUNT = 300;
		private const byte MAX_DEATH_HEALTH = 15;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            this.retrieveSicknessEventArchetypes();
		}

        bool hasPrint = false;
		protected override void OnUpdate()
		{
			if (GameManager.instance.gameMode != GameMode.Game || !Mod.settings.modEnabled)
			{
				return;
            }

            if (!hasPrint)
            {
                
            }

            this.removeDiseaseFromHealthy();
			this.addDiseaseToSick();
			if (this.shouldProgressDisease())
			{
				this.progressHealthProblems();
			}
		}

		private void removeDiseaseFromHealthy()
		{
			EntityManager.RemoveComponent<CurrentDisease>(this.healthyDiseaseQuery.ToEntityArray(Allocator.Temp));
			NativeArray<Entity> citizens = this.unhealthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
			for (int i = 0; i < citizens.Length; ++i)
			{
				if (!isSick(healthProblems[i].m_Flags))
				{
					EntityManager.RemoveComponent<CurrentDisease>(citizens[i]);
				}
			}
		}

		private void addDiseaseToSick()
		{
			NativeArray<Entity> citizens = this.healthProblemEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.healthProblemEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);

			Dictionary<Entity, uint> newInfectionCounts = new Dictionary<Entity, uint>();

			for (int i = 0; i < citizens.Length; ++i)
			{
				if (!isSick(healthProblems[i].m_Flags))
				{
					continue;
				}

				Entity disease = Entity.Null;
				Entity healthEvent = healthProblems[i].m_Event;

				if (EntityManager.Exists(healthEvent) && EntityManager.TryGetComponent<DiseaseRef>(healthEvent, out var diseaseRef))
				{
					disease = diseaseRef.disease;
				}

				disease = this.createOrMutateDisease(disease, out var diseaseDefinition);

				if (!EntityManager.TryGetComponent<LastDisease>(citizens[i], out var lastDisease))
				{
					lastDisease = new LastDisease();
					EntityManager.AddComponent<LastDisease>(citizens[i]);
				}

				lastDisease.setLastDisease(disease);

				EntityManager.SetComponentData(citizens[i], lastDisease);

				EntityManager.AddComponent<CurrentDisease>(citizens[i]);
				EntityManager.SetComponentData(citizens[i], new CurrentDisease() { disease = disease, progression = .001f });
				if (newInfectionCounts.ContainsKey(disease))
				{
					newInfectionCounts[disease]++;
				}
				else
				{
					newInfectionCounts[disease] = 1;
				}
				
			}

			foreach (var e in newInfectionCounts)
			{
				if (EntityManager.TryGetComponent<Disease>(e.Key, out var disease))
				{
					disease.infectionCount += e.Value;
					EntityManager.SetComponentData(e.Key, disease);
				}
			}
		}

		private void progressHealthProblems()
		{
			NativeArray<Entity> citizens = this.unhealthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<Citizen> citizenData = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<Citizen>(Allocator.Temp);
			NativeArray<CurrentDisease> currentDiseases = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<CurrentDisease>(Allocator.Temp);

			if (citizens.Length > 0) {
				for (int i = 0; i < citizens.Length; ++i)
				{
					Citizen c = citizenData[i];
					if (this.isInHospital(citizens[i]))
					{
						// Disease does not progress in hospital
						continue;
					}

					Disease disease = EntityManager.GetComponentData<Disease>(currentDiseases[i].disease);

					CurrentDisease currentDisease = currentDiseases[i];

                    if (this.isMutationCooldownActive() <= 0 && disease.shouldMutate())
                    {
                        Disease mutatedDisease = disease.mutate();
                        this.instantiateDiseaseEntity(ref mutatedDisease);
                        currentDisease.progression = 0f;
                        currentDisease.disease = mutatedDisease.entity;
                        currentDiseases[i] = currentDisease;

                        if (!EntityManager.TryGetComponent<LastDisease>(citizens[i], out var lastDisease))
                        {
                            lastDisease = new LastDisease();
                            EntityManager.AddComponent<LastDisease>(citizens[i]);
                        }

                        lastDisease.setLastDisease(mutatedDisease.entity);

                        EntityManager.SetComponentData(citizens[i], lastDisease);
                        continue;
                    }

                    currentDisease.progression += disease.progressionSpeed;
					currentDisease.progression = math.min(1f, currentDisease.progression);
					currentDiseases[i] = currentDisease;

					if (disease.baseHealthPenalty > 0)
					{
						byte healthPenalty = (byte)(currentDisease.progression * disease.baseHealthPenalty);
						if (c.m_Health <= healthPenalty)
						{
							c.m_Health = 0;
						}
						else
						{
							c.m_Health -= healthPenalty;
							EntityManager.SetComponentData(citizens[i], c);
						}
					}

					if (disease.baseDeathChance > 0 && c.m_Health <= disease.maxDeathHealth)
					{
						float deathChance = disease.baseDeathChance * currentDisease.progression;
						if (UnityEngine.Random.Range(0, 100) <= deathChance)
						{
							this.killCitizen(citizens[i]);
						}
					}
				}

				this.unhealthyDiseaseEntityQuery.CopyFromComponentDataArray(currentDiseases);
			}
		}

		public void makeCitizenSick(Entity targetCitizen, Entity disease)
		{
            if (!EntityManager.Exists(disease))
            {
                Mod.log.Info("Error: " + disease + " " + disease.ToString() + " does not exist");
                return;
            }

            //Mod.log.Info("Making entity " + targetCitizen.ToString() + " sick with disease " + disease.ToString() + " archetype: " + this.sickEventArchetype.ToString());
            Entity eventEntity = EntityManager.CreateEntity(this.sickEventArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(this.sicknessEventPrefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = targetCitizen });
			EntityManager.AddComponent<DiseaseRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new DiseaseRef() { disease = disease });

		}

		public bool validateDisease(Entity disease)
		{
			return EntityManager.Exists(disease) && EntityManager.HasComponent<Disease>(disease);
		}

		public void cureDisease(Entity disease)
		{
			if (disease != Entity.Null && !this.validateDisease(disease))
			{
				return;
			}

			NativeArray<Entity> citizens = this.unhealthyDiseaseEntityQuery.ToEntityArray(Allocator.Temp);
			NativeArray<CurrentDisease> currentDiseases = disease == Entity.Null ? default : this.unhealthyDiseaseEntityQuery.ToComponentDataArray<CurrentDisease>(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.unhealthyDiseaseEntityQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);

			for (int i = 0; i < citizens.Length; i++)
			{
				if ((disease == Entity.Null || currentDiseases[i].disease == disease) && isSick(healthProblems[i].m_Flags))
				{
					EntityManager.RemoveComponent<HealthProblem>(citizens[i]);
					this.resetCitizenTrip(citizens[i], Purpose.Hospital);
				}
			}
		}

		public void cureCitizen(Entity targetCitizen)
		{
			if (EntityManager.Exists(targetCitizen))
			{
				if (EntityManager.TryGetComponent<HealthProblem>(targetCitizen, out var healthProblem) && isSick(healthProblem.m_Flags))
				{
					Mod.log.Info("Attempt curing " + targetCitizen);
					EntityManager.RemoveComponent<HealthProblem>(targetCitizen);
					this.resetCitizenTrip(targetCitizen, Purpose.Hospital);
				}
			}
		}

		private void killCitizen(Entity target)
		{
			Entity eventEntity = EntityManager.CreateEntity(this.deathEventArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(this.suddenDeathPrefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = target });
		}

		private void resetCitizenTrip(Entity citizen, Purpose purposeFilter)
		{
			if (EntityManager.TryGetComponent<CurrentTransport>(citizen, out var currentTransport) &&
				(purposeFilter == Purpose.None ||
				EntityManager.TryGetComponent<TravelPurpose>(citizen, out var travelPurpose) && travelPurpose.m_Purpose == purposeFilter))
			{
				Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
				EntityManager.AddComponentData(e, new ResetTrip
				{
					m_Creature = currentTransport.m_CurrentTransport,
					m_Target = Entity.Null
				});
			}
		}

		private bool shouldProgressDisease()
		{
			return this.simulationSystem.frameIndex % PROGRESSION_FRAME_COUNT == 0;
		}

		public static bool isSick(HealthProblemFlags flags)
		{
			return (flags & HealthProblemFlags.Sick) > 0 && (flags & HealthProblemFlags.Dead) == 0;
		}

		private bool isInHospital(Entity citizen)
		{
			return EntityManager.TryGetComponent<CurrentBuilding>(citizen, out var building) && EntityManager.HasComponent<Game.Buildings.Hospital>(building.m_CurrentBuilding);
        }

        private void initializeDependentSystems()
        {
            this.prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            this.timeSystem = World.GetOrCreateSystemManaged<TimeSystem>();
            this.nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
        }
        private void initializeEntityQueries()
        {
            this.healthyDiseaseQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadOnly<CurrentDisease>(),
                ComponentType.ReadOnly<Citizen>()
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>(),
                ComponentType.ReadOnly<HealthProblem>()
                }
            });

            this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadOnly<Disease>(),
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
                }
            });

            this.diseaseBaseEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadOnly<DiseaseBase>(),
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
                }
            });

            this.healthProblemEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadOnly<HealthProblem>(),
                ComponentType.ReadOnly<Citizen>()
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>(),
                ComponentType.ReadOnly<CurrentDisease>(),
                }
            });

            this.unhealthyDiseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
            {
                ComponentType.ReadOnly<HealthProblem>(),
                ComponentType.ReadWrite<CurrentDisease>(),
                ComponentType.ReadOnly<Citizen>()
            },
                None = new ComponentType[]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
                }
            });
        }

        private void initializeCustomArchetypes()
        {
            this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());
            this.diseaseArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Disease>());
            this.diseaseBaseArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<DiseaseBase>());
        }

        private void initializePolicyPrefabs()
        {
            this.prefabSystem.TryGetPrefab(this.policyPrefabId, out this.policyPrefabEntity);
            
            PrefabBase pr = this.policyPrefabEntity.Clone("Mask Mandate");
            pr.Remove(typeof(CityModifiers));
            pr.Remove(typeof(Unlockable));
            /*pr.AddComponent<CityModifiers>();
            pr.GetComponent<CityModifiers>().m_Modifiers = new CityModifierInfo[1];
            pr.GetComponent<CityModifiers>().m_Modifiers[0] = new CityModifierInfo();
            pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Type = 100;
            pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Mode = ModifierValueMode.Relative;
            pr.GetComponent<CityModifiers>().m_Modifiers[0].m_Range = new Colossal.Mathematics.Bounds1(new float2() { x = 15.5f, y = 15.5f });*/

            /*pr.GetComponent<CityModifiers>().m_Modifiers[1] = new CityModifierInfo();
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Type = CityModifierType.DiseaseProbability;
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Mode = ModifierValueMode.Relative;
			pr.GetComponent<CityModifiers>().m_Modifiers[1].m_Range = new Colossal.Mathematics.Bounds1(new float2() {x = 100f, y = 100f });*/
            this.prefabSystem.AddPrefab(pr);
            //Mod.log.Info("asset: " + pr.ToString() + " ; " + pr.asset?.name + " ; " + pr.asset?.ToString() + " ; " + pr.asset?.uniqueName);
            //Mod.log.Info("original asset: " + this.policyPrefabEntity.asset?.name + " ; " + this.policyPrefabEntity.asset?.ToString() + " ; " + this.policyPrefabEntity.asset?.uniqueName);
            this.prefabSystem.AddComponentData(pr, new CityOptionData() { m_OptionMask = PandemicSpreadSystem.MASK_MANDATE_MASK });
            /*foreach (string s in GameManager.instance.localizationManager.activeDictionary.entryIDs)
			{
				if (s.Contains("Policy."))
				{
					Mod.log.Info("policy: " + s);
				}
			}*/
        }

        private void retrieveSicknessEventArchetypes()
        {
            if (this.prefabSystem.TryGetPrefab(sicknessEventPrefabId, out PrefabBase prefabBase))
            {
                //this.prefabSystem.TryGetEntity(prefabBase, out this.sicknessEventPrefabEntity);
                //this.sickEventArchetype = EntityManager.GetComponentData<EventData>(this.sicknessEventPrefabEntity).m_Archetype;

                PrefabBase pandemicPrefab = prefabBase.Clone("Pandemic Sickness");
                Game.Prefabs.HealthEvent healthEvent = pandemicPrefab.GetComponent<Game.Prefabs.HealthEvent>();
                healthEvent.m_RequireTracking = true;
                /*pandemicPrefab.Remove(typeof(Game.Prefabs.HealthEvent));
				pandemicPrefab.AddComponent<Game.Prefabs.HealthEvent>();
				pandemicPrefab.AddComponentFrom<Game.Prefabs.HealthEvent>();*/
                prefabBase.AddComponentFrom(healthEvent);
                if (this.prefabSystem.AddPrefab(pandemicPrefab))
                {
                    if (this.prefabSystem.TryGetEntity(pandemicPrefab, out this.sicknessEventPrefabEntity))
                    {
                        if (this.prefabSystem.TryGetEntity(prefabBase, out var e))
                        {
                            if (EntityManager.HasComponent<EventData>(e))
                            {
                                EventData eventData = EntityManager.GetComponentData<EventData>(e);
                                //Mod.log.Info("Got event data " + eventData.ToString() + " and archetype " + eventData.m_Archetype);
                                //Mod.log.Info("Got event data " + eventData.ToString() + " and archetype " + eventData.m_Archetype.ToString());
                                this.sickEventArchetype = eventData.m_Archetype;
                                Mod.log.Info("Got event data component");
                            }
                            else
                            {
                                Mod.log.Info("Error: missing component event data for " + e.ToString());
                            }
                        }
                        else
                        {
                            Mod.log.Info("Failed got sick archetype 5");
                        }
                    }
                    else
                    {
                        Mod.log.Info("Failed got sick archetype 2");
                    }
                }
                else
                {
                    Mod.log.Info("Failed to get sick archetype");
                }
            }
            else
            {
                Mod.log.Info("Failed to find sick prefab");
            }

            Mod.log.Info("Loaded disease progression system.");
            if (this.prefabSystem.TryGetPrefab(this.suddenDeathPrefabId, out PrefabBase prefabBase2))
            {
                this.prefabSystem.TryGetEntity(prefabBase2, out this.suddenDeathPrefabEntity);
                this.deathEventArchetype = EntityManager.GetComponentData<EventData>(this.suddenDeathPrefabEntity).m_Archetype;
            }
        }


		List<DiseaseBasePrefab> diseaseBasePrefabs = new List<DiseaseBasePrefab>();

		private void loadCustomPrefabs()
		{
			Mod.log.Info("Loading custom prefabs");
			NativeArray<Entity> prefabEntities = GetEntityQuery(ComponentType.ReadOnly<PrefabData>()).ToEntityArray(Allocator.Temp);
			foreach (Entity e in prefabEntities)
			{
				if (!EntityManager.TryGetComponent(e, out PrefabData prefabData))
				{
					continue;
				}

				if (!this.prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase))
				{
					continue;
				}
			}
		}

		private void loadDiseaseBasePrefabs()
		{
			this.diseaseBasePrefabs = new List<DiseaseBasePrefab>();
			NativeArray<Entity> prefabEntities = GetEntityQuery(ComponentType.ReadOnly<PrefabData>()).ToEntityArray(Allocator.Temp);
			foreach (Entity e in prefabEntities)
			{
				if (!EntityManager.TryGetComponent(e, out PrefabData prefabData))
				{
					continue;
				}

				if (!this.prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase))
				{
					continue;
				}

				if (prefabBase != null)
				{
					PrefabID prefabID = prefabBase.GetPrefabID();
					if (prefabID.GetName() == "DiseaseBasePrefab" || prefabBase.GetType().Name == "DiseaseBasePrefab")
					{
						Mod.log.Info(prefabBase.GetPrefabID() + " disease: " + prefabBase.ToString());
						this.diseaseBasePrefabs.Add((DiseaseBasePrefab)prefabBase);
					}
				}
			}

			Mod.log.Info("finished loading " + this.diseaseBasePrefabs.Count.ToString() + " disease base prefabs");
		}

        protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);
            EntityManager.DestroyEntity(this.diseaseEntityQuery);
            EntityManager.DestroyEntity(this.diseaseBaseEntityQuery);
			this.loadDiseaseBasePrefabs();
			this.loadCustomPrefabs();
			Mod.log.Info("Preloading game");
        }

        protected override void OnGameLoaded(Colossal.Serialization.Entities.Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);
            Mod.log.Info("Loaded game");
        }

        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            Mod.log.Info("Loaded game complete " + purpose.ToString());

            if (purpose == Colossal.Serialization.Entities.Purpose.NewGame || purpose == Colossal.Serialization.Entities.Purpose.LoadGame)
			{
				NativeArray<DiseaseBase> diseaseBases = this.diseaseBaseEntityQuery.ToComponentDataArray<DiseaseBase>(Allocator.Temp);
				// If no disease bases exist, create them from prefabs, otherwise assume they were loaded from save
				if (diseaseBases.Length > 0)
				{
					Mod.log.Info(diseaseBases.Length.ToString() + " existing disease bases found, skipping creation from prefabs");
					/*foreach (DiseaseBase diseaseBase in diseaseBases)
					{
						Disease cc = this.createDisease(diseaseBase);
						this.instantiateDiseaseEntity(ref cc);
					}*/
				}
				else
				{
					Mod.log.Info("no existing disease bases found, creating from prefabs");
					this.createDiseaseBasesFromPrefas();
				}
			}
		}

        public void createDiseaseBasesFromPrefas()
        {
            Mod.log.Info("initializing new game health system");
            foreach (DiseaseBasePrefab diseaseBasePrefab in this.diseaseBasePrefabs)
			{
                Entity entity = EntityManager.CreateEntity(this.diseaseBaseArchetype);
                DiseaseBase diseaseBase = new()
                {
                    baseDeathChance = diseaseBasePrefab.baseDeathChance,
                    baseHealthPenalty = (byte)diseaseBasePrefab.baseHealthPenalty,
                    baseSpreadChance = diseaseBasePrefab.baseSpreadChance,
                    baseSpreadRadius = diseaseBasePrefab.baseSpreadRadius,
                    maxDeathHealth = diseaseBasePrefab.maxDeathHealth,
                    mutationChance = diseaseBasePrefab.mutationChance,
                    mutationMagnitude = diseaseBasePrefab.mutationMagnitude,
                    progressionSpeed = diseaseBasePrefab.progressionSpeed,
                    baseSpontaneousChance = diseaseBasePrefab.baseSpontaneousChance,
                    entity = entity
                };

                EntityManager.SetComponentData(entity, diseaseBase);
                this.nameSystem.SetCustomName(entity, diseaseBasePrefab.diseaseName);

				Disease cc = this.createDisease(diseaseBase);
				this.instantiateDiseaseEntity(ref cc);
			}
        }
	}
}
