using Unity.Entities;

namespace Pandemic
{
	public struct CurrentDisease : IComponentData, IQueryTypeParameter
	{
		public Entity disease;
		public float progression;
	}
}
