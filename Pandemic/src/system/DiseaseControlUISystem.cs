using Colossal.UI.Binding;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pandemic
{
	internal partial class DiseaseControlUISystem : UISystemBase
	{
		private TriggerBinding<string> createDiseaseTrigger;
		private DiseaseProgressionSystem diseaseProgressionSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.diseaseProgressionSystem = World.GetOrCreateSystemManaged<DiseaseProgressionSystem>();

			this.createDiseaseTrigger = new TriggerBinding<string>("Pandemic", "createCustomDisease", (
				string json) =>
			{
				DiseaseCreateInput inp = JsonSerializer.Deserialize<DiseaseCreateInput>(json);

				this.diseaseProgressionSystem.createCustomDisease(inp);
			});

			AddBinding(this.createDiseaseTrigger);
		}
	}
}
