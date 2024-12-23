using Colossal;
using Colossal.IO.AssetDatabase;
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
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "Button";
		public const string kToggleGroup = "Toggle";
		public const string kSliderGroup = "Slider";
		public const string kDropdownGroup = "Dropdown";

		internal ForceSicknessSystem forceSicknessSystem;

		public Setting(IMod mod) : base(mod)
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
		private readonly Setting m_Setting;
		public LocaleEN(Setting setting)
		{
			m_Setting = setting;
		}
		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ m_Setting.GetSettingsLocaleID(), "Pandemic" },
				{ m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Buttons" },
				{ m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Toggle" },
				{ m_Setting.GetOptionGroupLocaleID(Setting.kSliderGroup), "Sliders" },
				{ m_Setting.GetOptionGroupLocaleID(Setting.kDropdownGroup), "Dropdowns" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.MakeEveryoneSickButton)), "Make All Citizens Sick" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.MakeEveryoneSickButton)), $"Make all citizens sick." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.DecreaseHealthButton)), "Decrease Health" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.DecreaseHealthButton)), $"Decrease all health." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.suddenDeathChance)), "Late-stage Death Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.suddenDeathChance)), $"The % chance for a citizen at 0 health to die" },

				
				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.diseaseProgressionSpeed)), "Disease Progress Speed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.diseaseProgressionSpeed)), $"The speed at which disease lowers the citizen's health. A citizen with low health is considered" +
				$"to be in \"late-stage\" severity and may have a higher chance to die" },

				{ m_Setting.GetEnumValueLocaleID(Setting.DiseaseProgression.Vanilla), "Vanilla" },
				{ m_Setting.GetEnumValueLocaleID(Setting.DiseaseProgression.Minor), "Minor" },
				{ m_Setting.GetEnumValueLocaleID(Setting.DiseaseProgression.Moderate), "Moderate" },
				{ m_Setting.GetEnumValueLocaleID(Setting.DiseaseProgression.Severe), "Severe" },
				{ m_Setting.GetEnumValueLocaleID(Setting.DiseaseProgression.Extreme), "Extreme" },

			};
		}

		public void Unload()
		{

		}
	}
}
