using Game.Common;
using Game.Creatures;
using Game.Pathfind;
using Game.Simulation;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace NoTrafficDespawn
{
	[BurstCompile]
	public struct TagStuckObjectsJob : IJobChunk
	{
		public EntityCommandBuffer.ParallelWriter commandBuffer;
		[ReadOnly]
		public EntityTypeHandle entityTypeHandle;

		[ReadOnly]
		public ComponentTypeHandle<Blocker> m_BlockerType;

		[ReadOnly]
		public ComponentTypeHandle<GroupMember> m_GroupMemberType;

		[ReadOnly]
		public ComponentTypeHandle<CurrentVehicle> m_CurrentVehicleType;

		[ReadOnly]
		public ComponentTypeHandle<RideNeeder> m_RideNeederType;

		[ReadOnly]
		public ComponentTypeHandle<Target> m_TargetType;

		[ReadOnly]
		public ComponentTypeHandle<Car> m_CarType;

		[ReadOnly]
		public ComponentLookup<Blocker> m_BlockerData;

		[ReadOnly]
		public ComponentLookup<Controller> m_ControllerData;

		[ReadOnly]
		public ComponentLookup<CurrentVehicle> m_CurrentVehicleData;

		[ReadOnly]
		public ComponentLookup<Dispatched> m_DispatchedData;
		[ReadOnly]
		public ComponentLookup<StuckObject> stuckObjectLookup;
		[ReadOnly]
		public ComponentLookup<UnstuckObject> unstuckObjectLookup;

		[NativeDisableParallelForRestriction]
		public ComponentLookup<CarCurrentLane> m_CarCurrentLaneData;

		[ReadOnly]
		public ComponentLookup<ParkedCar> m_ParkedCarData;

		[ReadOnly]
		public ComponentLookup<ParkedTrain> m_ParkedTrainData;

		[ReadOnly]
		public long maxTraversalCount;
		[ReadOnly]
		public byte minStuckSpeed;
		[ReadOnly]
		public bool deadlocksOnly;
		[ReadOnly]
		public bool highlightStuckObjects;

		public ComponentTypeHandle<PathOwner> m_PathOwnerType;
		public ComponentTypeHandle<AnimalCurrentLane> m_AnimalCurrentLaneType;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
			NativeArray<Blocker> blockers = chunk.GetNativeArray(ref m_BlockerType);
			NativeArray<GroupMember> nativeArray3 = chunk.GetNativeArray(ref m_GroupMemberType);
			NativeArray<CurrentVehicle> vehicles = chunk.GetNativeArray(ref m_CurrentVehicleType);
			NativeArray<RideNeeder> nativeArray5 = chunk.GetNativeArray(ref m_RideNeederType);
			NativeArray<Target> targets = chunk.GetNativeArray(ref m_TargetType);
			NativeArray<PathOwner> pathOwners = chunk.GetNativeArray(ref m_PathOwnerType);
			NativeArray<AnimalCurrentLane> nativeArray8 = chunk.GetNativeArray(ref m_AnimalCurrentLaneType);

			//All entities in the same chunk have the same component set, so only need to check before the loop
			bool wasStuck = chunk.Has<StuckObject>();
			bool wasUnstuck = chunk.Has<UnstuckObject>();
			bool hasCar = chunk.Has(ref m_CarType);

			for (int i = 0; i < blockers.Length; i++)
			{
				Blocker blocker = blockers[i];
				Entity entity = entities[i];

				bool notBlocked = false;
				if (blocker.m_Blocker == Entity.Null || blocker.m_Type == BlockerType.Temporary)
				{
					notBlocked = true;
				}

				if (hasCar && blocker.m_Type == BlockerType.Crossing)
				{
					Entity blockingEntity = blocker.m_Blocker;
					if (this.m_ControllerData.TryGetComponent(blocker.m_Blocker, out var componentData))
					{
						blockingEntity = componentData.m_Controller;
					}

					if (this.m_CarCurrentLaneData.TryGetComponent(blockingEntity, out var componentData2))
					{
						componentData2.m_LaneFlags |= CarLaneFlags.RequestSpace;
						this.m_CarCurrentLaneData[blockingEntity] = componentData2;
					}
				}

				if (blocker.m_MaxSpeed >= this.minStuckSpeed)
				{
					notBlocked = true;
				}

				if (notBlocked)
				{
					if (wasStuck)
					{
						this.commandBuffer.RemoveComponent<StuckObject>(unfilteredChunkIndex, entity);
						if (this.highlightStuckObjects)
						{
							this.commandBuffer.AddComponent<UnstuckObject>(unfilteredChunkIndex, entity);
						}
					}

					continue;
				}


				bool blocked = false;
				if (m_ParkedTrainData.HasComponent(blocker.m_Blocker) || (!hasCar && m_ParkedCarData.HasComponent(blocker.m_Blocker)))
				{
					blocked = true;
				}
				else
				{
					Entity entity2 = Entity.Null;
					if (vehicles.Length != 0)
					{
						entity2 = vehicles[i].m_Vehicle;
					}
					else if (nativeArray5.Length != 0)
					{
						RideNeeder rideNeeder = nativeArray5[i];
						if (this.m_DispatchedData.TryGetComponent(rideNeeder.m_RideRequest, out var componentData))
						{
							entity2 = componentData.m_Handler;
						}
					}
					else if (nativeArray3.Length != 0)
					{
						GroupMember groupMember = nativeArray3[i];
						if (this.m_CurrentVehicleData.TryGetComponent(groupMember.m_Leader, out var componentData2))
						{
							entity2 = componentData2.m_Vehicle;
						}
					}

					if (targets.Length != 0 && entity2 == Entity.Null)
					{
						entity2 = targets[i].m_Target;
					}

					if (entity2 != Entity.Null)
					{
						if (this.m_ControllerData.TryGetComponent(entity2, out var componentData3))
						{
							entity2 = componentData3.m_Controller;
						}

						blocked = IsBlocked(entity, entity2, blocker);
					}
					else
					{
						blocked = IsBlocked(entity, blocker);
					}
				}

				if (blocked)
				{
					if (pathOwners.Length != 0)
					{
						if ((pathOwners[i].m_State & PathFlags.Pending) == 0)
						{
							if (!wasStuck)
							{
								this.commandBuffer.AddComponent(unfilteredChunkIndex, entity, new StuckObject(0));
							}
							if (wasUnstuck)
							{
								this.commandBuffer.RemoveComponent<UnstuckObject>(unfilteredChunkIndex, entity);
							}

							continue;
						}
					}
					else if (nativeArray8.Length != 0)
					{
						AnimalCurrentLane value2 = nativeArray8[i];
						value2.m_Flags |= CreatureLaneFlags.Stuck;
						nativeArray8[i] = value2;
						continue;
					}
				}

				if (wasStuck)
				{
					if (this.highlightStuckObjects)
					{
						this.commandBuffer.AddComponent<UnstuckObject>(unfilteredChunkIndex, entity);
					}

					this.commandBuffer.RemoveComponent<StuckObject>(unfilteredChunkIndex, entity);
				}
			}
		}

		private bool IsBlocked(Entity entity, Blocker blocker)
		{
			int num = 0;
			if (m_ControllerData.TryGetComponent(blocker.m_Blocker, out var componentData))
			{
				blocker.m_Blocker = componentData.m_Controller;
			}

			while (m_BlockerData.HasComponent(blocker.m_Blocker))
			{
				if (blocker.m_Blocker == entity)
				{
					return true;
				}
				else if (++num >= this.maxTraversalCount)
				{
					return !this.deadlocksOnly;
				}

				blocker = m_BlockerData[blocker.m_Blocker];
				if (blocker.m_Blocker == Entity.Null)
				{
					return false;
				}

				if (blocker.m_Type == BlockerType.Temporary)
				{
					return false;
				}

				if (blocker.m_MaxSpeed >= this.minStuckSpeed)
				{
					return false;
				}

				if (m_ControllerData.TryGetComponent(blocker.m_Blocker, out componentData))
				{
					blocker.m_Blocker = componentData.m_Controller;
				}
			}

			return false;
		}

		private bool IsBlocked(Entity entity1, Entity entity2, Blocker blocker)
		{
			int num = 0;
			if (m_ControllerData.TryGetComponent(blocker.m_Blocker, out var componentData))
			{
				blocker.m_Blocker = componentData.m_Controller;
			}

			while (m_BlockerData.HasComponent(blocker.m_Blocker))
			{
				if (blocker.m_Blocker == entity1 || blocker.m_Blocker == entity2)
				{
					return true;
				}
				else if (++num >= this.maxTraversalCount)
				{
					return !this.deadlocksOnly;
				}

				blocker = m_BlockerData[blocker.m_Blocker];
				if (blocker.m_Blocker == Entity.Null)
				{
					return false;
				}

				if (blocker.m_Type == BlockerType.Temporary)
				{
					return false;
				}

				if (blocker.m_MaxSpeed >= this.minStuckSpeed)
				{
					return false;
				}

				if (m_ControllerData.TryGetComponent(blocker.m_Blocker, out componentData))
				{
					blocker.m_Blocker = componentData.m_Controller;
				}
			}

			return false;
		}
	}
}
