using Colossal.UI.Binding;
using System;
using System.Runtime.Remoting.Lifetime;
using Unity.Entities;
using Unity.Mathematics;

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

		public Disease mutate()
		{
			float m = this.mutationMagnitude;
			Disease mutation = new Disease() {
				parent = this.entity,
				type = this.type,
				baseSpreadRadius = mutated(this.baseSpreadRadius, m),
				baseSpreadChance = mutated(this.baseSpreadChance, m),
				baseHealthPenalty = mutated(this.baseHealthPenalty, m),
				maxDeathHealth = mutated(this.maxDeathHealth, m),
				mutationChance = mutated(this.mutationChance, m),
				mutationMagnitude = mutated(this.mutationMagnitude, m),
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

		public static float mutated(float original, float maxMagnitude)
		{
			float h = maxMagnitude * .5f;
			float amp = UnityEngine.Random.Range(1 - math.max(h, .01f), 1 + h);
			return original * amp;
		}

		public static byte mutated(byte original, float maxMagnitude)
		{
			float h = maxMagnitude * .5f;
			float amp = UnityEngine.Random.Range(1 - math.max(h, .01f), 1 + h);
			return (byte)(original * amp);
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
	}
}
