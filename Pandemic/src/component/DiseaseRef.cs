using Unity.Entities;

namespace Pandemic
{
	public struct DiseaseRef : IComponentData, IQueryTypeParameter
	{
		public Entity disease;
	}
}
