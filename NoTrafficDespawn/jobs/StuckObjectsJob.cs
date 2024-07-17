using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace NoTrafficDespawn
{
	public abstract class StuckObjectsJob : IJobChunk
	{
		public abstract void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask);
	}
}
