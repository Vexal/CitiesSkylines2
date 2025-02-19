using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace Pandemic
{
	internal partial class ForceSicknessSystem : UISystemBase
	{
		private InputAction forceSickAction;
		private InputAction forceDangerAction;
		private InputAction forceAllSickAction;
		ToolSystem toolSystem;
		SimulationSystem simulationSystem;
		Entity selectedEntity;
		private EntityArchetype resetTripArchetype;
		private EntityQuery sickEventQuery;
		private EntityQuery allHealthyCitizenQuery;
		private EntityQuery allHealthyCitizenInBuildingQuery;
		private EntityQuery removeDiseaseQuery;
		private EntityQuery hospitalQuery;
		private EntityQuery diseaseQuery;
		private EntityQuery travelCitizensQuery;

		protected override void OnCreate()   
		{
			base.OnCreate();
			this.prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
			this.addHealthProblemArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<AddHealthProblem>());
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

			this.forceSickAction = new InputAction("forceSick", InputActionType.Button);
			//this.forceSickAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/s").With("Modifier", "<keyboard>/shift");

			this.forceDangerAction = new InputAction("forceDangerAction", InputActionType.Button);
			//this.forceDangerAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/d").With("Modifier", "<keyboard>/shift");

			this.forceAllSickAction = new InputAction("forceDangerAction", InputActionType.Button);
			//this.forceAllSickAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/a").With("Modifier", "<keyboard>/shift");

			this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());
			this.sickEventQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<SicknessEventData>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.allHealthyCitizenQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});

			this.allHealthyCitizenInBuildingQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentBuilding>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});

			this.removeDiseaseQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<CurrentDisease>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});

			this.diseaseQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<CurrentDisease>(),
				ComponentType.ReadOnly<HealthProblem>(),
				ComponentType.ReadWrite<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.travelCitizensQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<HealthProblem>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<TravelPurpose>(),
				ComponentType.ReadOnly<CurrentTransport>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.hospitalQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Game.Buildings.Hospital>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});
		}

		protected override void OnStartRunning() 
		{
			base.OnStartRunning();
			this.forceSickAction.Enable();
			this.forceDangerAction.Enable();
			this.forceAllSickAction.Enable();
		}

		protected override void OnStopRunning() 
		{
			base.OnStopRunning();
			this.forceSickAction.Disable();
			this.forceDangerAction.Disable();
			this.forceAllSickAction.Disable();
		}

		private byte getDiseasePenalty()
		{
			switch (Mod.INSTANCE.m_Setting.diseaseProgressionSpeed)
			{
				case PandemicSettings.DiseaseProgression.Minor:
					return 2;
				case PandemicSettings.DiseaseProgression.Moderate:
					return 5;
				case PandemicSettings.DiseaseProgression.Severe:
					return 15;
				case PandemicSettings.DiseaseProgression.Extreme:
					return 35;
				default:
					return 0;
			}
		}

		protected override void OnUpdate()
		{
			this.checkHospitals();
			NativeArray<SicknessEventData> sickEvents = this.sickEventQuery.ToComponentDataArray<SicknessEventData>(Allocator.Temp);
			NativeArray<Entity> sickEventEntities = this.sickEventQuery.ToEntityArray(Allocator.Temp);
			for (int i = 0; i < sickEvents.Length; i++)
			{
				if (sickEvents[i].duration.m_EndFrame < this.simulationSystem.frameIndex)
				{
					EntityManager.AddComponent<Deleted>(sickEventEntities[i]);
				}
			}

			NativeArray<Entity> curedEntities = this.removeDiseaseQuery.ToEntityArray(Allocator.Temp);
			NativeArray<CurrentDisease> curedComponents = this.removeDiseaseQuery.ToComponentDataArray<CurrentDisease>(Allocator.Temp);
			for (int i = 0; i <  curedEntities.Length; i++)
			{
				//if (curedComponents[i].minFrame <  this.simulationSystem.frameIndex)
				{
					EntityManager.RemoveComponent<CurrentDisease>(curedEntities[i]);
				}
			}

			this.applyDiseasePenalty(false, this.getDiseasePenalty());

			Entity selected = this.getSelected();

			bool updatedSelection = selected != this.selectedEntity;
			if (updatedSelection)
			{				
				this.selectedEntity = selected;
			}

			if (this.forceSickAction.WasPressedThisFrame() && EntityManager.Exists(selected))
			{
				if (EntityManager.HasComponent<Building>(this.selectedEntity))
				{
					NativeArray<Entity> citizens = this.allHealthyCitizenInBuildingQuery.ToEntityArray(Allocator.Temp);
					NativeArray<CurrentBuilding> currentBuildings = this.allHealthyCitizenInBuildingQuery.ToComponentDataArray<CurrentBuilding>(Allocator.Temp);
					for (int i = 0; i < currentBuildings.Length; ++i)
					{
						if (currentBuildings[i].m_CurrentBuilding == this.selectedEntity)
						{
							this.makeCitizenSick(citizens[i]);
						}
					}
				}
				else
				{
					if (!EntityManager.TryGetComponent<Game.Creatures.Resident>(this.selectedEntity, out var resident) && EntityManager.Exists(resident.m_Citizen))
					{
						return;
					}
					if (!EntityManager.HasComponent<HealthProblem>(resident.m_Citizen))
					{
						this.makeCitizenSick(resident.m_Citizen);
					}
				}
			}

			/*if (this.forceAllSickAction.WasPressedThisFrame())
			{
				NativeArray<Entity> citizens = this.allHealthyCitizenQuery.ToEntityArray(Allocator.Temp);
				this.makeCitizensSick(citizens);
			}*/

			if (this.forceDangerAction.WasPressedThisFrame() && EntityManager.Exists(selected))
			{

				if (!EntityManager.TryGetComponent<Building>(this.selectedEntity, out var building))
				{
					return;
				}
				if (!EntityManager.HasComponent<InDanger>(this.selectedEntity))
				{
					this.makeDanger(this.selectedEntity, this.simulationSystem.frameIndex);
				}
			}
		}

		public void makeAllCitizensSick()
		{
			NativeArray<Entity> citizens = this.allHealthyCitizenQuery.ToEntityArray(Allocator.Temp);
			this.makeCitizensSick(citizens);
		}

		private const float EFFICIENCY_PENALTY = .5110110101f;

		private void checkHospitals()
		{
			NativeArray<Entity> hospitals = this.hospitalQuery.ToEntityArray(Allocator.Temp);
			NativeArray<Game.Buildings.Hospital> hospitalData = this.hospitalQuery.ToComponentDataArray<Game.Buildings.Hospital>(Allocator.Temp);
			for (int i = 0; i < hospitals.Length; ++i)
			{
				if ((hospitalData[i].m_Flags & HospitalFlags.HasRoomForPatients) == 0)
				{
					if (!EntityManager.HasBuffer<Efficiency>(hospitals[i]))
					{
						EntityManager.AddBuffer<Efficiency>(hospitals[i]);
					}

					if (EntityManager.TryGetBuffer<Efficiency>(hospitals[i], false, out var efficiencies))
					{
						bool foundExisting = false;
						for (int j = 0; j < efficiencies.Length; ++j)
						{
							if (efficiencies[j].m_Efficiency == EFFICIENCY_PENALTY)
							{
								foundExisting = true;
								break;
							}
						}

						if (!foundExisting)
						{
							efficiencies.Add(new Efficiency() { m_Efficiency = EFFICIENCY_PENALTY, m_Factor = EfficiencyFactor.Count });
						}
					}
				} 
				else if (EntityManager.TryGetBuffer<Efficiency>(hospitals[i], false, out var efficiencies))
				{
					for (int j = 0; j < efficiencies.Length; ++j)
					{
						if (efficiencies[j].m_Efficiency == EFFICIENCY_PENALTY)
						{
							Mod.log.Info("Removing efficiency " + j.ToString() + " from " + hospitals[i].ToString());
							efficiencies.RemoveAt(j);
							Mod.log.Info("Successfully removed efficiency " + j.ToString() + " from " + hospitals[i].ToString());
						}
					}
				}
			}
		}

		public void applyDiseasePenalty(bool force, byte amount)
		{
			if (amount == 0)
			{
				return;
			}

			if (force || this.simulationSystem.frameIndex % 1500 == 0)
			{
				NativeArray<Citizen> citizenData = this.diseaseQuery.ToComponentDataArray<Citizen>(Allocator.Temp);
				NativeArray<HealthProblem> healthData = this.diseaseQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
				NativeArray<Entity> citizens = this.diseaseQuery.ToEntityArray(Allocator.Temp);
				int deathChance = Mod.INSTANCE.m_Setting.ccDeathChance;
				Random random = new Random();
				for (int i = 0; i < citizenData.Length; ++i)
				{
					if ((healthData[i].m_Flags & HealthProblemFlags.Dead) > 0)
					{
						EntityManager.RemoveComponent<CurrentDisease>(citizens[i]);
						continue;
					}
					if (EntityManager.TryGetComponent<TravelPurpose>(citizens[i], out var travelPurpose) && travelPurpose.m_Purpose == Purpose.Hospital &&
						EntityManager.TryGetComponent<CurrentTransport>(citizens[i], out var currentTransport) &&
						EntityManager.TryGetComponent<Target>(currentTransport.m_CurrentTransport, out var dest) &&
						(!EntityManager.TryGetComponent<CurrentVehicle>(currentTransport.m_CurrentTransport, out var vehicle) ||
							!EntityManager.HasComponent<Game.Vehicles.Ambulance>(vehicle.m_Vehicle)) &&
						EntityManager.TryGetComponent<Game.Buildings.Hospital>(dest.m_Target, out var hospital) &&
						(hospital.m_Flags & HospitalFlags.HasRoomForPatients) == 0
						)
					{
						Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
						EntityManager.AddComponentData(e, new ResetTrip
						{
							m_Creature = currentTransport.m_CurrentTransport,
							m_Target = Entity.Null,
							//m_DivertPurpose = Purpose.Hospital
						});
						
						//EntityManager.SetComponentData(citizens[i], new TravelPurpose() { m_Purpose = Purpose.None, m_Resource = Game.Economy.Resource.NoResource});
						//EntityManager.RemoveComponent<Target>(currentTransport.m_CurrentTransport);
					}
					if (EntityManager.TryGetComponent<CurrentBuilding>(citizens[i], out var building))
					{
						if (EntityManager.TryGetComponent<CurrentTransport>(citizens[i], out var t))
						{

						}
						else if (EntityManager.HasComponent<Game.Buildings.Hospital>(building.m_CurrentBuilding))
						{
							EntityManager.RemoveComponent<CurrentDisease>(citizens[i]);
						}
					}

					Citizen citizen = citizenData[i];
					if (citizen.m_Health <= amount)
					{
						citizen.m_Health = 0;
					}
					else
					{
						citizen.m_Health -= amount;
						EntityManager.SetComponentData(citizens[i], citizen);
					}

					if (deathChance > 0 && citizen.m_Health < 5 && random.Next(0, 100) < deathChance)
					{
						this.killCitizen(citizens[i]);
						EntityManager.RemoveComponent<CurrentDisease>(citizens[i]);
					}
				}
			}
		}

		private Entity getSelected()
		{
			return toolSystem.selected;
		}

		private PrefabID sicknessEventPrefab = new PrefabID("EventPrefab", "Generic Sickness");
		private PrefabID suddenDeathPrefab = new PrefabID("EventPrefab", "Sudden Death");
		private PrefabSystem prefabSystem;
		private EntityArchetype addHealthProblemArchetype;

		public void makeCitizensSick(NativeArray<Entity> citizens)
		{
			if (this.prefabSystem.TryGetPrefab(sicknessEventPrefab, out PrefabBase prefabBase))
			{
				this.prefabSystem.TryGetEntity(prefabBase, out var prefabEntity);

				EventData componentData = EntityManager.GetComponentData<EventData>(prefabEntity);

				foreach (var citizen in citizens)
				{
					this.makeCitizenSick(citizen, prefabEntity, componentData.m_Archetype);
				}
			}
		}

		private void makeCitizenSick(Entity target,
			Entity prefabEntity,
			EntityArchetype prefabArchetype)
		{
			Entity eventEntity = EntityManager.CreateEntity(prefabArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(prefabEntity));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = target });

			EntityManager.AddComponent<CurrentDisease>(target);
			//EntityManager.SetComponentData(target, new Contagious() { minFrame = this.simulationSystem.frameIndex + 1000 });
		}

		private void killCitizen(Entity target, Entity deathPrefab, EntityArchetype deathArchetype)
		{
			Entity eventEntity = EntityManager.CreateEntity(deathArchetype);

			EntityManager.AddComponent<PrefabRef>(eventEntity);
			EntityManager.SetComponentData(eventEntity, new PrefabRef(deathPrefab));
			EntityManager.AddBuffer<TargetElement>(eventEntity);
			EntityManager.GetBuffer<TargetElement>(eventEntity).Add(new TargetElement() { m_Entity = target });
		}

		public void killCitizen(Entity target)
		{
			if (this.prefabSystem.TryGetPrefab(this.suddenDeathPrefab, out PrefabBase prefabBase))
			{
				this.prefabSystem.TryGetEntity(prefabBase, out var prefabEntity);

				EventData componentData = EntityManager.GetComponentData<EventData>(prefabEntity);

				this.killCitizen(target, prefabEntity, componentData.m_Archetype);
			}
		}

		public void makeCitizenSick(Entity target)
		{
			if (this.prefabSystem.TryGetPrefab(this.sicknessEventPrefab, out PrefabBase prefabBase))
			{
				this.prefabSystem.TryGetEntity(prefabBase, out var prefabEntity);

				EventData componentData = EntityManager.GetComponentData<EventData>(prefabEntity);

				this.makeCitizenSick(target, prefabEntity, componentData.m_Archetype);
			}
		}

		public void makeSick(Entity target)
		{

			Entity addSicknessProblem = EntityManager.CreateEntity(this.addHealthProblemArchetype);
			EntityManager.SetComponentData(addSicknessProblem, new AddHealthProblem()
			{
				m_Target = target,
				m_Event = Entity.Null,
				m_Flags = HealthProblemFlags.Sick | HealthProblemFlags.RequireTransport
			});

			EntityManager.AddComponent<BatchesUpdated>(target);
			EntityManager.AddComponent<EffectsUpdated>(target);
			EntityManager.AddComponent<BatchesUpdated>(addSicknessProblem);
			EntityManager.AddComponent<EffectsUpdated>(addSicknessProblem);
		}

		public void makeDanger(Entity target, uint currentFrame)
		{
			Entity sicknessEvent = EntityManager.CreateEntity();
			//EntityManager.AddComponent<PrefabRef>(sicknessEvent);
			EntityManager.AddComponent<Game.Events.Event>(sicknessEvent);
			EntityManager.AddComponent<Duration>(sicknessEvent);
			EntityManager.AddComponent<DangerLevel>(sicknessEvent);
			EntityManager.AddComponent<Simulate>(sicknessEvent);
			EntityManager.AddComponent<SicknessEventData>(sicknessEvent);

			Duration duration = new Duration() { m_StartFrame = currentFrame, m_EndFrame = currentFrame + 1000 };
			EntityManager.SetComponentData(sicknessEvent, duration);
			EntityManager.SetComponentData(sicknessEvent, new SicknessEventData() { duration = duration });
			EntityManager.SetComponentData(sicknessEvent, new DangerLevel() { m_DangerLevel = 1f });

			EntityManager.AddComponent<InDanger>(target);
			EntityManager.SetComponentData<InDanger>(target, new InDanger()
			{
				m_EvacuationRequest = Entity.Null,
				m_Event = sicknessEvent,
				m_Flags = DangerFlags.Evacuate,
				m_EndFrame = currentFrame + 1000
			});

			EntityManager.AddComponent<BatchesUpdated>(sicknessEvent);
			EntityManager.AddComponent<BatchesUpdated>(target);
			EntityManager.AddComponent<EffectsUpdated>(target);
			EntityManager.AddComponent<Updated>(target);
		}
	}
}
