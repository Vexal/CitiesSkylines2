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
		public float radius;
		[ReadOnly]
		public int spreadChance;
		[ReadOnly]
		public NativeArray<float3> citizenPositions;

		[NativeDisableParallelForRestriction]
		public NativeArray<bool> results;

		public void Execute(int start, int count)
		{
			Random random = new Random();
			for (int i = start; i < start + count; ++i)
			{
				for (int j = 0; j < this.diseasePositions.Length; ++j)
				{
					float distance = math.distance(this.diseasePositions[j], this.citizenPositions[i]);
					if (distance < radius)
					{
						int norm = (int)(((radius - distance) / radius) * this.spreadChance);
						bool shouldSpread = random.NextFloat(10000) < norm;
						if (shouldSpread)
						{
							this.results[i] = true;
						}
					}
				}
			}
		}
	}
}
