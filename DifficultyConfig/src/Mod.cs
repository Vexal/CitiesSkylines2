using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.SceneFlow;
using HarmonyLib;

namespace DifficultyConfig
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(DifficultyConfig)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		public static Mod INSTANCE;

		private DifficultySettings m_Setting;

		public void OnLoad(UpdateSystem updateSystem)
		{
			//DifficultPatcher.DoPatching();
			log.Info(nameof(OnLoad));
			updateSystem.UpdateBefore<DifficultSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<FireStarterSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<CollapseCitySystem>(SystemUpdatePhase.GameSimulation);
			//updateSystem.UpdateBefore<EmployeePresenceSystem>(SystemUpdatePhase.GameSimulation);

			INSTANCE = this;

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			m_Setting = new DifficultySettings(this);
			m_Setting.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

			AssetDatabase.global.LoadSettings(nameof(DifficultyConfig), m_Setting, new DifficultySettings(this));


		}

		public DifficultySettings settings()
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

	public class DifficultPatcher
	{
		public static void DoPatching()
		{
			var harmony = new Harmony("com.example.patch");
			harmony.PatchAll();
		}
	}
}
