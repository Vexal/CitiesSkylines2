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
			this.minColorWeight = expFunc(1, this.routeHighlightOptions.routeWeightMultiplier);
			this.maxVehichleWeight = expFunc(this.maxVehicleCount + 1, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight;
			this.maxTransitWeight = expFunc(this.maxTransitCount + 1, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight;
			this.maxPedestrianWeight = expFunc(this.maxPedestrianCount + 1, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight;

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
			float opacityAdd = 0;
			switch (type)
			{
				case 1:
					color = this.routeHighlightOptions.vehicleLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					break;
				case 2:
					color = this.routeHighlightOptions.pedestrianLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxPedestrianWeight;
					break;
				case 3:
					color = this.routeHighlightOptions.subwayLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxTransitWeight;
					break;
				default:
					color = this.routeHighlightOptions.vehicleLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					break;
			}

			color.a = this.routeHighlightOptions.minRouteAlpha + opacityAdd;
			return color;
		}

		[BurstCompile]
		public static float expFunc(float weight, float multiplier)
		{
			return -math.pow(.7f, weight);
		}
	}
}
