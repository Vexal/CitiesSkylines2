using Colossal.UI.Binding;
using System;
using Unity.Entities;

namespace Pandemic
{
	public struct Disease : IComponentData, IQueryTypeParameter, IJsonWritable
	{
		public int id;
		public uint type;
		public float baseSpreadRadius;
		public float baseSpreadChance;
		public float baseDeathChance;
		public byte baseHealthPenalty;
		public byte maxDeathHealth;
		public long ts;
		public float progressionSpeed;
		public int createYear;
		public int createMonth;
		public int createDay;
		public int createHour;
		public int createMinute;
		public int createSecond;
		public int createWeek;
		public uint infectionCount;
		public uint victimCount;
		public float mutationChance;
		public float mutationMagnitude;
		public Entity entity;
		public Entity parent;

		public string getUniqueKey()
		{
			return this.entity.keyString();
		}

		public Disease mutate(bool noParent = false)
		{
			float m = this.mutationMagnitude;
			Disease mutation = new Disease() {
				parent = !noParent ? this.entity : Entity.Null,
				type = this.type,
				baseSpreadRadius = Utils.mutated(this.baseSpreadRadius, m),
				baseSpreadChance = Utils.mutated(this.baseSpreadChance, m),
				baseHealthPenalty = Utils.mutated(this.baseHealthPenalty, m),
				baseDeathChance = Utils.mutated(this.baseDeathChance, m),
				maxDeathHealth = Utils.mutated(this.maxDeathHealth, m),
				mutationChance = Utils.mutated(this.mutationChance, m),
				mutationMagnitude = Utils.mutated(this.mutationMagnitude, m),
				progressionSpeed = Utils.mutated(this.progressionSpeed, m),
			};

			return mutation;
		}

		public bool shouldMutate()
		{
			if (this.mutationChance > 0)
			{
				return UnityEngine.Random.Range(0f, 100f) < this.mutationChance;
			}
			else
			{
				return false;
			}
		}

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);
			writer.PropertyName(nameof(this.id));
			writer.Write(id.ToString());
			writer.PropertyName(nameof(this.type));
			writer.Write(this.type);
			writer.PropertyName(nameof(this.baseSpreadRadius));
			writer.Write(baseSpreadRadius);
			writer.PropertyName(nameof(this.baseDeathChance));
			writer.Write(this.baseDeathChance);
			writer.PropertyName(nameof(this.baseHealthPenalty));
			writer.Write(this.baseHealthPenalty);
			writer.PropertyName(nameof(this.baseSpreadChance));
			writer.Write(this.baseSpreadChance);
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
			writer.PropertyName(nameof(this.mutationChance));
			writer.Write(this.mutationChance);
			writer.PropertyName(nameof(this.progressionSpeed));
			writer.Write(this.progressionSpeed);
			writer.PropertyName(nameof(this.mutationMagnitude));
			writer.Write(this.mutationMagnitude);
			writer.PropertyName(nameof(this.createWeek));
			writer.Write(this.createWeek);
			writer.PropertyName(nameof(this.createSecond));
			writer.Write(this.createSecond);
			writer.PropertyName(nameof(this.parent));
			writer.Write(this.parent.keyString());
			writer.TypeEnd();
		}

		public string getStrainAbbr()
		{
			switch (this.type)
			{
				case 1:
					return "CC";
				case 2:
					return "FL";
				case 3:
					return "EX";
				default:
					return "UNK";
			}
		}

		public string getStrainName()
		{
			return this.getStrainAbbr() + this.createYear.ToString() + "." + (this.createMonth.ToString()) + "." + (this.createHour.ToString()) + "." + (this.createMinute.ToString());
		}

		public string getDiseaseTypeName()
		{
			switch (this.type)
			{
				case 1:
					return "Common Cold";
				case 2:
					return "Influenza";
				case 3:
					return "Novel Virus";
				default:
					return "Unknown";
			}
		}


		private static DateTime EPOCH = new DateTime(1970, 1, 1);

		public void initMetadata(DateTime date, Entity diseaseEntity)
		{
			long unixTs = (long)date.Subtract(EPOCH).TotalMilliseconds;

			this.id = UnityEngine.Random.Range(1, int.MaxValue);
			this.ts = unixTs;
			this.entity = diseaseEntity;
			this.createYear = date.Year;
			this.createMonth = date.Month;
			this.createHour = date.Hour;
			this.createMinute = date.Minute;
			this.createDay = date.Day;
		}
	}
}
