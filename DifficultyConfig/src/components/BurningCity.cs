using Colossal.Serialization.Entities;
using Unity.Entities;

namespace DifficultyConfig
{
	public struct BurningCity : IComponentData, IQueryTypeParameter, ISerializable
	{
		public int maxBuildingCount;

		public BurningCity(int maxBuildingCount)
		{
			this.maxBuildingCount = maxBuildingCount;
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out this.maxBuildingCount);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(this.maxBuildingCount);
		}
	}
}
