using Colossal.UI.Binding;
using Game;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using Unity.Entities;

namespace BuildingUsageTracker
{
	internal abstract partial class SelectedBuildingInfoSection : InfoSectionBase
	{
		protected UIUpdateState uf;
		private Entity previousSelectedEntity = Entity.Null;
		private ToolSystem toolSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.uf = UIUpdateState.Create(World, 60);
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_InfoUISystem.AddMiddleSection(this);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (!Enabled)
			{
				this.Enabled = true;
			}

			Entity selectedEntity = this.toolSystem.selected;
			if (selectedEntity != this.previousSelectedEntity)
			{
				this.previousSelectedEntity = selectedEntity;
				this.uf.ForceUpdate();
				this.selectionChanged();
				if (!EntityManager.isBuilding(selectedEntity))
				{
					this.visible = false;
					return;
				}
			}

			if (!EntityManager.Exists(selectedEntity))
			{
				this.visible = false;
				return;
			}

			if (this.uf.Advance())
			{
				this.update(selectedEntity);
			}

			this.visible = true;
		}

		protected abstract void selectionChanged();

		protected abstract void update(Entity selectedEntity);

		public override GameMode gameMode => GameMode.Game;


		public override void OnWriteProperties(IJsonWriter writer)
		{
			
		}

		protected override void OnProcess()
		{
			
		}

		protected override void Reset()
		{
			
		}
	}
}
