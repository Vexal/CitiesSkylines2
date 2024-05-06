using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace EmploymentTracker
{
	[BurstCompile]
	public class MathUtil
	{
		[BurstCompile]
		public static float expFunc(float weight, float multiplier)
		{
			return -math.pow(multiplier, weight);
		}

		[BurstCompile]
		public static NativeHashMap<CurveDef, int> mergeResultCurves(ref NativeArray<NativeHashMap<CurveDef, int>> results, out int maxVehicleWeight, out int maxPedestrianWeight, out int maxTransitWeight)
		{
			NativeHashMap<CurveDef, int> resultCurves = new NativeHashMap<CurveDef, int>(20000, Allocator.Temp);

			maxVehicleWeight = 1;
			maxPedestrianWeight = 1;
			maxTransitWeight = 1;

			for (int i = 0; i < results.Length; ++i)
			{
				NativeHashMap<CurveDef, int> batchResult = results[i];

				foreach (var r in batchResult)
				{
					CurveDef resultCurve = r.Key;
					int weight = r.Value;

					if (resultCurves.ContainsKey(resultCurve))
					{
						int newWeight = resultCurves[resultCurve] += weight;
						if (resultCurve.type == 2)
						{
							maxPedestrianWeight = Math.Max(newWeight, maxPedestrianWeight);
						}
						else if (resultCurve.type == 3)
						{
							maxTransitWeight = Math.Max(newWeight, maxTransitWeight);
						}
						else
						{
							maxVehicleWeight = Math.Max(newWeight, maxVehicleWeight);
						}

					}
					else
					{
						resultCurves[resultCurve] = weight;
					}
				}
			}

			return resultCurves;
		}
	}
}
