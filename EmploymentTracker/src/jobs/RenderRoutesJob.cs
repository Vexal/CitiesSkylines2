using Colossal.Mathematics;
using Game.Rendering;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
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
		public NativeList<Bezier4x3> selectedCurves;
		[ReadOnly]
		public int maxVehicleCount;
		[ReadOnly]
		public int maxPedestrianCount;
		[ReadOnly]
		public int maxTransitCount;
		[ReadOnly]
		public int maxGenericVehicleCount;
		[ReadOnly]
		public Bezier4x3 hoverCurve;
		[ReadOnly]
		public bool isHovering;
		
		private float minColorWeight;
		private float maxVehichleWeight;
		private float maxPedestrianWeight;
		private float maxTransitWeight;

		public void Execute()
		{
			this.yellowInverse = UnityEngine.Color.white - UnityEngine.Color.yellow;
			this.yellowIncrement = this.maxGenericVehicleCount > 0 ? this.yellowInverse / this.maxGenericVehicleCount : default;
			if (this.selectedCurves.IsCreated)
			{	
				for (int i = 0; i < this.selectedCurves.Length; ++i)
				{
					this.overlayBuffer.DrawCurve(new UnityEngine.Color(.7f, .7f, 1f, .3f), this.selectedCurves[i], 5f, new float2 { x = 1, y = 1 });
				}
			}
			if (this.isHovering)
			{
				this.overlayBuffer.DrawCurve(new UnityEngine.Color(.7f, .7f, 1f, .6f), this.hoverCurve, 5f, new float2 { x = 1, y = 1 });
			}

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
		private UnityEngine.Color yellowInverse;
		private UnityEngine.Color yellowIncrement;

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
					float mult2 = .05f;
					if (this.maxGenericVehicleCount > 20)
					{
						mult2 = .015f;
					}
					else if (this.maxGenericVehicleCount > 10)
					{
						mult2 = .03f;
					}
					color = UnityEngine.Color.yellow;
					//color.g = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					//opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					if (weight > 1)
					{
						/*color.g += (weight - 1) * mult2;
						color.b += (weight - 1) * mult2;

						color.g = Math.Min(color.g, maxColor);
						color.b = Math.Min(color.b, maxColor);*/
						color += this.yellowIncrement * (weight - 1);
					}
					break;
				default:
					color = this.routeHighlightOptions.vehicleLineColor;
					opacityAdd = (1f - this.routeHighlightOptions.minRouteAlpha) * (MathUtil.expFunc(weight, this.routeHighlightOptions.routeWeightMultiplier) - this.minColorWeight) / this.maxVehichleWeight;
					break;
			}

			color.a = .8f;// this.routeHighlightOptions.minRouteAlpha + opacityAdd;
			return color;
		}
	}
}
