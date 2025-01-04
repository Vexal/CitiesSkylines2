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
	[SettingsUIGroupOrder(diseaseSpreadSettings, diseaseImpactSettings, citizenBehaviorGroup, appearanceSettings, kKeybindingGroup)]
	[SettingsUITabOrder(mainSection, actionsSection)]
	[SettingsUIShowGroupName(citizenBehaviorGroup, diseaseImpactSettings, diseaseSpreadSettings, kKeybindingGroup, appearanceSettings)]
	[SettingsUIKeyboardAction(Mod.impartDiseaseActionName, ActionType.Button, usages: new string[] { Usages.kDefaultUsage, "PTestUsage" })]
	public class PandemicSettings : ModSetting
	{
		public const string mainSection = "Main";
		public const string actionsSection = "ActionsSection";

		public const string appearanceSettings = "Appearance";
		public const string kKeybindingGroup = "KeyBinding";
		public const string citizenBehaviorGroup = "CitizenBehavior";
		public const string diseaseSpreadSettings = "DiseaseSpreadSettings";
		public const string diseaseImpactSettings = "DiseaseImpactSettings";
		public const string kButtonGroup = "Actions";

		internal DiseaseToolSystem diseaseToolSystem;
		internal ForceSicknessSystem forceSicknessSystem;

		public PandemicSettings(IMod mod) : base(mod)
		{
			this.suddenDeathChance = 0;
			this.diseaseSpreadChance = .02f;
			this.diseaseSpreadRadius = 25f;
			this.diseaseSpreadInterval = 60;
			this.maxDiseaseSpreadPerFrame = 1;
			//this.diseaseFleeRadius = 10f;
			this.maskEffectiveness = 65;
			this.showContagiousCircle = true;
			this.contagiousGraphicOpacity = .15f;
		}

		[SettingsUISection(actionsSection, kButtonGroup)]
		public bool MakeEveryoneSickButton { set { this.forceSicknessSystem.makeAllCitizensSick(); } }

		[SettingsUISection(actionsSection, kButtonGroup)]
		public bool DecreaseHealthButton { set { this.forceSicknessSystem.applyDiseasePenalty(true, 10); } }

		//Disease Impact Settings

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
		[SettingsUISection(mainSection, diseaseImpactSettings)]
		public int suddenDeathChance { get; set; }

		//Disease Spread Settings
		[SettingsUISlider(min = 0, max = 100, step = .001f, unit = Unit.kFloatThreeFractions)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float diseaseSpreadChance { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = 1, unit = Unit.kPercentage)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float maskEffectiveness { get; set; }

		[SettingsUISlider(min = 0, max = 100, step = .1f, scalarMultiplier = 1)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float diseaseSpreadRadius { get; set; }

		[SettingsUISlider(min = 1, max = 600, step = 1, scalarMultiplier = 1)]
		[SettingsUISection(mainSection, diseaseSpreadSettings)]
		public float diseaseSpreadInterval { get; set; }

		/*[SettingsUISlider(min = 1, max = 600, step = 1, scalarMultiplier = 1)]
		[SettingsUISection(mainSection, kSliderGroup)]
		public float diseaseFleeRadius { get; set; }*/

		[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1)]
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

		//Key bindings
		[SettingsUIKeyboardBinding(BindingKeyboard.X, Mod.impartDiseaseActionName, shift: true)]
		[SettingsUISection(mainSection, kKeybindingGroup)]
		public ProxyBinding impartDiseaseKeyBinding { get; set; }

		public override void SetDefaults()
		{
			this.suddenDeathChance = 0;
			this.diseaseSpreadChance = .02f;
			this.diseaseSpreadRadius = 5f;
			//this.diseaseFleeRadius = 10f;
			this.diseaseSpreadInterval = 60;
			this.maxDiseaseSpreadPerFrame = 100;
			this.maskEffectiveness = 65;
			this.showContagiousCircle = true;
			this.contagiousGraphicOpacity = .15f;
			this.underEducatedModifier = UnderEducatedPolicyAdherenceModifier.Minor;
			this.diseaseProgressionSpeed = DiseaseProgression.Minor;
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

				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.appearanceSettings), "Appearance Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.diseaseSpreadSettings), "Disease Spread Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.citizenBehaviorGroup), "Citizen Behavior Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.diseaseImpactSettings), "Disease Progression Settings" },
				{ m_Setting.GetOptionGroupLocaleID(PandemicSettings.kKeybindingGroup), "Key Bindings" },



				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.MakeEveryoneSickButton)), "Make All Citizens Sick" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.MakeEveryoneSickButton)), $"Make all citizens sick." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.DecreaseHealthButton)), "Decrease Health" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.DecreaseHealthButton)), $"Decrease all health." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.suddenDeathChance)), "Late-stage Death Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.suddenDeathChance)), $"The % chance for a citizen at 0 health to die" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.diseaseSpreadChance)), "Disease Spread Chance" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.diseaseSpreadChance)), $"The % chance for a contagious citizen to spread disease to a nearby citizen. The chance of spreading falls off with distance." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.maskEffectiveness)), "Mask Effectiveness" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.maskEffectiveness)), $"The % reduction in chance to spread or contract contagious sickness for citizens wearing masks." },
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.underEducatedModifier)), "Education Policy Adherence Impact" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.underEducatedModifier)), $"The impact of under-education on citizens' adherence to health policies such as Mask Mandates." },

				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.None), "None" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Minor), "Minor" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Moderate), "Moderate" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Severe), "Severe" },
				{ m_Setting.GetEnumValueLocaleID(PandemicSettings.UnderEducatedPolicyAdherenceModifier.Extreme), "Extreme" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.diseaseSpreadRadius)), "Disease Spread Radius" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.diseaseSpreadRadius)), $"The distance at which a contagious citizen can spread disease to nearby citizens. The chance of spreading falls off with distance." },

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
				{ m_Setting.GetOptionLabelLocaleID(nameof(PandemicSettings.impartDiseaseKeyBinding)), "Apply Contagious Disease Key" },
				{ m_Setting.GetOptionDescLocaleID(nameof(PandemicSettings.impartDiseaseKeyBinding)), $"Key binding to apply contagious disease to citizen." },
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
