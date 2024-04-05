using Colossal.Serialization.Entities;
using Unity.Entities;

namespace EmploymentTracker
{
	public struct DeleteTimer : IComponentData, IQueryTypeParameter, ISerializable
	{
		public long currentFrame;
		public long endFrame;
		public ComponentTypeSelector componentType;

		public DeleteTimer(long startFrame, int frameCount, ComponentTypeSelector componentType)
		{
			this.currentFrame = startFrame;
			this.endFrame = this.currentFrame + frameCount;
			this.componentType = componentType;
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
	public enum ComponentTypeSelector
	{
		HIGHLIGHT
	}
}

