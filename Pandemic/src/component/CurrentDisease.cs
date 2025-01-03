using Unity.Entities;

namespace Pandemic
{
	public struct CurrentDisease : IComponentData, IQueryTypeParameter
	{
		public uint diseaseId;
		public byte progression;
	}
}
