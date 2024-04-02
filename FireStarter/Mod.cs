using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.SceneFlow;

namespace FireStarter
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(FireStarter)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		private Setting m_Setting;
		public static Mod INSTANCE;

		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			m_Setting = new Setting(this);
			m_Setting.RegisterInOptionsUI(); 
			INSTANCE = this;
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

			AssetDatabase.global.LoadSettings(nameof(FireStarter), m_Setting, new Setting(this));
			updateSystem.UpdateBefore<FireStarterSystem>(SystemUpdatePhase.GameSimulation);
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
