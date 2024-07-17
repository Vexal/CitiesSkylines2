using Unity.Entities;

namespace NoTrafficDespawn
{
	public struct StuckObject : IComponentData, IQueryTypeParameter
	{
		public int frameCount;

		public StuckObject(int frameCount)
		{
			this.frameCount = frameCount;
		}
	}

	public enum StuckType
	{
		None = 0,
		Circular = 1,
		Misc =2
	}
}
