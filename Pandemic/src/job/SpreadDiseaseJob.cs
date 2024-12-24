using Colossal;
using Colossal.Mathematics;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Objects;
using Game.Pathfind;
using Game.Routes;
using Game.Vehicles;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pandemic
{
	[BurstCompile]
	public struct SpreadDiseaseJob : IJobParallelForBatch
	{
		[ReadOnly]
		public NativeArray<float3> diseasePositions;
		[ReadOnly]
		public float spreadRadius;
		[ReadOnly]
		public float fleeRadius;
		[ReadOnly]
		public int spreadChance;
		[ReadOnly]
		public NativeArray<float3> citizenPositions;

		[NativeDisableParallelForRestriction]
		public NativeArray<bool> spread;
		[NativeDisableParallelForRestriction]
		public NativeArray<bool> flee;

		public void Execute(int start, int count)
		{
			Random random = new Random();
			for (int i = start; i < start + count; ++i)
			{
				for (int j = 0; j < this.diseasePositions.Length; ++j)
				{
					float distance = math.distance(this.diseasePositions[j], this.citizenPositions[i]);
					if (distance < spreadRadius)
					{
						int norm = (int)((spreadRadius - distance) / spreadRadius * this.spreadChance);
						bool shouldSpread = random.NextInt(10000) < norm;
						if (shouldSpread)
						{
							this.spread[i] = true;
						}
					}
					else if (distance < fleeRadius)
					{
						this.flee[i] = true;
					}
				}
			}
		}
	}
}
