using Unity.Entities;

namespace Pandemic
{
	public struct LastDisease : IComponentData, IQueryTypeParameter
	{
		public uint diseaseId;
	}
}
