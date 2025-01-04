using Unity.Entities;

namespace Pandemic
{
	public struct Disease : IComponentData, IQueryTypeParameter
	{
		public uint id;
		public uint type;
		public float baseSpreadRadius;
		public float baseSpreadChance;
		public float baseDeathChance;
		public byte baseHealthPenalty;
		public byte maxDeathHealth;
		public long ts;
		public int createYear;
		public int createMonth;
		public int createDay;
		public int createHour;
		public int createMinute;
		public uint infectionCount;
		public uint victimCount;
	}
}
