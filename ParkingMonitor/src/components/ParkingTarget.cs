using Colossal.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ParkingMonitor
{
	public struct ParkingTarget : IBufferElementData, IQueryTypeParameter
	{
		public Entity currentTarget;
		public Entity currentDestination;

		public ParkingTarget(Entity currentTarget, Entity currentDestination)
		{
			this.currentTarget = currentTarget;
			this.currentDestination = currentDestination;
		}
	}
}
