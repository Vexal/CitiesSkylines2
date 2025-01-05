using Colossal.UI.Binding;
using Unity.Entities;

namespace Pandemic
{
	public struct Disease : IComponentData, IQueryTypeParameter, IJsonWritable
	{
		public long id;
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
		public Entity entity;

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);
			writer.PropertyName(nameof(this.id));
			writer.Write(id);
			writer.PropertyName(nameof(this.type));
			writer.Write(this.type);
			writer.PropertyName(nameof(this.baseSpreadRadius));
			writer.Write(baseSpreadRadius);
			writer.PropertyName(nameof(this.baseDeathChance));
			writer.Write(this.baseDeathChance);
			writer.PropertyName(nameof(this.baseHealthPenalty));
			writer.Write(this.baseHealthPenalty);
			writer.PropertyName(nameof(this.ts));
			writer.Write(this.ts.ToString());
			writer.PropertyName("uniqueKey");
			writer.Write(this.getUniqueKey());
			writer.PropertyName(nameof(this.createYear));
			writer.Write(this.createYear);
			writer.PropertyName(nameof(this.createMonth));
			writer.Write(this.createMonth);
			writer.PropertyName(nameof(this.createHour));
			writer.Write(this.createHour);
			writer.PropertyName(nameof(this.createMinute));
			writer.Write(this.createMinute);
			writer.PropertyName(nameof(this.infectionCount));
			writer.Write(infectionCount);
			writer.PropertyName(nameof(this.victimCount));
			writer.Write(victimCount);
			writer.TypeEnd();
		}

		public string getUniqueKey()
		{
			return this.entity.Index.ToString() + ":" + this.entity.Version.ToString();
		}
	}
}
