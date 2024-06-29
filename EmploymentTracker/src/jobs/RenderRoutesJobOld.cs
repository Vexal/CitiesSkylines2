using Game.Rendering;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace EmploymentTracker
{
	//[BurstCompile]
	public struct RouteRenderJobOld : IJob
	{
		public OverlayRenderSystem.Buffer overlayBuffer;
		[ReadOnly]
		public NativeArray<CurveDef> curveDefs;
		[ReadOnly]
		public NativeArray<int> curveCounts;
		[ReadOnly]
		public RouteOptions routeHighlightOptions;
		[ReadOnly]
		public int maxVehicleCount;
		[ReadOnly]
		public int maxPedestrianCount;
		[ReadOnly]
		public int maxTransitCount;
		
		private float minColorWeight;
		private float maxVehichleWeight;
		private float maxPedestrianWeight;
		private float maxTransitWeight;

		public void Execute()
		{
			this.minColorWeight = MathUtil.expFunc(1, this.routeHighlightOptions.routeWeightMultiplier);
			this.maxVehichleWeight = MathUtil.expFunc(this.maxVehicleCount + 1, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight;
			this.maxTransitWeight = MathUtil.expFunc(this.maxTransitCount + 1, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight;
			this.maxPedestrianWeight = MathUtil.expFunc(this.maxPedestrianCount + 1, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight;

			for (int i = 0; i < this.curveDefs.Length; ++i)
			{
				CurveDef curve = this.curveDefs[i];
				overlayBuffer.DrawCurve(this.getCurveColor(curve.type, this.curveCounts[i]), curve.curve, this.routeHighlightOptions.getCurveWidth(curve.type), this.routeHighlightOptions.routeRoundness);
			}
		}

		public UnityEngine.Color getCurveColor(byte type, float weight)
		{
			UnityEngine.Color color;
			float opacityAdd = 0;
			switch (type)
			{
				case 1:
					color = this.routeHighlightOptions.vehicleLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					break;
				case 2:
					color = this.routeHighlightOptions.pedestrianLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxPedestrianWeight;
					break;
				case 3:
					color = this.routeHighlightOptions.subwayLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxTransitWeight;
					break;
				default:
					color = this.routeHighlightOptions.vehicleLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					break;
			}

			color.a = this.routeHighlightOptions.minRouteAlpha + opacityAdd;
			return color;
		}
	}
}
