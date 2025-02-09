using Colossal.UI.Binding;
using Game.Tools;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
	internal partial class DiseaseControlUISystem : UISystemBase
	{
		private DiseaseProgressionSystem diseaseProgressionSystem;
		private ToolSystem toolSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.diseaseProgressionSystem = World.GetOrCreateSystemManaged<DiseaseProgressionSystem>();
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

			AddBinding(new TriggerBinding<string>("Pandemic", "createCustomDisease", (
				string json) =>
			{
				DiseaseCreateInput inp = JsonSerializer.Deserialize<DiseaseCreateInput>(json);
				this.diseaseProgressionSystem.createCustomDisease(inp);
			}));

			AddBinding(new TriggerBinding<string>("Pandemic", "editDisease", (
				string json) =>
			{
				DiseaseCreateInput inp = JsonSerializer.Deserialize<DiseaseCreateInput>(json);
				this.diseaseProgressionSystem.editDisease(inp);
			}));

			AddBinding(new TriggerBinding<string>("Pandemic", "cureDisease", (
				string json) =>
			{
				DiseaseCreateInput inp = JsonSerializer.Deserialize<DiseaseCreateInput>(json);
				this.diseaseProgressionSystem.cureDisease(inp.getEntity());
			}));

			AddBinding(new TriggerBinding<string>("Pandemic", "cureSelected", (
				string json) =>
			{
				this.diseaseProgressionSystem.cureCitizen(EntityManager.tryGetCitizen(this.toolSystem.selected));
			}));

			AddBinding(new TriggerBinding<string>("Pandemic", "cureAll", (
				string json) =>
			{
				this.diseaseProgressionSystem.cureDisease(Entity.Null);
			}));

			AddBinding(new TriggerBinding<string>("Pandemic", "infectCitizen", (
				string json) =>
			{
				DiseaseCreateInput inp = JsonSerializer.Deserialize<DiseaseCreateInput>(json);
				if (this.diseaseProgressionSystem.validateDisease(inp.getEntity()))
				{
					this.diseaseProgressionSystem.makeCitizenSick(this.toolSystem.selected, inp.getEntity());
				}
			}));
		}
	}
}
