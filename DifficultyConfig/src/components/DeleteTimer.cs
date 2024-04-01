using Colossal.Serialization.Entities;
using Unity.Entities;

namespace DifficultyConfig
{
	public struct DeleterTimer : IComponentData, IQueryTypeParameter, ISerializable
	{
		public long currentFrame;
		public long endFrame;

		public DeleterTimer(long startFrame, int frameCount)
		{
			this.currentFrame = startFrame;
			this.endFrame = this.currentFrame + frameCount;
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out this.currentFrame);
			reader.Read(out this.endFrame);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(this.currentFrame);
			writer.Write(this.endFrame);
		}
	}
}
