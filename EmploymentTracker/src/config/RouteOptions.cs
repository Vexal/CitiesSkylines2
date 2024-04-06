using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace EmploymentTracker
{
	internal struct RouteOptions
	{
		public float vehicleLineWidth;
		public float pedestrianLineWidth;
		public UnityEngine.Color vehicleLineColor;
		public UnityEngine.Color pedestrianLineColor;
		public UnityEngine.Color subwayLineColor;
		public float2 routeRoundness;

		public RouteOptions(float vehicleLineWidth, float pedestrianLineWidth, Color vehicleLineColor, Color pedestrianLineColor, Color subwayLineColor)
		{
			this.vehicleLineWidth = vehicleLineWidth;
			this.pedestrianLineWidth = pedestrianLineWidth;
			this.vehicleLineColor = vehicleLineColor;
			this.pedestrianLineColor = pedestrianLineColor;
			this.subwayLineColor = subwayLineColor;
			this.routeRoundness = new float2() { x = 1, y = 1 };
		}

		public RouteOptions(EmploymentTrackerSettings settings)
		{
			this.vehicleLineWidth = settings.vehicleRouteWidth;
			this.pedestrianLineWidth = settings.pedestrianRouteWidth;
			this.vehicleLineColor = new Color(.2f, 1f, .2f);
			this.pedestrianLineColor = new Color(.2f, .5f, 1f);
			this.subwayLineColor = new Color(1f, .5f, 1f);
			this.routeRoundness = new float2() { x = 1, y = 1 };
		}
	}
}
