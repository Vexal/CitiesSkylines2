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
	[SettingsUIGroupOrder(diseaseRaritySettings, diseaseSpreadSettings, ccDiseaseGrp, flDiseaseGrp, diseaseImpactSettings, citizenBehaviorGroup, appearanceSettings, kKeybindingGroup)]
	[SettingsUITabOrder(mainSection, actionsSection, actionsSection)]
	[SettingsUIShowGroupName(diseaseRaritySettings, citizenBehaviorGroup, ccDiseaseGrp, flDiseaseGrp, diseaseImpactSettings, diseaseSpreadSettings, kKeybindingGroup, appearanceSettings)]
	public class PandemicSettings : ModSetting
	{
		public const string mainSection = "Main";
		public const string diseaseSection = "Diseases";
		public const string actionsSection = "ActionsSection";

		public const string appearanceSettings = "Appearance";
		public const string kKeybindingGroup = "KeyBinding";
		public const string citizenBehaviorGroup = "CitizenBehavior";
		public const string diseaseSpreadSettings = "DiseaseSpreadSettings";
		public const string diseaseImpactSettings = "DiseaseImpactSettings";
		public const string diseaseRaritySettings = "DiseaseRaritySettings";
		public const string kButtonGroup = "Actions";
		public const string ccDiseaseGrp = "CommonCold";
		public const string flDiseaseGrp = "Flu";

		public PandemicSettings(IMod mod) : base(mod)
		{
			/*this.diseaseSpreadInterval = 60;
			this.maxDiseaseSpreadPerFrame = 100;
			//this.diseaseFleeRadius = 10f;
			this.maskEffectiveness = 65;
			this.showContagiousCircle = true;
			this.contagiousGraphicOpacity = .15f;

			this.setDiseaseDefaults();

			this.newDiseaseChance = 5;
			this.ccChance = 100;
			this.flChance = 30;
			this.exChance = 1;
			this.modEnabled = true;*/
		}

		[SettingsUISlider(min = 0, max = 100, step = .1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(mainSection, diseaseRaritySettings)]
		public float newDiseaseChance { get; set; }

		//Disease Chances
		[SettingsUISlider(min = 0, max = 100, step = .1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(mainSection, diseaseRaritySettings)]
		public float ccChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(mainSection, diseaseRaritySettings)]
		public float flChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(mainSection, diseaseRaritySettings)]
		public float exChance { get; set; }

		[SettingsUISlider(min = 60, max = 1000, step = 5, unit = Unit.kInteger)]
		[SettingsUISection(mainSection, diseaseRaritySettings)]
		public int globalMutationCooldown { get; set; }

		[SettingsUISection(diseaseSection, kButtonGroup)]
		public bool resetDefaulDiseasesButton { set { this.setDiseaseDefaults(); this.ApplyAndSave(); } }

		/*
		 * Common Cold
		 */
		[SettingsUISlider(min = 0, max = 100, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public float ccMutationChance { get; set; }

		[SettingsUISlider(min = 0, max = 1.99f, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public float ccMutationMagnitude { get; set; }

		[SettingsUISlider(min = 0, max = 1, step = .001f)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public float ccProgressionSpeed { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .1f, scalarMultiplier = 1)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public float ccSpreadRadius { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public float ccSpreadChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public int ccDeathChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(diseaseSection, ccDiseaseGrp)]
		public int ccHealthImpact { get; set; }

		/*
		 * Flu
		 */
		[SettingsUISlider(min = 0, max = 100, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public float flMutationChance { get; set; }

		[SettingsUISlider(min = 0, max = 1.99f, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public float flMutationMagnitude { get; set; }

		[SettingsUISlider(min = 0, max = 1, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public float flProgressionSpeed { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .1f, scalarMultiplier = 1)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public float flSpreadRadius { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public float flSpreadChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public int flDeathChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(diseaseSection, flDiseaseGrp)]
		public int flHealthImpact { get; set; }


		//Disease Impact Settings

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


		[SettingsUISection(mainSection, diseaseImpactSettings)]
		public DiseaseProgression diseaseProgressionSpeed { get; set; } = DiseaseProgression.Minor;

		//Appearance
		[SettingsUISection(mainSection, appearanceSettings)]
		public bool showContagiousCircle { get; set; }

		[SettingsUISlider(min = 0.01f, max = 1, step = .01f, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
		[SettingsUISection(mainSection, appearanceSettings)]
		[SettingsUIDisableByCondition(typeof(PandemicSettings), nameof(hideContagiousAppearanceOptions))]
		public float contagiousGraphicOpacity { get; set; }

		private bool hideContagiousAppearanceOptions => !this.showContagiousCircle;

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
			this.contagiousGraphicOpacity = .15f;
			this.underEducatedModifier = UnderEducatedPolicyAdherenceModifier.Minor;

			this.setDiseaseDefaults();

			this.newDiseaseChance = 5;
			this.ccChance = 100;
			this.flChance = 0;
			this.exChance = 0;
			this.modEnabled = true;
			this.globalMutationCooldown = 60 * 30;

		}

		public void setDiseaseDefaults()
		{
			this.ccMutationChance = .005f;
			this.ccMutationMagnitude = .15f;
			this.ccProgressionSpeed = .015f;
			this.ccHealthImpact = 0;
			this.ccDeathChance = 0;
			this.ccSpreadChance = .02f;
			this.ccSpreadRadius = 10f;

			this.flMutationChance = .05f;
			this.flMutationMagnitude = .35f;
			this.flProgressionSpeed = .1f;
			this.flHealthImpact = 5;
			this.flDeathChance = 2;
			this.flSpreadChance = .1f;
			this.flSpreadRadius = 7;
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
				{ m_Setting.GetOptionTabLocaleID(PandemicSettings.actionsSection), "Actions" },
				{ m_Setting.GetOptionTabLocaleID(PandemicSettings.diseaseSection), "Diseases Config" },

				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.appearanceSettings), "Appearance Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.diseaseRaritySettings), "Relative Disease Rarity" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.diseaseSpreadSettings), "Disease Spread Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.citizenBehaviorGroup), "Citizen Behavior Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.diseaseImpactSettings), "Disease Progression Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.kKeybindingGroup), "Key Bindings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.ccDiseaseGrp), "Common Cold" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.flDiseaseGrp), "Influenza" },

				/**
				 * Disease chance
				 */
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.newDiseaseChance)), "New Disease Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.newDiseaseChance)), $"The % chance for a citizen who becomes spontanously sick via the normal base sick mechanic, to contract a disease that does not yet exist." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccChance)), "Common Cold Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccChance)), $"The weighted chance for a new disease to be a Common Cold" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flChance)), "Flu Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flChance)), $"The weighted chance for a new disease to be a Flu" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.exChance)), "Novel Disease Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.exChance)), $"The weighted chance for a new disease to be a unique strain with highly variable parameters." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.globalMutationCooldown)), "Global Mutation Cooldown" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.globalMutationCooldown)), $"The minimum number of frames between disease mutations or creations." },
				
				/**
				 * Common Cold
				 */
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccMutationChance)), "Mutation Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccMutationChance)), $"The % chance for a common cold strain to mutate upon spread." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccMutationMagnitude)), "Mutation Variability" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccMutationMagnitude)), $"The upper bound in disease parameter variability upon mutation." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccProgressionSpeed)), "Progression Speed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccProgressionSpeed)), $"The rate at which the common cold sickness advances for a sick citizen." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccDeathChance)), "Late-stage Death Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccDeathChance)), $"The % chance for a citizen at 0 health to die" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccSpreadChance)), "Disease Spread Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccSpreadChance)), $"The % chance for a contagious citizen to spread disease to a nearby citizen. The chance of spreading falls off with distance." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccHealthImpact)), "Health Impact" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccHealthImpact)), $"The amount of health to drain per tick." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.ccSpreadRadius)), "Disease Spread Radius" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.ccSpreadRadius)), $"The distance at which a contagious citizen can spread disease to nearby citizens. The chance of spreading falls off with distance." },

				/**
				 * Flu
				 */
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flMutationChance)), "Mutation Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flMutationChance)), $"The % chance for a common cold strain to mutate upon spread." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flMutationMagnitude)), "Mutation Variability" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flMutationMagnitude)), $"The upper bound in disease parameter variability upon mutation." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flProgressionSpeed)), "Progression Speed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flProgressionSpeed)), $"The rate at which the common cold sickness advances for a sick citizen." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flDeathChance)), "Late-stage Death Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flDeathChance)), $"The % chance for a citizen at 0 health to die" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flSpreadChance)), "Disease Spread Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flSpreadChance)), $"The % chance for a contagious citizen to spread disease to a nearby citizen. The chance of spreading falls off with distance." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flHealthImpact)), "Health Impact" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flHealthImpact)), $"The amount of health to drain per tick." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.flSpreadRadius)), "Disease Spread Radius" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.flSpreadRadius)), $"The distance at which a contagious citizen can spread disease to nearby citizens. The chance of spreading falls off with distance." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.resetDefaulDiseasesButton)), "Reset Disease Config" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.resetDefaulDiseasesButton)), $"Reset diseases config to defaults." },
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

				
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.diseaseProgressionSpeed)), "Disease Progress Speed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.diseaseProgressionSpeed)), $"The speed at which disease lowers the citizen's health. A citizen with low health is considered" +
				$"to be in \"late-stage\" severity and may have a higher chance to die" },

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
