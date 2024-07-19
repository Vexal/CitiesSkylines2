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
	public struct TagStuckObjectsJobNoHighlight : IJobChunk
	{
		public EntityCommandBuffer.ParallelWriter commandBuffer;
		[ReadOnly]
		public EntityTypeHandle m_EntityType;

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
		public long maxTraversalCount;
		[ReadOnly]
		public byte minStuckSpeed;
		[ReadOnly]
		public bool deadlocksOnly;

		public ComponentTypeHandle<PathOwner> m_PathOwnerType;
		public ComponentTypeHandle<AnimalCurrentLane> m_AnimalCurrentLaneType;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityType);
			NativeArray<Blocker> nativeArray2 = chunk.GetNativeArray(ref m_BlockerType);
			NativeArray<GroupMember> nativeArray3 = chunk.GetNativeArray(ref m_GroupMemberType);
			NativeArray<CurrentVehicle> nativeArray4 = chunk.GetNativeArray(ref m_CurrentVehicleType);
			NativeArray<RideNeeder> nativeArray5 = chunk.GetNativeArray(ref m_RideNeederType);
			NativeArray<Target> nativeArray6 = chunk.GetNativeArray(ref m_TargetType);
			NativeArray<PathOwner> nativeArray7 = chunk.GetNativeArray(ref m_PathOwnerType);
			NativeArray<AnimalCurrentLane> nativeArray8 = chunk.GetNativeArray(ref m_AnimalCurrentLaneType);

			//All entities in the same chunk have the same component set, so only need to check before the loop
			bool wasStuck = chunk.Has<StuckObject>();

			for (int i = 0; i < nativeArray2.Length; i++)
			{
				Blocker blocker = nativeArray2[i];
				if (!(blocker.m_Blocker != Entity.Null) || blocker.m_MaxSpeed >= this.minStuckSpeed)
				{
					if (wasStuck)
					{
						this.commandBuffer.RemoveComponent<StuckObject>(unfilteredChunkIndex, nativeArray[i]);
					}

					continue;
				}

				Entity entity = nativeArray[i];
				Entity entity2 = Entity.Null;
				if (nativeArray4.Length != 0)
				{
					entity2 = nativeArray4[i].m_Vehicle;
				}
				else if (nativeArray5.Length != 0)
				{
					RideNeeder rideNeeder = nativeArray5[i];
					if (m_DispatchedData.TryGetComponent(rideNeeder.m_RideRequest, out var componentData))
					{
						entity2 = componentData.m_Handler;
					}
				}
				else if (nativeArray3.Length != 0)
				{
					GroupMember groupMember = nativeArray3[i];
					if (m_CurrentVehicleData.TryGetComponent(groupMember.m_Leader, out var componentData2))
					{
						entity2 = componentData2.m_Vehicle;
					}
				}

				if (nativeArray6.Length != 0 && entity2 == Entity.Null)
				{
					entity2 = nativeArray6[i].m_Target;
				}

				bool flag;
				if (entity2 != Entity.Null)
				{
					if (m_ControllerData.TryGetComponent(entity2, out var componentData3))
					{
						entity2 = componentData3.m_Controller;
					}

					flag = IsBlocked(entity, entity2, blocker);
				}
				else
				{
					flag = IsBlocked(entity, blocker);
				}

				if (!flag)
				{
					if (wasStuck)
					{
						this.commandBuffer.RemoveComponent<StuckObject>(unfilteredChunkIndex, nativeArray[i]);
					}

					continue;
				}

				if (nativeArray7.Length != 0)
				{
					PathOwner value = nativeArray7[i];
					if ((value.m_State & PathFlags.Pending) == 0)
					{
						if (entity != Entity.Null)
						{
							if (!wasStuck)
							{
								this.commandBuffer.AddComponent(unfilteredChunkIndex, entity, new StuckObject(0));
							}
						}
					}
				}
				else if (nativeArray8.Length != 0)
				{
					AnimalCurrentLane value2 = nativeArray8[i];
					value2.m_Flags |= CreatureLaneFlags.Stuck;
					nativeArray8[i] = value2;
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
