using Colossal.Serialization.Entities;
using Unity.Entities;

namespace Pandemic
{
	public struct DiseaseRef : IComponentData, IQueryTypeParameter, ISerializable
	{
		public static readonly int CURRENT_VERSION = 1;
		public Entity disease;
		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out int version);
			reader.Read(out disease);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(CURRENT_VERSION);
			writer.Write(disease);
		}
	}
}
