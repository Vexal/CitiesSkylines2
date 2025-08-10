using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;

namespace Pandemic
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(Pandemic)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		public PandemicSettings m_Setting;
		public static Mod INSTANCE;

		public void OnLoad(UpdateSystem updateSystem)
		{
			INSTANCE = this;
			log.Info(nameof(OnLoad));
			
			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			m_Setting = new PandemicSettings(this);
			m_Setting.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
			//m_Setting.RegisterKeyBindings();

			AssetDatabase.global.LoadSettings(nameof(Pandemic), m_Setting, new PandemicSettings(this));

			//updateSystem.UpdateBefore<ForceSicknessSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<PandemicSpreadSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<RenderDiseaseSystem>(SystemUpdatePhase.Rendering);
			updateSystem.UpdateAt<HealthInfoUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<DiseaseControlUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateBefore<DiseaseProgressionSystem>(SystemUpdatePhase.GameSimulation);
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

		public static PandemicSettings settings { get => Mod.INSTANCE.m_Setting; }
	}
}
