using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Pandemic
{
	[BurstCompile]
	public static class Utils
	{
		public static string keyString(this Entity entity)
		{
			return entity.Index.ToString() + ":" + entity.Version.ToString();
		}

		[BurstCompile]
		public static float mutated(float original, float maxMagnitude)
		{
			float h = maxMagnitude * .5f;
			float amp = UnityEngine.Random.Range(1 - math.max(h, .01f), 1 + h);
			return original * amp;
		}

		[BurstCompile]
		public static byte mutated(byte original, float maxMagnitude)
		{
			float h = maxMagnitude * .5f;
			float amp = UnityEngine.Random.Range(1 - math.max(h, .01f), 1 + h);
			return (byte)(original * amp);
		}
	}
}
