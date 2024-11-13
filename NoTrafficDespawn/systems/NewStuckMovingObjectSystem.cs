using Game;
using Game.Common;
using Game.Creatures;
using Game.Pathfind;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using Unity.Entities;

namespace NoTrafficDespawn
{
	public partial class NewStuckMovingObjectSystem : GameSystemBase
    {
		private EntityQuery blockedEntityQuery;
		private EntityCommandBufferSystem entityCommandBufferSystem;
		private DisableTrafficDespawnSystem disableTrafficDespawnSystem;

		public override int GetUpdateInterval(SystemUpdatePhase phase)
		{
			return 4;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			this.entityCommandBufferSystem = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			this.disableTrafficDespawnSystem = World.GetOrCreateSystemManaged<DisableTrafficDespawnSystem>();
			blockedEntityQuery = GetEntityQuery(ComponentType.ReadOnly<Blocker>(), ComponentType.ReadOnly<UpdateFrame>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
			RequireForUpdate(blockedEntityQuery);
		}

		protected override void OnUpdate()
		{
			//uint index = (m_SimulationSystem.frameIndex >> 2) % 16;
			//blockedEntityQuery.ResetFilter();
			//blockedEntityQuery.SetSharedComponentFilter(new UpdateFrame(index));
			if (this.disableTrafficDespawnSystem.highlightStuckObjects)
			{
				TagStuckObjectsJob stuckCheckJob = default;
				stuckCheckJob.m_EntityType = SystemAPI.GetEntityTypeHandle();
				stuckCheckJob.m_BlockerType = SystemAPI.GetComponentTypeHandle<Blocker>(isReadOnly: true);
				stuckCheckJob.m_GroupMemberType = SystemAPI.GetComponentTypeHandle<GroupMember>(isReadOnly: true);
				stuckCheckJob.m_CurrentVehicleType = SystemAPI.GetComponentTypeHandle<CurrentVehicle>(isReadOnly: true);
				stuckCheckJob.m_RideNeederType = SystemAPI.GetComponentTypeHandle<RideNeeder>(isReadOnly: true);
				stuckCheckJob.m_TargetType = SystemAPI.GetComponentTypeHandle<Target>(isReadOnly: true);
				stuckCheckJob.m_BlockerData = SystemAPI.GetComponentLookup<Blocker>(isReadOnly: true);
				stuckCheckJob.m_ControllerData = SystemAPI.GetComponentLookup<Controller>(isReadOnly: true);
				stuckCheckJob.m_CurrentVehicleData = SystemAPI.GetComponentLookup<CurrentVehicle>(isReadOnly: true);
				stuckCheckJob.m_DispatchedData = SystemAPI.GetComponentLookup<Dispatched>(isReadOnly: true);
				stuckCheckJob.m_PathOwnerType = SystemAPI.GetComponentTypeHandle<PathOwner>();
				stuckCheckJob.m_AnimalCurrentLaneType = SystemAPI.GetComponentTypeHandle<AnimalCurrentLane>();
				stuckCheckJob.stuckObjectLookup = SystemAPI.GetComponentLookup<StuckObject>(true);
				stuckCheckJob.unstuckObjectLookup = SystemAPI.GetComponentLookup<UnstuckObject>(true);
				stuckCheckJob.minStuckSpeed = (byte)this.disableTrafficDespawnSystem.maxStuckObjectSpeed;
				stuckCheckJob.maxTraversalCount = this.disableTrafficDespawnSystem.deadlockSearchDepth;
				stuckCheckJob.deadlocksOnly = this.disableTrafficDespawnSystem.despawnBehavior == DespawnBehavior.DespawnDeadlocksOnly;
				EntityCommandBuffer entityCommandBuffer = this.entityCommandBufferSystem.CreateCommandBuffer();
				stuckCheckJob.commandBuffer = this.entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

				base.Dependency = JobChunkExtensions.ScheduleParallel(stuckCheckJob, blockedEntityQuery, base.Dependency);
			}
			else
			{
				TagStuckObjectsJobNoHighlight stuckCheckJob = default;
				stuckCheckJob.m_EntityType = GetEntityTypeHandle();
				stuckCheckJob.m_BlockerType = GetComponentTypeHandle<Blocker>(isReadOnly: true);
				stuckCheckJob.m_GroupMemberType = GetComponentTypeHandle<GroupMember>(isReadOnly: true);
				stuckCheckJob.m_CurrentVehicleType = GetComponentTypeHandle<CurrentVehicle>(isReadOnly: true);
				stuckCheckJob.m_RideNeederType = GetComponentTypeHandle<RideNeeder>(isReadOnly: true);
				stuckCheckJob.m_TargetType = GetComponentTypeHandle<Target>(isReadOnly: true);
				stuckCheckJob.m_BlockerData = GetComponentLookup<Blocker>(isReadOnly: true);
				stuckCheckJob.m_ControllerData = GetComponentLookup<Controller>(isReadOnly: true);
				stuckCheckJob.m_CurrentVehicleData = GetComponentLookup<CurrentVehicle>(isReadOnly: true);
				stuckCheckJob.m_DispatchedData = GetComponentLookup<Dispatched>(isReadOnly: true);
				stuckCheckJob.m_PathOwnerType = GetComponentTypeHandle<PathOwner>();
				stuckCheckJob.m_AnimalCurrentLaneType = GetComponentTypeHandle<AnimalCurrentLane>();
				stuckCheckJob.stuckObjectLookup = GetComponentLookup<StuckObject>(true);
				stuckCheckJob.minStuckSpeed = (byte)this.disableTrafficDespawnSystem.maxStuckObjectSpeed;
				stuckCheckJob.maxTraversalCount = this.disableTrafficDespawnSystem.deadlockSearchDepth;
				stuckCheckJob.deadlocksOnly = this.disableTrafficDespawnSystem.despawnBehavior == DespawnBehavior.DespawnDeadlocksOnly;
				EntityCommandBuffer entityCommandBuffer = this.entityCommandBufferSystem.CreateCommandBuffer();
				stuckCheckJob.commandBuffer = this.entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

				base.Dependency = JobChunkExtensions.ScheduleParallel(stuckCheckJob, blockedEntityQuery, base.Dependency);
			}

			this.entityCommandBufferSystem.AddJobHandleForProducer(base.Dependency);
		}
	}
}