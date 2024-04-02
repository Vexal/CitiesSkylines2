using Colossal.UI.Binding;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using System;
using Unity.Entities;

namespace FireStarter
{
	internal partial class FireStarterSystem : UISystemBase
	{
		ToolSystem toolSystem;
		Entity selectedEntity; 
		private FireStarter fireStarter;
		private ValueBinding<bool> toolActiveBinding;     
		private ValueBinding<bool> settingActiveBinding;     

		protected override void OnCreate()   
		{
			base.OnCreate();
			this.fireStarter = new FireStarter(World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>(), EntityManager);
			AddBinding(new TriggerBinding<string>("FireStarter", "test", test));
			this.toolActiveBinding = new ValueBinding<bool>("FireStarter", "FireToolActive", false);
			this.settingActiveBinding = new ValueBinding<bool>("FireStarter", "FireToolSettingActive", Mod.INSTANCE.settings().enabled);

			Mod.INSTANCE.settings().onSettingsApplied += settings =>
			{
				if (settings.GetType() == typeof(Setting))
				{
					this.settingActiveBinding.Update(((Setting)settings).enabled);
				}
			};
			AddBinding(this.toolActiveBinding);
			AddBinding(this.settingActiveBinding);
		}

		protected override void OnStartRunning() 
		{
			base.OnStartRunning();
			
		}

		protected override void OnUpdate()
		{
			if (!this.toolActiveBinding.value || !Mod.INSTANCE.settings().enabled)
			{
				return; 
			}

			Entity selected = this.getSelected();

			bool updatedSelection = selected != this.selectedEntity;
			if (updatedSelection)
			{				
				this.selectedEntity = selected;
				this.fireStarter.createFire(this.selectedEntity); 
			}
		}


		public void test(String s)
		{
			//Mod.log.Info("Testing trigger " + s + " the value binding is: " + this.toolActiveBinding.value);
			this.toolActiveBinding.Update(!this.toolActiveBinding.value);
		}

		private Entity getSelected()
		{
			ToolSystem toolSystem = World.GetExistingSystemManaged<ToolSystem>();
			return toolSystem.selected;
		}
	}
}
