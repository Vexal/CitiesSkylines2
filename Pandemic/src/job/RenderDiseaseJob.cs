using Colossal;
using Game.Rendering;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pandemic
{
	//[BurstCompile]
	public struct RenderDiseaseJob : IJob
	{
		public OverlayRenderSystem.Buffer overlayBuffer;
		[ReadOnly]
		public NativeArray<float3> positions;
		[ReadOnly]
		public NativeArray<float> radius;
		[ReadOnly]
		public NativeCounter count;

		public void Execute()
		{
			UnityEngine.Color color = new UnityEngine.Color(.15f, .72f, .24f, .28f);
			int c = this.count.Count;
			for (int i = 0; i < c; ++i)
			{
				overlayBuffer.DrawCircle(color, this.positions[i], math.sqrt(this.radius[i]) * 2);
			}

			this.count.Dispose();
		}
	}
}
