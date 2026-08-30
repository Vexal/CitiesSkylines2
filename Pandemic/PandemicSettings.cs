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
	[SettingsUIGroupOrder(diseaseSpreadSettings, citizenBehaviorGroup, appearanceSettings, kKeybindingGroup)]
	[SettingsUITabOrder(mainSection)]
	[SettingsUIShowGroupName(citizenBehaviorGroup, diseaseSpreadSettings, kKeybindingGroup, appearanceSettings)]
	public class PandemicSettings : ModSetting
	{
		public const string mainSection = "Main";

		public const string appearanceSettings = "Appearance";
		public const string kKeybindingGroup = "KeyBinding";
		public const string citizenBehaviorGroup = "CitizenBehavior";
		public const string diseaseSpreadSettings = "DiseaseSpreadSettings";

		public PandemicSettings(IMod mod) : base(mod)
		{
			this.SetDefaults();
		}

		[SettingsUISlider(min = 60, max = 1000, step = 5, unit = Unit.kInteger)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public int globalMutationCooldown { get; set; }


		//Disease Spread Settings

		[SettingsUISlider(min = 0, max = 100, step = 1, unit = Unit.kPercentage)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float maskEffectiveness { get; set; }

		[SettingsUISlider(min = 1, max = 600, step = 1, scalarMultiplier = 1)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float diseaseSpreadInterval { get; set; }

		/*[SettingsUISlider(min = 1, max = 600, step = 1, scalarMultiplier = 1)]
		[SettingsUISection(mainSection, kSliderGroup)]
		public float diseaseFleeRadius { get; set; }*/

		[SettingsUISlider(min = 0, max = 10000, step = 1, scalarMultiplier = 1)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float maxDiseaseSpreadPerFrame { get; set; }


		//Appearance
		[SettingsUISection(mainSection, appearanceSettings)]
		public bool showContagiousCircle { get; set; }

		[SettingsUISlider(min = 0.01f, max = 1, step = .01f, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
		[SettingsUISection(mainSection, appearanceSettings)]
		[SettingsUIDisableByCondition(typeof(PandemicSettings), nameof(hideContagiousAppearanceOptions))]
		public float contagiousGraphicOpacity { get; set; }

		private bool hideContagiousAppearanceOptions => !this.showContagiousCircle;

		[SettingsUISection(mainSection, appearanceSettings)]
		public bool showCitizenHealth { get; set; }

		[SettingsUISection(mainSection, appearanceSettings)]
		public bool showActiveDiseaseDetails { get; set; }

		//Citizen behavior
		[SettingsUISection(mainSection, citizenBehaviorGroup)]
		public UnderEducatedPolicyAdherenceModifier underEducatedModifier { get; set; } = UnderEducatedPolicyAdherenceModifier.Minor;
		//Appearance
		[SettingsUISection(mainSection, appearanceSettings)]
		public bool modEnabled { get; set; }

		[SettingsUISection(mainSection, appearanceSettings)]
		public bool resetAllDefaults { set { this.SetDefaults(); this.ApplyAndSave(); } }

		public override void SetDefaults()
		{
			//this.diseaseFleeRadius = 10f;
			this.diseaseSpreadInterval = 60;
			this.maxDiseaseSpreadPerFrame = 100;
			this.maskEffectiveness = 65;
			this.showContagiousCircle = true;
			this.showActiveDiseaseDetails = true;
			this.contagiousGraphicOpacity = .15f;
			this.underEducatedModifier = UnderEducatedPolicyAdherenceModifier.Minor;

			this.modEnabled = true;
			this.globalMutationCooldown = 60 * 30;
			this.showCitizenHealth = true;

		}

		public enum DiseaseProgression
		{
			Vanilla,
			Minor,
			Moderate,
			Severe,
			Extreme
		}

		public enum UnderEducatedPolicyAdherenceModifier
		{
			None,
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
				{ m_Setting.GetOptionTabLocaleID(PandemicSettings.mainSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.appearanceSettings), "Appearance Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.diseaseSpreadSettings), "Disease Spread Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.citizenBehaviorGroup), "Citizen Behavior Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.kKeybindingGroup), "Key Bindings" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.globalMutationCooldown)), "Global Mutation Cooldown" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.globalMutationCooldown)), $"The minimum number of frames between disease mutations or creations." },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.resetAllDefaults)), "Reset Config" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.resetAllDefaults)), $"Reset all options to defaults." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.modEnabled)), "Mod Enabled" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.modEnabled)), $"Enable / disable the pandemic mod." },

				
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.maskEffectiveness)), "Mask Effectiveness" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.maskEffectiveness)), $"The % reduction in chance to spread or contract contagious sickness for citizens wearing masks." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.underEducatedModifier)), "Education Policy Adherence Impact" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.underEducatedModifier)), $"The impact of under-education on citizens' adherence to health policies such as Mask Mandates." },

				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.None), "None" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Minor), "Minor" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Moderate), "Moderate" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Severe), "Severe" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Extreme), "Extreme" },

				
			    //{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.diseaseFleeRadius)), "Disease Flee Radius" },
				//{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.diseaseFleeRadius)), $"The distance at which nearby citizens will flee contagious citizens." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.diseaseSpreadInterval)), "Disease Spread Frequency" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.diseaseSpreadInterval)), $"The interval at which disease spread is checked; lower is faster." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.maxDiseaseSpreadPerFrame)), "Max Disease Spread per Tick" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.maxDiseaseSpreadPerFrame)), $"The maximum number of additional citizens who can become sick each update." },

				//Appearance
			    { m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.showContagiousCircle)), "Show Contagious Bounds Indicator" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.showContagiousCircle)), $"Display a circle around contagious citizens, whose size indicates how far the citizen can spread disease, based on all factors (such as whether they're wearing a mask)." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.contagiousGraphicOpacity)), "Contagious Radius Graphic Opacity" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.contagiousGraphicOpacity)), $"The opacity of the circle graphic indicating the contagious radius of a sick citizen." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.showCitizenHealth)), "Show Selected Citizen Health Information" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.showCitizenHealth)), $"Display the disease information for selected citizens, if the citizen is currently sick." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.showActiveDiseaseDetails)), "Show Active Disease Details (Health Infoview)" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.showActiveDiseaseDetails)), $"Display the list of active diseases in the Health Infoview." },


				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Vanilla), "Vanilla" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Minor), "Minor" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Moderate), "Moderate" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Severe), "Severe" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.DiseaseProgression.Extreme), "Extreme" },
				{"Policy.TITLE[Mask Mandate]", "Mask Mandate" },
				{"Policy.DESCRIPTION[Mask Mandate]", $"Require all citizens to wear masks, drastically decreasing the chance of spreading or catching " +
				$"contagious sickness.\n\nDecreases citizen happiness.\nLower education citizens have a higher chance of defying the mask mandate." },

			};
		}

		public void Unload()
		{

		}
	}
}
