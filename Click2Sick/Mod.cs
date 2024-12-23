using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using UnityEngine;

namespace Click2Sick
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(Click2Sick)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		public static Mod INSTANCE;
		public Click2SickSettings m_Setting;
		public static ProxyAction makeSelectedSickAction;
		public static ProxyAction healSelectedAction;
		public static ProxyAction decreaseHealthSelectedAction;

		public const string makeSelectedSickActionName = "MakeSelectedSickBinding";
		public const string healSelectedActionName = "HealSelectedBinding";
		public const string decreaseHealthSelectedActionName = "DecreaseHealthSelectedBinding";

		public void OnLoad(UpdateSystem updateSystem)
		{
			INSTANCE = this;
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			m_Setting = new Click2SickSettings(this);
			m_Setting.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

			m_Setting.RegisterKeyBindings();

			makeSelectedSickAction = m_Setting.GetAction(makeSelectedSickActionName);
			makeSelectedSickAction.shouldBeEnabled = true;

			healSelectedAction = m_Setting.GetAction(healSelectedActionName);
			healSelectedAction.shouldBeEnabled = true;
			
			decreaseHealthSelectedAction = m_Setting.GetAction(decreaseHealthSelectedActionName);
			decreaseHealthSelectedAction.shouldBeEnabled = true;

			AssetDatabase.global.LoadSettings(nameof(Click2Sick), m_Setting, new Click2SickSettings(this));
			updateSystem.UpdateBefore<ClickSicknessSystem>(SystemUpdatePhase.GameSimulation);
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
