using Unity.Entities;

namespace Pandemic
{
	public struct LastDisease : IComponentData, IQueryTypeParameter
	{
		public Entity lastCold;
		public Entity lastFlu;
		public Entity lastNovel;

		public Entity getLastOfType(uint type)
		{
			switch (type)
			{
				case 1:
					return lastCold;
				case 2:
					return lastFlu;
				case 3:
					return lastNovel;
				default:
					return Entity.Null;
			}
		}
	}
}
