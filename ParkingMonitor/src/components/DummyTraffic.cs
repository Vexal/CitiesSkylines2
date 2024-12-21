using System.Runtime.InteropServices;
using Unity.Entities;

namespace ParkingMonitor
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct DummyTraffic : IComponentData, IQueryTypeParameter
	{
	}
}
