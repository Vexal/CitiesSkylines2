using Colossal.Serialization.Entities;
using Unity.Entities;

namespace DifficultyConfig
{
	public struct EmployeeStatus : IComponentData, IQueryTypeParameter, ISerializable
	{
		public int maxWorkers;
		public int currentWorkers;
		public int commutingWorkers;
		public int onPremisesWorkers;

		public EmployeeStatus(int maxWorkers, int currentWorkers, int commutingWorkers, int onPremisesWorkers)
		{
			this.maxWorkers = maxWorkers;
			this.currentWorkers = currentWorkers;
			this.commutingWorkers = commutingWorkers;
			this.onPremisesWorkers = onPremisesWorkers;
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out this.maxWorkers);
			reader.Read(out this.currentWorkers);
			reader.Read(out this.maxWorkers);
			reader.Read(out this.commutingWorkers);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(this.maxWorkers);
			writer.Write(this.currentWorkers);
			writer.Write(this.commutingWorkers);
			writer.Write(this.onPremisesWorkers);
		}
	}
}
