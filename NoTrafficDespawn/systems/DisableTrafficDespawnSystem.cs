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

			/*this.stuckObjectRemovalQuery = GetEntityQuery(new EntityQueryDesc
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
				ComponentType.ReadOnly<UnstuckObject>(),
				}
			});*/

			//this.singleFrameDeadlockRemovalAction = new InputAction("renderType", InputActionType.Button);
			//this.singleFrameDeadlockRemovalAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/x").With("Modifier", "<keyboard>/shift");
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
					}

					highlightDirty = false;
				}
				if (!EntityManager.HasComponent<Blocker>(stuckEntity))
				{
					EntityManager.RemoveComponent<StuckObject>(stuckEntity);
					if (this.highlightStuckObjects)
					{
						EntityManager.RemoveComponent<Highlighted>(stuckEntity);
						EntityManager.AddComponent<BatchesUpdated>(stuckEntity);
					}
				}
				else 
				{
					StuckObject stuck = stuckComponents[i];
					if (this.despawnBehavior != DespawnBehavior.NoDespawn) {
						if ((stuck.frameCount+=4) >= this.deadlockLingerFrames && availableRemovalCount > 0)
						{
							if (EntityManager.TryGetComponent(stuckEntity, out PathOwner pathOwner))
							{
								pathOwner.m_State |= PathFlags.Stuck;
								EntityManager.SetComponentData(stuckEntity, pathOwner);
								EntityManager.AddComponent<BatchesUpdated>(stuckEntity);
								--availableRemovalCount;
							}
						}
						else
						{
							EntityManager.SetComponentData(stuckEntity, stuck);
						}
					}
					
					if (this.highlightStuckObjects && !EntityManager.HasComponent<Highlighted>(stuckEntity))
					{
						EntityManager.AddComponent<Highlighted>(stuckEntity);
						EntityManager.AddComponent<BatchesUpdated>(stuckEntity);
					}					
				}
			}

			if (this.highlightStuckObjects)
			{
				NativeArray<Entity> unstuckEntities = this.unstuckObjectQuery.ToEntityArray(Allocator.Temp);
				for (int i = 0; i < unstuckEntities.Length; i++)
				{
					EntityManager.RemoveComponent<StuckObject>(unstuckEntities[i]);
					EntityManager.RemoveComponent<Highlighted>(unstuckEntities[i]);
					EntityManager.RemoveComponent<UnstuckObject>(unstuckEntities[i]);
					EntityManager.AddComponent<BatchesUpdated>(unstuckEntities[i]);
				}
			}
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			//this.singleFrameDeadlockRemovalAction.Enable();
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
			//this.singleFrameDeadlockRemovalAction.Disable();
		}

		private void updateSettings(TrafficDespawnSettings settings)
		{
			this.stuckMovingObjectSystem.Enabled = settings.despawnBehavior == DespawnBehavior.Vanilla;
			this.Enabled = settings.despawnBehavior != DespawnBehavior.Vanilla;

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
		}
	}
}
