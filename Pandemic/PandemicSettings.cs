using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace Pandemic
{
	[FileLocation(nameof(Pandemic))]
	[SettingsUIGroupOrder(kButtonGroup, kToggleGroup, kSliderGroup, kDropdownGroup)]
	[SettingsUIShowGroupName(kButtonGroup, kToggleGroup, kSliderGroup, kDropdownGroup)]
	[SettingsUIKeyboardAction(Mod.impartDiseaseActionName, ActionType.Button, usages: new string[] { Usages.kDefaultUsage, "PTestUsage" })]
	public class PandemicSettings : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "Button";
		public const string kToggleGroup = "Toggle";
		public const string kSliderGroup = "Slider";
		public const string kDropdownGroup = "Dropdown";
		public const string kKeybindingGroup = "KeyBinding";

		internal DiseaseToolSystem diseaseToolSystem;
		internal ForceSicknessSystem forceSicknessSystem;

		public PandemicSettings(IMod mod) : base(mod)
		{
			this.suddenDeathChance = 0;
		}

		[SettingsUISection(kSection, kButtonGroup)]
		public bool MakeEveryoneSickButton { set { this.forceSicknessSystem.makeAllCitizensSick(); } }

		[SettingsUISection(kSection, kButtonGroup)]
		public bool DecreaseHealthButton { set { this.forceSicknessSystem.applyDiseasePenalty(true, 10); } }


		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
		[SettingsUISection(kSection, kSliderGroup)]
		public int suddenDeathChance { get; set; }

		[SettingsUISection(kSection, kDropdownGroup)]
		public DiseaseProgression diseaseProgressionSpeed { get; set; } = DiseaseProgression.Minor;

		[SettingsUIKeyboardBinding(BindingKeyboard.X, Mod.impartDiseaseActionName, shift: true)]
		[SettingsUISection(kSection, kKeybindingGroup)]
		public ProxyBinding impartDiseaseKeyBinding { get; set; }

		public override void SetDefaults()
		{
			this.suddenDeathChance = 0;
		}

		public enum DiseaseProgression
		{
			Vanilla,
			Minor,
			Moderate,
			Severe,
			Extreme
		}
	}

	public class LocaleEN : IDictionarySource
	{
		private readonly PandemicSettings m_Setting;
		public LocaleEN(PandemicSettings setting)
		{
			m_Setting = setting;
		}
		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ m_Setting.GetSettingsLocaleID(), "Pandemic" },
				{ m_Setting.GetOptionTabLocaleID(PandemicSettings.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.kButtonGroup), "Buttons" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.MakeEveryoneSickButton)), "Make All Citizens Sick" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.MakeEveryoneSickButton)), $"Make all citizens sick." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.DecreaseHealthButton)), "Decrease Health" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.DecreaseHealthButton)), $"Decrease all health." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.suddenDeathChance)), "Late-stage Death Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.suddenDeathChance)), $"The % chance for a citizen at 0 health to die" },

				
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.diseaseProgressionSpeed)), "Disease Progress Speed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.diseaseProgressionSpeed)), $"The speed at which disease lowers the citizen's health. A citizen with low health is considered" +
				$"to be in \"late-stage\" severity and may have a higher chance to die" },

				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Vanilla), "Vanilla" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Minor), "Minor" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Moderate), "Moderate" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Severe), "Severe" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Extreme), "Extreme" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.impartDiseaseKeyBinding)), "Apply Contagious Disease Key" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.impartDiseaseKeyBinding)), $"Key binding to apply contagious disease to citizen." },

			};
		}

		public void Unload()
		{

		}
	}
}
