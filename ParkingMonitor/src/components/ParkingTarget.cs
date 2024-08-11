using Colossal.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ParkingMonitor
{
	public struct ParkingTarget : IComponentData, IQueryTypeParameter, ISerializable
	{
		public Entity currentTarget;
		public int attemptCount;
		public Entity currentDestination;

		public ParkingTarget(Entity currentTarget, int attemptCount, Entity currentDestination)
		{
			this.currentTarget = currentTarget;
			this.attemptCount = attemptCount;
			this.currentDestination = currentDestination;
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out this.currentTarget);
			reader.Read(out this.attemptCount);
			//this.currentDestination = Entity.Null;
			reader.Read(out this.currentDestination);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(this.currentTarget);
			writer.Write(this.attemptCount);
			writer.Write(this.currentDestination);
		}
	}
}
