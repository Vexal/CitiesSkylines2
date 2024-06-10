using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace DCMilestoneRewards
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(DCMilestoneRewards)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		private Setting m_Setting;
		public static Mod INSTANCE;

		public void OnLoad(UpdateSystem updateSystem)
		{
			INSTANCE = this;
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			m_Setting = new Setting(this);
			m_Setting.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

			AssetDatabase.global.LoadSettings(nameof(DCMilestoneRewards), m_Setting, new Setting(this));
			updateSystem.UpdateBefore<MilestoneSystem>(SystemUpdatePhase.GameSimulation);
		}

		public Setting settings()
		{
			return this.m_Setting;
		}

		public void OnDispose()
		{
			log.Info(nameof(OnDispose));
			if (m_Setting != null)
			{
				m_Setting.UnregisterInOptionsUI();
				m_Setting = null;
			}
		}
	}
}
