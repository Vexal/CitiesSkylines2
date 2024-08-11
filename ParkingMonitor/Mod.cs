using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace ParkingMonitor
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ParkingMonitor)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		public Setting m_Setting;
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


			AssetDatabase.global.LoadSettings(nameof(ParkingMonitor), m_Setting, new Setting(this));

			updateSystem.UpdateBefore<ParkingMonitorSystem>(SystemUpdatePhase.Modification1);
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
