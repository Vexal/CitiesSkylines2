using Colossal.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ParkingMonitor
{
	public struct FailedParkingAttempt : IBufferElementData, IQueryTypeParameter, ISerializable
	{
		public long time;

		public FailedParkingAttempt(long time)
		{
			this.time = time;
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out this.time);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(this.time);
		}
	}
}
