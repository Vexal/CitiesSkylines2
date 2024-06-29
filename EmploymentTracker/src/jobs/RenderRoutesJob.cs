﻿using Game.Rendering;
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
		//public OverlayRenderSystem.Buffer overlayBuffer;
		public SimpleOverlayRendererSystem.Buffer overlayBuffer;
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

		private const float maxColor = .8f;
		private const float colorMultiplier = .08f;

		public UnityEngine.Color getCurveColor(byte type, float weight)
		{
			UnityEngine.Color color;
			float opacityAdd = 0;
			
			switch (type)
			{
				case 4:
					color = this.routeHighlightOptions.vehicleLineColor;
					//color.g = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					//opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					if (weight > 1)
					{
						color.r += weight * colorMultiplier;
						color.b += weight * colorMultiplier;

						color.r = Math.Min(color.r, maxColor);
						color.b = Math.Min(color.b, maxColor);
					}
					break;
				case 2:
					color = this.routeHighlightOptions.pedestrianLineColor;
					//opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxPedestrianWeight;
					if (weight > 1)
					{
						color.r += weight * colorMultiplier;
						color.g += weight * colorMultiplier;

						color.r = Math.Min(color.r, maxColor);
						color.g = Math.Min(color.g, maxColor);
					}
					break;
				case 3:
					color = this.routeHighlightOptions.subwayLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxTransitWeight;
					break;
				case 1:
					color = new UnityEngine.Color(1f, 0f, 0f);
					//color.g = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					//opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					if (weight > 1)
					{
						color.g += weight * .05f;
						color.b += weight * .05f;

						color.g = Math.Min(color.r, maxColor);
						color.b = Math.Min(color.b, maxColor);
					}
					break;
				default:
					color = this.routeHighlightOptions.vehicleLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					break;
			}

			color.a = 1;// this.routeHighlightOptions.minRouteAlpha + opacityAdd;
			return color;
		}
	}
}
