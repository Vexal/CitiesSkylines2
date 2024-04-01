using Colossal.Serialization.Entities;
using Unity.Entities;

namespace DifficultyConfig
{
	public struct CollapsingCity : IComponentData, IQueryTypeParameter, ISerializable
	{
		public int frameInterval;

		public CollapsingCity(int frameInterval)
		{
			this.frameInterval = frameInterval;
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out this.frameInterval);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(this.frameInterval);
		}
	}
}
