using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace Click2Sick
{
	[FileLocation(nameof(Click2Sick))]
	[SettingsUIGroupOrder(kButtonGroup, kKeybindingGroup)]
	[SettingsUIShowGroupName(kButtonGroup, kKeybindingGroup)]
	[SettingsUIKeyboardAction(Mod.makeSelectedSickActionName, ActionType.Button, usages: new string[] { Usages.kDefaultUsage, "MSTestUsage" })]
	[SettingsUIKeyboardAction(Mod.healSelectedActionName, ActionType.Button, usages: new string[] { Usages.kDefaultUsage, "MSTestUsage" })]
	[SettingsUIKeyboardAction(Mod.decreaseHealthSelectedActionName, ActionType.Button, usages: new string[] { Usages.kDefaultUsage, "MSTestUsage" })]
	public class Click2SickSettings : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "Actions";
		public const string kKeybindingGroup = "KeyBinding";
		internal ClickSicknessSystem clickSicknessSystem;

		public Click2SickSettings(IMod mod) : base(mod)
		{

		}

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(kSection, kButtonGroup)]
		[SettingsUIDisableByCondition(typeof(Click2SickSettings), nameof(disableActionsCondition))]
		public bool MakeEveryoneSickButton { set { this.clickSicknessSystem.triggerMakeAllCitizensSick(); } }

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(kSection, kButtonGroup)]
		[SettingsUIDisableByCondition(typeof(Click2SickSettings), nameof(disableActionsCondition))]
		public bool healAllCitizensButton { set { this.clickSicknessSystem.triggerHealAllCitizens(); } }

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(kSection, kButtonGroup)]
		[SettingsUIDisableByCondition(typeof(Click2SickSettings), nameof(disableActionsCondition))]
		public bool decreaseAllHealthButton { set { this.clickSicknessSystem.triggerDecreaseHealthAll(); } }


		[SettingsUIKeyboardBinding(BindingKeyboard.S, Mod.makeSelectedSickActionName, shift:true, ctrl: true)]
		[SettingsUISection(kSection, kKeybindingGroup)]
		public ProxyBinding makeSelectedSickKeybinding { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.H, Mod.healSelectedActionName, shift:true, ctrl: true)]
		[SettingsUISection(kSection, kKeybindingGroup)]
		public ProxyBinding healSelectedKeybinding { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.D, Mod.decreaseHealthSelectedActionName, shift:true, ctrl: true)]
		[SettingsUISection(kSection, kKeybindingGroup)]
		public ProxyBinding decreaseSelectedHealthKeybinding { get; set; }

		private bool disableActionsCondition => GameManager.instance.gameMode != Game.GameMode.Game;

		[SettingsUISection(kSection, kKeybindingGroup)]
		public bool ResetBindings
		{
			set
			{
				ResetKeyBindings();
			}
		}


		public override void SetDefaults()
		{

		}
	}

	public class LocaleEN : IDictionarySource
	{
		private readonly Click2SickSettings m_Setting;
		public LocaleEN(Click2SickSettings setting)
		{
			m_Setting = setting;
		}
		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ m_Setting.GetSettingsLocaleID(), "Click 2 Sick" },
				{ m_Setting.GetOptionTabLocaleID(Click2SickSettings.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(Click2SickSettings.kButtonGroup), "Health Actions (must be in game)" },
				{ m_Setting.GetOptionGroupLocaleID(Click2SickSettings.kKeybindingGroup), "Key bindings" },

				//{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.Button)), "Button" },
				//{ m_Setting.GetOptionDescLocaleID(nameof(Setting.Button)), $"Simple single button. It should be bool property with only setter or use [{nameof(SettingsUIButtonAttribute)}] to make button from bool property with setter and getter" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.MakeEveryoneSickButton)), "Make All Citizens Sick" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.MakeEveryoneSickButton)), $"Click this button to make all citizens in this city sick. This has a devestating effect on the city; ensure your save is backed up first." },
				{ m_Setting.GetOptionWarningLocaleID(nameof(Click2SickSettings.MakeEveryoneSickButton)), "Make all citizens in this city sick (make sure to back up your save first)?" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.healAllCitizensButton)), "Heal All Citizens" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.healAllCitizensButton)), $"Click this button to remove sickness and injury from all citizens." },
				{ m_Setting.GetOptionWarningLocaleID(nameof(Click2SickSettings.healAllCitizensButton)), "Remove all sickness and injury from all citizens?" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.decreaseAllHealthButton)), "Decrease All Citizens' Health" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.decreaseAllHealthButton)), $"Click this button to decrease the current health of all citizens by 25." },
				{ m_Setting.GetOptionWarningLocaleID(nameof(Click2SickSettings.decreaseAllHealthButton)), "Decrease the current health of all citizens?" },


				{ m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.makeSelectedSickKeybinding)), "Make Selected Citizen or Building Occupants Sick Key Binding" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.makeSelectedSickKeybinding)), $"Key binding to make the selected citizen, or all citizens currently inside a building, sick." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.healSelectedKeybinding)), "Remove sickness from selected citizen keybinding" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.healSelectedKeybinding)), $"Key binding to remove sickness/injury from the selected citizen" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.decreaseSelectedHealthKeybinding)), "Decrease selected citizen's health key binding" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.decreaseSelectedHealthKeybinding)), $"Key binding to decrease the health of the selected citizen by 25." },


				{ m_Setting.GetOptionLabelLocaleID(nameof(Click2SickSettings.ResetBindings)), "Reset key bindings" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Click2SickSettings.ResetBindings)), $"Reset all key bindings of the mod" },

				{ m_Setting.GetBindingKeyLocaleID(Mod.makeSelectedSickActionName), "Make Selected Sick Key" }
			};
		}

		public void Unload()
		{

		}
	}
}
