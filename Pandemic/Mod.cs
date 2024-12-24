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
		public static ProxyAction impartDiseaseAction;
		public const string impartDiseaseActionName = "ImpartDiseaseBinding";

		public void OnLoad(UpdateSystem updateSystem)
		{
			INSTANCE = this;
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			m_Setting = new PandemicSettings(this);
			m_Setting.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
			m_Setting.RegisterKeyBindings();
			impartDiseaseAction = m_Setting.GetAction(impartDiseaseActionName);
			impartDiseaseAction.shouldBeEnabled = true;

			AssetDatabase.global.LoadSettings(nameof(Pandemic), m_Setting, new PandemicSettings(this));

			//updateSystem.UpdateBefore<ForceSicknessSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<DiseaseToolSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<PandemicSystem>(SystemUpdatePhase.GameSimulation);
			updateSystem.UpdateBefore<RenderDiseaseSystem>(SystemUpdatePhase.Rendering);
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
