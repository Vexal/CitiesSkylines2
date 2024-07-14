using Game;
using Game.Simulation;

namespace NoTrafficDespawn
{
	public partial class DisableTrafficDespawnSystem : GameSystemBase
	{
		private StuckMovingObjectSystem stuckMovingObjectSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.stuckMovingObjectSystem = World.GetExistingSystemManaged<StuckMovingObjectSystem>();

			Mod.INSTANCE.settings.onSettingsApplied += settings =>
			{
				if (settings.GetType() == typeof(Setting))
				{
					this.stuckMovingObjectSystem.Enabled = !((Setting)settings).trafficDespawnDisabled;
				}
			};

			this.stuckMovingObjectSystem.Enabled = !Mod.INSTANCE.settings.trafficDespawnDisabled;
		}

		protected override void OnUpdate()
		{

		}
	}
}
