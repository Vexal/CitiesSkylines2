using Colossal.Entities;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using Unity.Collections;
using Unity.Entities;

namespace Click2Sick
{
	internal partial class ClickSicknessSystem : UISystemBase
	{
		ToolSystem toolSystem;
		Entity selectedEntity;
		private EntityQuery allHealthyCitizenQuery;
		private EntityQuery allHealthyCitizenInBuildingQuery;
		private EntityQuery diseaseQuery;
		private EntityQuery allCitizenQuery;

		private PrefabID sicknessEventPrefab = new PrefabID("EventPrefab", "Generic Sickness");
		private EntityArchetype resetTripArchetype;
		private PrefabSystem prefabSystem;
		private Entity nextSickTarget = Entity.Null;
		private Entity nextHealTarget = Entity.Null;
		private Entity nextDecreaseHealthTarget = Entity.Null;
		private bool makeAllSick = false;
		private bool healAll = false;
		private byte decreaseHealthlAll = 0;

		protected override void OnCreate()   
		{
			base.OnCreate();
			Mod.INSTANCE.m_Setting.clickSicknessSystem = this;

			Mod.makeSelectedSickAction.onInteraction += (_, phase) =>
			{
				if (GameManager.instance.gameMode == Game.GameMode.Game)
				{
					this.selectedEntity = this.getSelected();
					this.nextSickTarget = this.selectedEntity;
					this.nextHealTarget = Entity.Null;
					this.nextDecreaseHealthTarget = Entity.Null;
				}
			};

			Mod.healSelectedAction.onInteraction += (_, phase) =>
			{
				if (GameManager.instance.gameMode == Game.GameMode.Game)
				{
					this.selectedEntity = this.getSelected();
					this.nextHealTarget = this.selectedEntity;
					this.nextSickTarget = Entity.Null;
					this.nextDecreaseHealthTarget = Entity.Null;
				}
			};

			Mod.decreaseHealthSelectedAction.onInteraction += (_, phase) =>
			{
				if (GameManager.instance.gameMode == Game.GameMode.Game)
				{
					this.selectedEntity = this.getSelected();
					this.nextDecreaseHealthTarget = this.selectedEntity;
					this.nextHealTarget = Entity.Null;
					this.nextSickTarget = Entity.Null;
				}
			};

			this.prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			this.resetTripArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<ResetTrip>());

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

			this.allCitizenQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
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

			this.diseaseQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<HealthProblem>(),
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (this.nextSickTarget != Entity.Null && EntityManager.Exists(this.nextSickTarget))
			{
				this.makeCitizenSick(this.nextSickTarget);
				this.nextSickTarget = Entity.Null;
			}
			else if (this.nextHealTarget != Entity.Null && EntityManager.Exists(this.nextHealTarget))
			{
				this.healCitizen(this.nextHealTarget);
				this.nextSickTarget = Entity.Null;
			}
			else if (this.nextDecreaseHealthTarget != Entity.Null && EntityManager.Exists(this.nextDecreaseHealthTarget))
			{
				this.decreaseCitizenHealth(this.nextDecreaseHealthTarget, 25);
				this.nextDecreaseHealthTarget = Entity.Null;
			}

			Entity selected = this.getSelected();

			bool updatedSelection = selected != this.selectedEntity;
			if (updatedSelection)
			{				
				this.selectedEntity = selected;
			}

			if (this.makeAllSick)
			{
				this.makeAllCitizensSick();
				this.reset();
			}
			else if (this.healAll)
			{
				this.healAllCitizens();
				this.reset();
			}
			
			if (this.decreaseHealthlAll > 0)
			{
				this.decreaseAllCitizenHealth(this.decreaseHealthlAll);
				this.decreaseHealthlAll = 0;
			}
		}

		private void reset()
		{
			this.makeAllSick = false;
			this.healAll = false;
		}
		
		public void triggerMakeAllCitizensSick()
		{
			this.makeAllSick = true;
		}

		public void triggerHealAllCitizens()
		{
			this.healAll = true;
		}

		public void triggerDecreaseHealthAll()
		{
			if (byte.MaxValue - 25 <= this.decreaseHealthlAll)
			{
				this.decreaseHealthlAll = byte.MaxValue;
			}
			else
			{
				this.decreaseHealthlAll += 25;
			}
		}

		private void makeAllCitizensSick()
		{
			NativeArray<Entity> citizens = this.allHealthyCitizenQuery.ToEntityArray(Allocator.Temp);
			this.makeCitizensSick(citizens);
		}

		private void healAllCitizens()
		{
			NativeArray<Entity> citizens = this.diseaseQuery.ToEntityArray(Allocator.Temp);
			NativeArray<HealthProblem> healthProblems = this.diseaseQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);

			for (int i = 0; i < citizens.Length; ++i)
			{
				if (isSick(healthProblems[i]))
				{
					EntityManager.RemoveComponent<HealthProblem>(citizens[i]);
					this.resetCitizenTrip(citizens[i]);
				}
			}
		}

		private void healCitizen(Entity target)
		{
			if (this.tryGetCitizen(target, out var citizen) && this.isCitizenSick(citizen))
			{
				EntityManager.RemoveComponent<HealthProblem>(citizen);
				this.resetCitizenTrip(citizen);
			}
		}

		private void decreaseCitizenHealth(Entity target, byte amount)
		{
			if (this.tryGetCitizen(target, out var citizen) && EntityManager.TryGetComponent(citizen, out Citizen citizenData))
			{
				if (amount >= citizenData.m_Health)
				{
					citizenData.m_Health = 0;
				}
				else
				{
					citizenData.m_Health -= amount;
				}

				EntityManager.SetComponentData(citizen, citizenData);
			}
		}

		private void decreaseAllCitizenHealth(byte amount)
		{
			NativeArray<Citizen> citizens = this.allCitizenQuery.ToComponentDataArray<Citizen>(Allocator.Temp);
			NativeArray<Entity> entities = this.allCitizenQuery.ToEntityArray(Allocator.Temp);

			for (int i = 0; i < citizens.Length; ++i)
			{
				Citizen c = citizens[i];
				if (amount >= c.m_Health)
				{
					c.m_Health = 0;
				}
				else
				{
					c.m_Health -= amount;
				}

				EntityManager.SetComponentData(entities[i], c);
			}
		}
		
		private Entity getSelected()
		{
			return toolSystem.selected;
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
		}

		public void makeCitizensSick(NativeArray<Entity> citizens)
		{
			if (citizens.Length == 0)
			{
				return;
			}

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

		public void makeCitizenSick(Entity target)
		{
			if (!EntityManager.Exists(target))
			{
				return;
			}

			NativeList<Entity> targets = new NativeList<Entity>(Allocator.Temp);

			if (EntityManager.HasComponent<Building>(target))
			{
				NativeArray<Entity> citizens = this.allHealthyCitizenInBuildingQuery.ToEntityArray(Allocator.Temp);
				NativeArray<CurrentBuilding> currentBuildings = this.allHealthyCitizenInBuildingQuery.ToComponentDataArray<CurrentBuilding>(Allocator.Temp);
				for (int i = 0; i < currentBuildings.Length; ++i)
				{
					if (currentBuildings[i].m_CurrentBuilding == target)
					{
						targets.Add(citizens[i]);
					}
				}
			}
			else
			{
				if (this.tryGetCitizen(target, out var citizen) && !EntityManager.HasComponent<HealthProblem>(citizen))
				{
					targets.Add(citizen);
				}
			}

			this.makeCitizensSick(targets.AsArray());
		}

		private bool tryGetCitizen(Entity target, out Entity citizen)
		{
			if (EntityManager.TryGetComponent<Game.Creatures.Resident>(target, out var resident) && EntityManager.Exists(resident.m_Citizen))
			{
				citizen = resident.m_Citizen;
				return true;
			}
			else
			{
				citizen = Entity.Null;
				return false;
			}
		}

		private void resetCitizenTrip(Entity citizen)
		{
			if (EntityManager.TryGetComponent<TravelPurpose>(citizen, out var travelPurpose) && travelPurpose.m_Purpose == Purpose.Hospital &&
				EntityManager.TryGetComponent<CurrentTransport>(citizen, out var currentTransport)
						)
			{
				Entity e = EntityManager.CreateEntity(this.resetTripArchetype);
				EntityManager.AddComponentData(e, new ResetTrip
				{
					m_Creature = currentTransport.m_CurrentTransport,
					m_Target = Entity.Null
				});
			}
		}

		private bool isCitizenSick(Entity citizen)
		{
			return EntityManager.TryGetComponent<HealthProblem>(citizen, out var healthProblem) && isSick(healthProblem);
		}

		private static bool isSick(HealthProblem healthProblem)
		{
			return ((healthProblem.m_Flags & (HealthProblemFlags.Injured | HealthProblemFlags.Sick)) > 0 && ((healthProblem.m_Flags & HealthProblemFlags.Dead) == 0));
		}
	}
}
