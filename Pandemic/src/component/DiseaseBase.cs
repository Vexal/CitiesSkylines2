using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
    public struct DiseaseBase : IComponentData, IQueryTypeParameter, ISerializable, IJsonWritable
	{
		public static readonly int CURRENT_VERSION = 1;

		public Entity entity;
		public float baseSpreadRadius;
        public float baseSpreadChance;
        public float baseDeathChance;
        public byte baseHealthPenalty;
        public byte maxDeathHealth;
        public float mutationChance;
        public float mutationMagnitude;
        public float progressionSpeed;
        public float baseSpontaneousChance;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out int version);
			reader.Read(out entity);
			reader.Read(out baseSpreadRadius);
			reader.Read(out baseSpreadChance);
			reader.Read(out baseDeathChance);
			reader.Read(out baseHealthPenalty);
			reader.Read(out maxDeathHealth);
			reader.Read(out mutationChance);
			reader.Read(out mutationMagnitude);
			reader.Read(out progressionSpeed);
			reader.Read(out baseSpontaneousChance);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(CURRENT_VERSION);
			writer.Write(entity);
			writer.Write(baseSpreadRadius);
			writer.Write(baseSpreadChance);
			writer.Write(baseDeathChance);
			writer.Write(baseHealthPenalty);
			writer.Write(maxDeathHealth);
			writer.Write(mutationChance);
			writer.Write(mutationMagnitude);
			writer.Write(progressionSpeed);
			writer.Write(baseSpontaneousChance);
		}

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);
			writer.PropertyName(nameof(this.entity));
			writer.Write(this.entity.keyString());
			writer.PropertyName(nameof(this.baseSpreadRadius));
			writer.Write(this.baseSpreadRadius);
			writer.PropertyName(nameof(this.baseSpreadChance));
			writer.Write(this.baseSpreadChance);
			writer.PropertyName(nameof(this.baseDeathChance));
			writer.Write(this.baseDeathChance);
			writer.PropertyName(nameof(this.baseHealthPenalty));
			writer.Write(this.baseHealthPenalty);
			writer.PropertyName(nameof(this.maxDeathHealth));
			writer.Write(this.maxDeathHealth);
			writer.PropertyName(nameof(this.mutationChance));
			writer.Write(this.mutationChance);
			writer.PropertyName(nameof(this.mutationMagnitude));
			writer.Write(this.mutationMagnitude);
			writer.PropertyName(nameof(this.progressionSpeed));
			writer.Write(this.progressionSpeed);
			writer.PropertyName(nameof(this.baseSpontaneousChance));
			writer.Write(this.baseSpontaneousChance);
			writer.TypeEnd();
		}
	}
}
