using Unity.Burst;
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
		public NativeArray<float> diseaseRadiusSq;
		[ReadOnly]
		public float fleeRadius;
		[ReadOnly]
		public NativeArray<float> spreadChance;
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
				/*if (math.lengthsq(this.citizenPositions[i]) < 1)
				{
					continue;
				}*/

				for (int j = 0; j < this.diseasePositions.Length; ++j)
				{
					float distance = math.distancesq(this.diseasePositions[j], this.citizenPositions[i]);
					/*if (float.IsNaN(distance) || float.IsInfinity(distance))
					{
						continue;
					}*/

					if (distance < diseaseRadiusSq[j])
					{
						float norm = ((diseaseRadiusSq[j] - distance) / diseaseRadiusSq[j]) * this.spreadChance[j];
						float r = UnityEngine.Random.Range(0.001f, 100f);

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
