﻿using Unity.Mathematics;
using UnityEngine;

namespace EmploymentTracker
{
	public struct RouteOptions
	{
		public float vehicleLineWidth;
		public float pedestrianLineWidth;
		public UnityEngine.Color vehicleLineColor;
		public UnityEngine.Color pedestrianLineColor;
		public UnityEngine.Color subwayLineColor;
		public float minRouteAlpha;
		public float routeWeightMultiplier;
		public float2 routeRoundness;
		//highlight types
		public bool incomingRoutes;
		public bool incomingRoutesTransit;
		public bool highlightSelected;
		public bool transitPassengerRoutes;

		public RouteOptions(float vehicleLineWidth,
			float pedestrianLineWidth,
			Color vehicleLineColor,
			Color pedestrianLineColor,
			Color subwayLineColor,
			float minRouteAlpha,
			float routeWeightMultiplier,
			bool incomingRoutes,
			bool incomingRoutesTransit,
			bool highlightSelected,
			bool transitPassengerRoutes)
		{
			this.vehicleLineWidth = vehicleLineWidth;
			this.pedestrianLineWidth = pedestrianLineWidth;
			this.vehicleLineColor = vehicleLineColor;
			this.pedestrianLineColor = pedestrianLineColor;
			this.subwayLineColor = subwayLineColor;
			this.minRouteAlpha = minRouteAlpha;
			this.routeWeightMultiplier = routeWeightMultiplier;
			this.routeRoundness = new float2() { x = 1, y = 1 };
			this.incomingRoutes = incomingRoutes;
			this.incomingRoutesTransit = incomingRoutesTransit; ;
			this.highlightSelected = highlightSelected;
			this.transitPassengerRoutes = transitPassengerRoutes;
		}

		public RouteOptions(EmploymentTrackerSettings settings)
		{
			this.vehicleLineWidth = settings.vehicleRouteWidth;
			this.pedestrianLineWidth = settings.pedestrianRouteWidth;
			this.vehicleLineColor = new Color(.2f, 1f, .2f);
			this.pedestrianLineColor = new Color(.2f, .5f, 1f);
			this.subwayLineColor = new Color(1f, .5f, 1f);
			this.minRouteAlpha = settings.routeOpacity;
			this.routeWeightMultiplier = settings.routeOpacityMultilier;
			this.routeRoundness = new float2() { x = 1, y = 1 };
			this.transitPassengerRoutes = settings.highlightSelectedTransitVehiclePassengerRoutes;
			this.highlightSelected = settings.highlightSelected;
			this.incomingRoutes = settings.incomingRoutes;
			this.incomingRoutesTransit = settings.incomingRoutesTransit;
		}

		public static float getCurveWidth(byte type)
		{
			switch (type)
			{
				case 1:
					return 1f;
				case 2:
					return .85f;
				case 3:
					return 1f;
				default:
					return 1f;
			}
		}
	}
}
