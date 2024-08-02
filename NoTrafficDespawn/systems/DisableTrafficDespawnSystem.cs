using Colossal.Entities;
using Game;
using Game.Buildings;
using Game.Common;
using Game.Creatures;
using Game.Pathfind;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace NoTrafficDespawn
{
	public partial class DisableTrafficDespawnSystem : GameSystemBase
	{
		private StuckMovingObjectSystem stuckMovingObjectSystem;
		private SimulationSystem simulationSystem;
		private EntityQuery stuckObjectQuery;

		//private EntityQuery stuckObjectRemovalQuery;
		private EntityQuery unstuckObjectQuery;
		//private InputAction singleFrameDeadlockRemovalAction;
		private StuckType removeType;
		private bool highlightDirty = false;
		private bool wasHighlighting = false;
		private bool despawnAll;
		private bool despawnCommercialVehicles;
		private bool despawnPedestrians;
		private bool despawnPersonalVehicles;
		private bool despawnPublicTransit;
		private bool despawnServiceVehicles;
		private bool despawnTaxis;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.stuckMovingObjectSystem = World.GetOrCreateSystemManaged<StuckMovingObjectSystem>();
			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

			Mod.INSTANCE.settings.onSettingsApplied += settings =>
			{
				if (settings.GetType() == typeof(TrafficDespawnSettings))
				{
					this.updateSettings((TrafficDespawnSettings)settings);
				}
			};

			this.updateSettings(Mod.INSTANCE.settings);

			this.unstuckObjectQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<UnstuckObject>()
			},
				Any = new ComponentType[]
			{
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>(),
				}
			});

			this.stuckObjectQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<StuckObject>()
			},
				Any = new ComponentType[]
			{
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>(),
				//ComponentType.ReadOnly<Highlighted>(),
				ComponentType.ReadOnly<UnstuckObject>(),
				}
			});
		}

		private int frameCount = 0;
		public DespawnBehavior despawnBehavior;
		public bool highlightStuckObjects;
		public int deadlockLingerFrames;
		public int deadlockSearchDepth;
		public int maxStuckObjectRemovalCount;
		public int maxStuckObjectSpeed;

		protected override void OnUpdate()
		{
			if (this.simulationSystem.selectedSpeed <= 0)
			{
				return;
			}

			if (this.shouldDisable)
			{
				this.cleanupAfterDisable();
				this.Enabled = false;
				return;
			}

			if (frameCount++ % 4 != 0)
			{
				return;
			}

			NativeArray<Entity> stuckEntities = this.stuckObjectQuery.ToEntityArray(Allocator.Temp);
			NativeArray<StuckObject> stuckComponents = this.stuckObjectQuery.ToComponentDataArray<StuckObject>(Allocator.Temp);

			int availableRemovalCount = this.maxStuckObjectRemovalCount;

			for (int i = 0; i < stuckEntities.Length; i++)
			{
				Entity stuckEntity = stuckEntities[i];
				bool updated = false;

				if (highlightDirty)
				{
					if (EntityManager.HasComponent<Highlighted>(stuckEntity))
					{
						EntityManager.RemoveComponent<Highlighted>(stuckEntity);
						EntityManager.AddComponent<BatchesUpdated>(stuckEntity);
					}
					if (EntityManager.HasComponent<UnstuckObject>(stuckEntity))
					{
						EntityManager.RemoveComponent<UnstuckObject>(stuckEntity);
						updated = true;
					}
				}
				if (!EntityManager.HasComponent<Blocker>(stuckEntity))
				{
					EntityManager.RemoveComponent<StuckObject>(stuckEntity);
					updated = true;
					if (this.highlightStuckObjects)
					{
						EntityManager.RemoveComponent<Highlighted>(stuckEntity);
						EntityManager.AddComponent<BatchesUpdated>(stuckEntity);
					}
				}
				else 
				{
					StuckObject stuck = stuckComponents[i];
					if (this.despawnBehavior != DespawnBehavior.NoDespawn) 
					{
						if ((stuck.frameCount += 4) >= this.deadlockLingerFrames && availableRemovalCount > 0)
						{
							if (this.despawnAll ||
								(this.despawnCommercialVehicles && EntityManager.HasComponent<DeliveryTruck>(stuckEntity)) ||
								(this.despawnPedestrians && EntityManager.HasComponent<Creature>(stuckEntity)) ||
								(this.despawnPersonalVehicles && EntityManager.HasComponent<PersonalCar>(stuckEntity)) ||
								(this.despawnPublicTransit && EntityManager.HasComponent<PassengerTransport>(stuckEntity)) ||
								(this.despawnTaxis && EntityManager.HasComponent<Taxi>(stuckEntity)) ||
								(this.despawnServiceVehicles && (
										!EntityManager.HasComponent<Creature>(stuckEntity) &&
										!EntityManager.HasComponent<PersonalCar>(stuckEntity) &&
										!EntityManager.HasComponent<Taxi>(stuckEntity) &&
										!EntityManager.HasComponent<DeliveryTruck>(stuckEntity) &&
										!EntityManager.HasComponent<PassengerTransport>(stuckEntity)
									)
								)
							)
							{
								if (EntityManager.TryGetComponent(stuckEntity, out PathOwner pathOwner))
								{
									pathOwner.m_State |= PathFlags.Stuck;
									//pathOwner.m_State |=  PathFlags.Obsolete;
									EntityManager.SetComponentData(stuckEntity, pathOwner);
									updated = true;
									--availableRemovalCount;
								}
							}
						}
						else
						{
							EntityManager.SetComponentData(stuckEntity, stuck);
							updated = true;
						}
					}
					
					if (this.highlightStuckObjects && !EntityManager.HasComponent<Highlighted>(stuckEntity))
					{
						EntityManager.AddComponent<Highlighted>(stuckEntity);
						EntityManager.AddComponent<BatchesUpdated>(stuckEntity);
					}					
				}

				if (updated && !EntityManager.HasComponent<Updated>(stuckEntity))
				{
					EntityManager.AddComponent<Updated>(stuckEntity);
				}
			}

			highlightDirty = false;

			if (this.highlightStuckObjects)
			{
				NativeArray<Entity> unstuckEntities = this.unstuckObjectQuery.ToEntityArray(Allocator.Temp);
				for (int i = 0; i < unstuckEntities.Length; i++)
				{
					EntityManager.RemoveComponent<StuckObject>(unstuckEntities[i]);
					EntityManager.RemoveComponent<Highlighted>(unstuckEntities[i]);
					EntityManager.RemoveComponent<UnstuckObject>(unstuckEntities[i]);
					EntityManager.AddComponent<BatchesUpdated>(unstuckEntities[i]);
					if (!EntityManager.HasComponent<Updated>(unstuckEntities[i]))
					{
						EntityManager.AddComponent<Updated>(unstuckEntities[i]);
					}
				}
			}
		}

		private bool shouldDisable;

		private void updateSettings(TrafficDespawnSettings settings)
		{
			this.stuckMovingObjectSystem.Enabled = settings.despawnBehavior == DespawnBehavior.Vanilla;
			this.shouldDisable = settings.despawnBehavior == DespawnBehavior.Vanilla;
			if (!this.shouldDisable)
			{
				this.Enabled = true;
			}

			this.despawnBehavior = settings.despawnBehavior;
			this.highlightStuckObjects = settings.highlightStuckObjects;
			this.deadlockLingerFrames = settings.deadlockLingerFrames;
			this.deadlockSearchDepth = settings.deadlockSearchDepth;
			this.maxStuckObjectRemovalCount = settings.maxStuckObjectRemovalCount;
			this.maxStuckObjectSpeed = settings.maxStuckObjectSpeed;
			if (this.wasHighlighting && !this.highlightStuckObjects)
			{
				this.highlightDirty = true;
			}

			this.wasHighlighting = this.highlightStuckObjects;

			this.despawnAll = settings.despawnAll;
			this.despawnCommercialVehicles = settings.despawnCommercialVehicles;
			this.despawnPedestrians = settings.despawnPedestrians;
			this.despawnPersonalVehicles = settings.despawnPersonalVehicles;
			this.despawnPublicTransit = settings.despawnPublicTransit;
			this.despawnServiceVehicles = settings.despawnServiceVehicles;
			this.despawnTaxis = settings.despawnTaxis;
		}

		private void cleanupAfterDisable()
		{
			NativeArray<Entity> cleanupEntities = this.stuckObjectQuery.ToEntityArray(Allocator.Temp);
			for (int i = 0; i < cleanupEntities.Length; ++i)
			{
				if (EntityManager.HasComponent<Highlighted>(cleanupEntities[i]))
				{
					EntityManager.RemoveComponent<Highlighted>(cleanupEntities[i]);
					EntityManager.AddComponent<BatchesUpdated>(cleanupEntities[i]);
				}
				if (EntityManager.HasComponent<UnstuckObject>(cleanupEntities[i]))
				{
					EntityManager.RemoveComponent<UnstuckObject>(cleanupEntities[i]);
					EntityManager.AddComponent<Updated>(cleanupEntities[i]);
				}
				if (EntityManager.HasComponent<StuckObject>(cleanupEntities[i]))
				{
					EntityManager.RemoveComponent<StuckObject>(cleanupEntities[i]);
					EntityManager.AddComponent<Updated>(cleanupEntities[i]);
				}
			}
		}
	}
}
