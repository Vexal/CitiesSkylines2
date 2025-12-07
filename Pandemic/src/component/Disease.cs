using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using System;
using Unity.Entities;

namespace Pandemic
{
	public struct Disease : IComponentData, IQueryTypeParameter, IJsonWritable, ISerializable
	{
		public static readonly int CURRENT_VERSION = 1;

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
		public float spontaneousProbability;
		public bool preventSpontaneously;
		public int spreadCount;
		public Entity entity;
		public Entity parent;
        public Entity diseaseBase;

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(CURRENT_VERSION);
			writer.Write(baseSpreadRadius);
			writer.Write(baseSpreadChance);
			writer.Write(baseDeathChance);
			writer.Write(baseHealthPenalty);
			writer.Write(maxDeathHealth);
			writer.Write(ts);
			writer.Write(progressionSpeed);
			writer.Write(createYear);
			writer.Write(createMonth);
			writer.Write(createDay);
			writer.Write(createHour);
			writer.Write(createMinute);
			writer.Write(createSecond);
			writer.Write(createWeek);
			writer.Write(infectionCount);
			writer.Write(victimCount);
			writer.Write(mutationChance);
			writer.Write(mutationMagnitude);
			writer.Write(spontaneousProbability);
			writer.Write(preventSpontaneously);
			writer.Write(spreadCount);
			writer.Write(entity);
			writer.Write(parent);
			writer.Write(diseaseBase);
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out int version);
			reader.Read(out baseSpreadRadius);
			reader.Read(out baseSpreadChance);
			reader.Read(out baseDeathChance);
			reader.Read(out baseHealthPenalty);
			reader.Read(out maxDeathHealth);
			reader.Read(out ts);
			reader.Read(out progressionSpeed);
			reader.Read(out createYear);
			reader.Read(out createMonth);
			reader.Read(out createDay);
			reader.Read(out createHour);
			reader.Read(out createMinute);
			reader.Read(out createSecond);
			reader.Read(out createWeek);
			reader.Read(out infectionCount);
			reader.Read(out victimCount);
			reader.Read(out mutationChance);
			reader.Read(out mutationMagnitude);
			reader.Read(out spontaneousProbability);
			reader.Read(out preventSpontaneously);
			reader.Read(out spreadCount);
			reader.Read(out entity);
			reader.Read(out parent);
			reader.Read(out diseaseBase);
		}

		public string getUniqueKey()
		{
			return this.entity.keyString();
		}

		public Disease mutate(bool noParent = false)
		{
			float m = this.mutationMagnitude;
			Disease mutation = new Disease() {
				parent = !noParent ? this.entity : Entity.Null,
				baseSpreadRadius = Utils.mutated(this.baseSpreadRadius, m),
				baseSpreadChance = Utils.mutated(this.baseSpreadChance, m),
				baseHealthPenalty = Utils.mutated(this.baseHealthPenalty, m),
				baseDeathChance = Utils.mutated(this.baseDeathChance, m),
				maxDeathHealth = Utils.mutated(this.maxDeathHealth, m),
				mutationChance = Utils.mutated(this.mutationChance, m),
				mutationMagnitude = Utils.mutated(this.mutationMagnitude, m),
				progressionSpeed = Utils.mutated(this.progressionSpeed, m),
				spontaneousProbability = Utils.mutated(this.spontaneousProbability, m),
                diseaseBase = this.diseaseBase
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
			writer.PropertyName(nameof(this.spontaneousProbability));
			writer.Write(this.spontaneousProbability);
			writer.PropertyName(nameof(this.preventSpontaneously));
			writer.Write(this.preventSpontaneously);
			writer.PropertyName(nameof(this.spreadCount));
			writer.Write(this.spreadCount);
			writer.PropertyName(nameof(this.parent));
			writer.Write(this.parent.keyString());
			writer.PropertyName(nameof(this.diseaseBase));
			writer.Write(this.diseaseBase.keyString());
			writer.TypeEnd();
		}

		public string getStrainName()
		{
			return this.createYear.ToString() + "." + (this.createMonth.ToString()) + "." + (this.createHour.ToString()) + "." + (this.createMinute.ToString());
		}



		private static DateTime EPOCH = new DateTime(1970, 1, 1);

		public void initMetadata(DateTime date, Entity diseaseEntity)
		{
			long unixTs = (long)date.Subtract(EPOCH).TotalMilliseconds;

			this.ts = unixTs;
			this.entity = diseaseEntity;
			this.createYear = date.Year;
			this.createMonth = date.Month;
			this.createHour = date.Hour;
			this.createMinute = date.Minute;
			this.createDay = date.Day;
		}
	}

	public struct DiseaseCreateInput
	{
		public string name { get; set; }
		public int baseEntityIndex { get; set; }
		public int baseEntityVersion { get; set; }
		public float baseSpreadChance { get; set; }
		public float baseDeathChance { get; set; }
		public byte baseHealthPenalty { get; set; }
		public float baseSpreadRadius { get; set; }
		public float mutationChance { get; set; }
		public float mutationMagnitude { get; set; }
		public float progressionSpeed { get; set; }
		public float spontaneousProbability { get; set; }
		public bool preventSpontaneously { get; set; }
		public int entityIndex { get; set; }
		public int entityVersion { get; set; }
		public int spreadCount { get; set; }

		public Entity getEntity()
		{
			return new Entity { Index = this.entityIndex, Version = this.entityVersion };
		}

		public Entity getBaseEntity()
		{
			return new Entity { Index = this.baseEntityIndex, Version = this.baseEntityVersion };
		}
	}
}
