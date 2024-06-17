using Colossal.Mathematics;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

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


		[BurstCompile]
		public static float Length(ref Bezier4x2 curve)
		{
			float num = 0f;
			float2 x = curve.a;
			for (int i = 1; i <= 16; i++)
			{
				float t = (float)i / 16f;
				//float2 @float = Position(curve, (float)i / 16f);

				float3 f = new float3(t, 1f - t, 3f * t);
				float4 float2 = f.yzzx * f.yyxx * f.yyyx;
				float4 float3 = curve.ab * float2.xxyy + curve.cd * float2.zzww;
				float2 @float = float3.xy + float3.zw;

				num += math.distance(x, @float);
				x = @float;
			}

			return num;
		}

		[BurstCompile]
		public static void Position(ref Bezier4x2 curve, float t, out float2 result)
		{
			float3 @float = new float3(t, 1f - t, 3f * t);
			float4 float2 = @float.yzzx * @float.yyxx * @float.yyyx;
			float4 float3 = curve.ab * float2.xxyy + curve.cd * float2.zzww;
			result = float3.xy + float3.zw;
		}

		[BurstCompile]
		public static void BuildCurveMatrix(ref Bezier4x3 curve, float length, out Matrix4x4 results)
		{
			float2 @float = default(float2);
			@float.x = math.distance(curve.a, curve.b);
			@float.y = math.distance(curve.c, curve.d);
			@float /= length;
			float4x4 result = default(float4x4);
			result.c0 = new float4(curve.a, 0f);
			result.c1 = new float4(curve.b, @float.x);
			result.c2 = new float4(curve.c, 1f - @float.y);
			result.c3 = new float4(curve.d, 1f);

			results = result;
		}

		[BurstCompile]
		public static void FitQuad(ref Bezier4x3 curve, float extend, out Bounds3 bounds, out Matrix4x4 resultMatrix)
		{
			bounds = MathUtils.Bounds(curve);
			bounds.min.xz -= extend;
			bounds.max.xz += extend;
			float3 @float = MathUtils.Center(bounds);
			quaternion quaternion = quaternion.identity;
			float3 float2 = 0f;
			float2.xz = MathUtils.Extents(bounds.xz);
			float2.y = 1f;
			float3 float3 = curve.d - curve.a;
			float num = math.length(float3);
			if (num > 0.1f)
			{
				float3 /= num;
				float3 float4 = math.cross(float3, curve.b - curve.a);
				float3 float5 = math.cross(float3, curve.d - curve.c);
				float4 = math.select(float4, -float4, float4.y < 0f);
				float5 = math.select(float5, -float5, float5.y < 0f);
				float3 float6 = float4 + float5;
				float num2 = math.length(float6);
				float6 = math.lerp(new float3(0f, 1f, 0f), float6, math.saturate(num2 / num * 10f));
				float6 = math.normalize(float6);
				float3 value = math.cross(float6, float3);
				if (MathUtils.TryNormalize(ref value))
				{
					float3 x = curve.b - curve.a;
					float3 x2 = curve.c - curve.a;
					float3 x3 = curve.d - curve.a;
					float2 y = new float2(math.dot(x, value), math.dot(x, float3));
					float2 x4 = new float2(math.dot(x2, value), math.dot(x2, float3));
					float2 y2 = new float2(math.dot(x3, value), math.dot(x3, float3));
					float2 float7 = math.min(math.min(0f, y), math.min(x4, y2));
					float2 float8 = math.max(math.max(0f, y), math.max(x4, y2));
					float2 float9 = math.lerp(float7, float8, 0.5f);
					quaternion = quaternion.LookRotation(float3, float6);
					@float = curve.a + math.rotate(quaternion, new float3(float9.x, 0f, float9.y));
					float2.xz = (float8 - float7) * 0.5f + extend;
				}
			}

			resultMatrix = Matrix4x4.TRS(@float, quaternion, float2);
		}

		[BurstCompile]
		public static void FitQuad(ref float2 direction, ref float3 position, float extend, out Bounds3 bounds, out Matrix4x4 resultMatrix)
		{
			bounds = new Bounds3(position, position);
			bounds.min.xz -= extend;
			bounds.max.xz += extend;
			quaternion quaternion = quaternion.RotateY(math.atan2(direction.x, direction.y));
			resultMatrix = Matrix4x4.TRS(s: new float3(extend, 1f, extend), pos: position, q: quaternion);
		}

		[BurstCompile]
		public static void ToBounds(ref Bounds3 bounds, out Bounds result)
		{
			result = new Bounds(MathUtils.Center(bounds), MathUtils.Size(bounds));
		}
	}
}
