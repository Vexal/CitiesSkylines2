using Colossal;
using Game.Common;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace EmploymentTracker
{
	[BurstCompile]
	public struct EntitySearchJob : IJobChunk
	{
		[ReadOnly]
		public Entity searchTarget;
		[ReadOnly]
		public Entity searchTarget2;
		[ReadOnly]
		public bool hasTarget2;
		[ReadOnly]
		public ComponentTypeHandle<Target> targetHandle;
		[ReadOnly]
		public EntityTypeHandle entityHandle;
		public NativeCounter searchCounter;

		public NativeList<Entity> results;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetHandle);
			NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);
			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			int count = 0;
			while (chunkIterator.NextEntityIndex(out var i))
			{
				Target entityTarget = targets[i];
				if (entityTarget.m_Target == this.searchTarget || (this.hasTarget2 && entityTarget.m_Target == this.searchTarget2))
				{
					this.results.Add(entities[i]);
				}

				++count;
			}

			this.searchCounter.Count += count;
		}
	}
}
