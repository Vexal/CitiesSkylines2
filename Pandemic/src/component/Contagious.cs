using Colossal.Serialization.Entities;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace Pandemic
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Contagious : IComponentData, IQueryTypeParameter, IEmptySerializable
	{

	}
}
