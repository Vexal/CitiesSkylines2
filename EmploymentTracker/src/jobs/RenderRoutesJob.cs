using Game.Rendering;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace EmploymentTracker
{
	//[BurstCompile]
	public struct RouteRenderJob : IJob
	{
		public OverlayRenderSystem.Buffer overlayBuffer;
		[ReadOnly]
		public NativeArray<CurveDef> curveDefs;
		[ReadOnly]
		public NativeArray<int> curveCounts;
		[ReadOnly]
		public RouteOptions routeHighlightOptions;

		public void Execute()
		{
			for (int i = 0; i < this.curveDefs.Length; ++i)
			{
				CurveDef curve = this.curveDefs[i];
				overlayBuffer.DrawCurve(this.getCurveColor(curve.type, this.curveCounts[i]), curve.curve, this.getCurveWidth(curve.type), this.routeHighlightOptions.routeRoundness);
			}
		}

		public float getCurveWidth(byte type)
		{
			switch (type)
			{
				case 1:
					return this.routeHighlightOptions.vehicleLineWidth;
				case 2:
					return this.routeHighlightOptions.pedestrianLineWidth;
				case 3:
					return this.routeHighlightOptions.vehicleLineWidth;
				default:
					return 1f;
			}
		}

		public UnityEngine.Color getCurveColor(byte type, float weight)
		{
			UnityEngine.Color color;
			switch (type)
			{
				case 1:
					color = this.routeHighlightOptions.vehicleLineColor;
					break;
				case 2:
					color = this.routeHighlightOptions.pedestrianLineColor;
					break;
				case 3:
					color = this.routeHighlightOptions.subwayLineColor;
					break;
				default:
					color = this.routeHighlightOptions.vehicleLineColor;
					break;
			}

			color.a = math.min(this.routeHighlightOptions.minRouteAlpha + (weight * this.routeHighlightOptions.routeWeightMultiplier), 1);
			return color;
		}
	}
}
