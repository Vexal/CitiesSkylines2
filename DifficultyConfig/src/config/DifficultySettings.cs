using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace DifficultyConfig
{
	[FileLocation(nameof(DifficultyConfig))]
	[SettingsUIGroupOrder(moneyGroup, workplaceEfficiencyGroup, lossGroup)]
	[SettingsUIShowGroupName(moneyGroup, workplaceEfficiencyGroup, lossGroup)]
	public class DifficultySettings : ModSetting
	{
		public const string kSection = "Main";

		public const string moneyGroup = "Money";
		public const string workplaceEfficiencyGroup = "Workplace Efficiency";
		public const string lossGroup = "Loss Criteria";

		public DifficultySettings(IMod mod) : base(mod)
		{
			this.disableMilestoneRewards = false;
			this.subsidyType = SubsidyType.DEFAULT;
			this.allowGameLoss = false;
			this.minimumMoneyLoss = -1900000000;
			this.lossSpeed = 5;
		}

		public override void SetDefaults()
		{
			this.disableMilestoneRewards = false;
			this.subsidyType = SubsidyType.DEFAULT;
			this.allowGameLoss = false;
			this.minimumMoneyLoss = -1900000000;
			this.lossSpeed = 5;
		}

		[SettingsUISection(kSection, moneyGroup)]
		public bool disableMilestoneRewards { get; set; }

		[SettingsUISection(kSection, moneyGroup)]
		public SubsidyType subsidyType { get; set; }

		public enum SubsidyType
		{
			NEGATIVE,
			NONE,
			DEFAULT,
			HIGH
		}

		//workplace efficiency

		[SettingsUISection(kSection, workplaceEfficiencyGroup)]
		public bool requireEmployeePresence { get; set; }

		//game loss

		[SettingsUISection(kSection, lossGroup)]
		public bool allowGameLoss { get; set; }

		[SettingsUISlider(min = -1900000000, max = 100000000, step = 100000, scalarMultiplier = 1, unit = Unit.kMoney)]
		[SettingsUISection(kSection, lossGroup)]
		public int minimumMoneyLoss { get; set; }

		[SettingsUISlider(min = 1, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(kSection, lossGroup)]
		public int lossSpeed { get; set; }

		/*
		[SettingsUISection(kSection, kButtonGroup)]
		public bool Button { set { Mod.log.Info("Button clicked"); } }

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(kSection, kButtonGroup)]
		public bool ButtonWithConfirmation { set { Mod.log.Info("ButtonWithConfirmation clicked"); } }

		[SettingsUISection(kSection, kToggleGroup)]
		public bool Toggle { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kDataMegabytes)]
		[SettingsUISection(kSection, kSliderGroup)]
		public int IntSlider { get; set; }

		[SettingsUIDropdown(typeof(Setting), nameof(GetIntDropdownItems))]
		[SettingsUISection(kSection, kDropdownGroup)]
		public int IntDropdown { get; set; }

		[SettingsUISection(kSection, kDropdownGroup)]
		public SomeEnum EnumDropdown { get; set; } = SomeEnum.Value1;

		public DropdownItem<int>[] GetIntDropdownItems()
		{
			var items = new List<DropdownItem<int>>();

			for (var i = 0; i < 3; i += 1)
			{
				items.Add(new DropdownItem<int>()
				{
					value = i,
					displayName = i.ToString(),
				});
			}

			return items.ToArray();
		}

		public override void SetDefaults()
		{
			throw new System.NotImplementedException();
		}

		public enum SomeEnum
		{
			Value1,
			Value2,
			Value3,
		}*/
	}

	public class LocaleEN : IDictionarySource
	{
		private readonly DifficultySettings settings;
		public LocaleEN(DifficultySettings setting)
		{
			settings = setting;
		}
		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ settings.GetSettingsLocaleID(), "Difficulty Config Mod" },
				{ settings.GetOptionTabLocaleID(DifficultySettings.kSection), "Main" },

				//Money options
				{ settings.GetOptionGroupLocaleID(DifficultySettings.moneyGroup), "Money" },

				{ settings.GetOptionLabelLocaleID(nameof(DifficultySettings.disableMilestoneRewards)), "Disable Milestone Rewards" },
				{ settings.GetOptionDescLocaleID(nameof(DifficultySettings.disableMilestoneRewards)), $"Remove the money bonus when reaching a milestone" },

				{ settings.GetOptionLabelLocaleID(nameof(DifficultySettings.subsidyType)), "Government Subsidies" },
				{ settings.GetOptionDescLocaleID(nameof(DifficultySettings.subsidyType)), $"Modify the government monetary handouts; default behavior is math.clamp(Mathf.RoundToInt(8000f + 5f * (float)population - 0.5f * (float)(moneyDelta + loanInterest)), 0, -expenses - loanInterest)." },

				{ settings.GetEnumValueLocaleID(DifficultySettings.SubsidyType.DEFAULT), "Default" },
				{ settings.GetEnumValueLocaleID(DifficultySettings.SubsidyType.NONE), "None" },
				{ settings.GetEnumValueLocaleID(DifficultySettings.SubsidyType.NEGATIVE), "Negative" },
				{ settings.GetEnumValueLocaleID(DifficultySettings.SubsidyType.HIGH), "Positive" },

				//workplace efficiency
				{ settings.GetOptionGroupLocaleID(DifficultySettings.workplaceEfficiencyGroup), "Workplace Efficiency" },

				{ settings.GetOptionLabelLocaleID(nameof(DifficultySettings.requireEmployeePresence)), "Require Employee Presence" },
				{ settings.GetOptionDescLocaleID(nameof(DifficultySettings.requireEmployeePresence)), $"Penalize companies whose employees cannot make it to the workplace." },


				//Losing game
				{ settings.GetOptionGroupLocaleID(DifficultySettings.lossGroup), "Game Defeat Criteria" },

				{ settings.GetOptionLabelLocaleID(nameof(DifficultySettings.allowGameLoss)), "Allow Game Defeat" },
				{ settings.GetOptionDescLocaleID(nameof(DifficultySettings.allowGameLoss)), $"Allow the player to lose the game when certain negative conditions are reached." },

				{ this.settings.GetOptionLabelLocaleID(nameof(DifficultySettings.minimumMoneyLoss)), "Game Defeat Money Threshold" },
				{ this.settings.GetOptionDescLocaleID(nameof(DifficultySettings.minimumMoneyLoss)), $"Player is defeated when money falls below the set threshold (city simulators in the 1990's did this). Only applies if 'Allow Game Defeat' is toggled" },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(DifficultySettings.lossSpeed)), "Game Defeat Speed" },
				{ this.settings.GetOptionDescLocaleID(nameof(DifficultySettings.lossSpeed)), $"The speed at which the game defeat takes affect after losing (lower is faster)." },

				/*{ m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Buttons" },
				{ m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Toggle" },
				{ m_Setting.GetOptionGroupLocaleID(Setting.kSliderGroup), "Sliders" },
				{ m_Setting.GetOptionGroupLocaleID(Setting.kDropdownGroup), "Dropdowns" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.Button)), "Button" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.Button)), $"Simple single button. It should be bool property with only setter or use [{nameof(SettingsUIButtonAttribute)}] to make button from bool property with setter and getter" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.ButtonWithConfirmation)), "Button with confirmation" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.ButtonWithConfirmation)), $"Button can show confirmation message. Use [{nameof(SettingsUIConfirmationAttribute)}]" },
				{ m_Setting.GetOptionWarningLocaleID(nameof(Setting.ButtonWithConfirmation)), "is it confirmation text which you want to show here?" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.Toggle)), "Toggle" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.Toggle)), $"Use bool property with setter and getter to get toggable option" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.IntSlider)), "Int slider" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.IntSlider)), $"Use int property with getter and setter and [{nameof(SettingsUISliderAttribute)}] to get int slider" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.IntDropdown)), "Int dropdown" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.IntDropdown)), $"Use int property with getter and setter and [{nameof(SettingsUIDropdownAttribute)}(typeof(SomeType), nameof(SomeMethod))] to get int dropdown: Method must be static or instance of your setting class with 0 parameters and returns {typeof(DropdownItem<int>).Name}" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnumDropdown)), "Simple enum dropdown" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.EnumDropdown)), $"Use any enum property with getter and setter to get enum dropdown" },

				{ m_Setting.GetEnumValueLocaleID(Setting.SomeEnum.Value1), "Value 1" },
				{ m_Setting.GetEnumValueLocaleID(Setting.SomeEnum.Value2), "Value 2" },
				{ m_Setting.GetEnumValueLocaleID(Setting.SomeEnum.Value3), "Value 3" },*/
			};
		}


		public void Unload()
		{

		}
	}
}
