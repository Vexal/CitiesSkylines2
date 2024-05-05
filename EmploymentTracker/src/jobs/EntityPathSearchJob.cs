using Colossal;
using Game.Pathfind;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace EmploymentTracker
{
	[BurstCompile]
	public struct EntityPathSearchJob : IJobChunk
	{
		[ReadOnly]
		public NativeHashSet<Entity> targets;
		[ReadOnly]
		public BufferTypeHandle<CarNavigationLane> immediateCarLaneHandle;
		[ReadOnly]
		public BufferTypeHandle<PathElement> pathHandle;
		[ReadOnly]
		public ComponentTypeHandle<PathOwner> pathOwnerHandle;
		[ReadOnly]
		public EntityTypeHandle entityHandle;

		public NativeCounter.Concurrent searchCounter;
		public NativeCounter.Concurrent resultCounter;

		public NativeArray<Entity> results;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			BufferAccessor<PathElement> entityPaths = chunk.GetBufferAccessor(ref this.pathHandle);
			NativeArray<PathOwner> pathOwners = chunk.GetNativeArray(ref this.pathOwnerHandle);

			BufferAccessor<CarNavigationLane> carNavigationLanes;
			bool hasCarNavigationLanes = false;
			if (chunk.Has<CarNavigationLane>())
			{
				carNavigationLanes = chunk.GetBufferAccessor(ref this.immediateCarLaneHandle);
				hasCarNavigationLanes = true;
			} 
			else
			{
				carNavigationLanes = default;
			}

			NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);

			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			int count = 0;
			bool reachedLimit = false;
			while (chunkIterator.NextEntityIndex(out var i))
			{
				if (reachedLimit)
				{
					break;
				}

				bool foundTarget = false;
				DynamicBuffer<PathElement> path = entityPaths[i];
				PathOwner pathOwner = pathOwners[i];

				for (int pathIndex = pathOwner.m_ElementIndex; pathIndex < path.Length; ++pathIndex)
				{
					++count;
					if (this.targets.Contains(path[pathIndex].m_Target))
					{
						int resultIndex = this.resultCounter.Increment();
						if (resultIndex < this.results.Length)
						{
							results[resultIndex] = entities[i];
						}
						else
						{
							reachedLimit = true;
						}

						foundTarget = true;

						break;
					}
				}

				if (!foundTarget && hasCarNavigationLanes && !reachedLimit)
				{
					DynamicBuffer<CarNavigationLane> immediateLanes = carNavigationLanes[i];
					for (int pathIndex = 0; pathIndex < immediateLanes.Length; ++pathIndex)
					{
						++count;
						if (this.targets.Contains(immediateLanes[pathIndex].m_Lane))
						{
							int resultIndex = this.resultCounter.Increment();
							if (resultIndex < this.results.Length)
							{
								results[resultIndex] = entities[i];
							}
							else
							{
								reachedLimit = true;
							}

							break;
						}
					}
				}
			}

			this.searchCounter.Increment(count);
		}
	}
}
