using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace FireStarter
{
	[FileLocation(nameof(FireStarter))]
	[SettingsUIGroupOrder(kToggleGroup)]
	[SettingsUIShowGroupName(kToggleGroup)]
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string kToggleGroup = "Toggle";

		public Setting(IMod mod) : base(mod)
		{
			this.enabled = false;

		}

		[SettingsUISection(kSection, kToggleGroup)]
		public bool enabled { get; set; }

		public override void SetDefaults()
		{
			this.enabled = false;
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
				{ m_Setting.GetSettingsLocaleID(), "Firestarter Mod" },
				{ m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Firestarter Settings" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.enabled)), "Enabled" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.enabled)), $"Enable/disable starting fires when clicking objects." },

				
			};
		}

		public void Unload()
		{

		}
	}
}
