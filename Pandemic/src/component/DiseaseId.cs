using Unity.Entities;

namespace Pandemic
{
	public struct DiseaseId : IComponentData, IQueryTypeParameter
	{
		public uint diseaseId;
	}
}
