using Colossal.Serialization.Entities;
using Unity.Entities;

namespace EmploymentTracker
{
	public struct RouteHighlighted : IComponentData, IQueryTypeParameter, ISerializable
	{
		

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{

		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{

		}
	}
}

