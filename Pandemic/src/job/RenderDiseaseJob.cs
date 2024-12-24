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
		public float radius;

		public void Execute()
		{
			UnityEngine.Color color = new UnityEngine.Color(.15f, .72f, .24f, .28f);
			for (int i = 0; i < this.positions.Length; ++i)
			{
				overlayBuffer.DrawCircle(color, this.positions[i], this.radius);
			}
		}
	}
}
