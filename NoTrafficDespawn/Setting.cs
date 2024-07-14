using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using System.Collections.Generic;

namespace NoTrafficDespawn
{
	[FileLocation(nameof(NoTrafficDespawn))]
	[SettingsUIGroupOrder(kToggleGroup)]
	[SettingsUIShowGroupName(kToggleGroup)]
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string kToggleGroup = "Toggle";

		public Setting(IMod mod) : base(mod)
		{
			this.trafficDespawnDisabled = false;
		}

		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIMultilineText]
		public string generalInfo => string.Empty;

		[SettingsUISection(kSection, kToggleGroup)]
		public bool trafficDespawnDisabled { get; set; }

		public override void SetDefaults()
		{
			this.trafficDespawnDisabled = false;
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
				{ m_Setting.GetSettingsLocaleID(), "No Vehicle Despawn" },
				{ m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "No Vehicle/Traffic Despawn Settings" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.generalInfo)), "This mod disables the automatic despawn of vehicles when a pathing deadlock is detected in the game. It applies to both cars and public transport vehicles. When this option is enabled, extra care must be taken to manually check for stuck traffic. The effects of disabling/enabling despawning are immediate and can be toggled at any time." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.trafficDespawnDisabled)), "Disable Traffic Despawn" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.trafficDespawnDisabled)), $"Disable vehicles from despawning when they are unable to make progress with pathing. Affects both road traffic and public transport." },


			};
		}

		public void Unload()
		{

		}
	}
}
