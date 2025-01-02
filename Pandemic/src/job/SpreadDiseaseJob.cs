﻿using Unity.Burst;
using Unity.Collections;
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
		public NativeArray<float> diseaseRadiuses;
		[ReadOnly]
		public float spreadRadius;
		[ReadOnly]
		public float fleeRadius;
		[ReadOnly]
		public float spreadChance;
		[ReadOnly]
		public NativeArray<float3> citizenPositions;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> spread;
		[NativeDisableParallelForRestriction]
		public NativeArray<bool> flee;

		public void Execute(int start, int count)
		{
			for (int i = start; i < start + count; ++i)
			{
				if (math.lengthsq(this.citizenPositions[i]) < 1)
				{
					continue;
				}

				for (int j = 0; j < this.diseasePositions.Length; ++j)
				{
					float distance = math.distancesq(this.diseasePositions[j], this.citizenPositions[i]);
					if (float.IsNaN(distance) || float.IsInfinity(distance))
					{
						continue;
					}

					if (distance < spreadRadius)
					{
						float norm = ((spreadRadius - distance) / spreadRadius * this.spreadChance);
						float r = UnityEngine.Random.Range(0f, 100f);

						bool shouldSpread = r < norm;
						if (shouldSpread)
						{
							this.spread[i] = j + 1;
						}
					}
					/*else if (distance < fleeRadius)
					{
						this.flee[i] = true;
					}*/
				}
			}
		}
	}
}
