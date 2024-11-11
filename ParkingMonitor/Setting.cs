using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace ParkingMonitor
{
	[FileLocation(nameof(ParkingMonitor))]
	[SettingsUIGroupOrder(kButtonGroup)]
	[SettingsUIShowGroupName(kButtonGroup)]
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "Settings";

		public Setting(IMod mod) : base(mod)
		{
			this.SetDefaults();
		}
	

		[SettingsUISlider(min = 1, max = 100, step = 1)]
		[SettingsUISection(kSection, kButtonGroup)]
		public int parkingRowCount { get; set; }

		[SettingsUISection(kSection, kButtonGroup)]
		public InitialValue initialState { get; set; } = InitialValue.ACTIVE;

		public override void SetDefaults()
		{
			this.initialState = InitialValue.ACTIVE;
			this.parkingRowCount = 25;
		}

		public enum InitialValue
		{
			ACTIVE,
			PAUSED,
			STOPPED,
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
				{ m_Setting.GetSettingsLocaleID(), "Parking Monitor" },
				{ m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Settings" },

		
				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.parkingRowCount)), "Max Parking Locations Displayed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.parkingRowCount)), $"The maximum number of parking locations whose counts are displayed in the parking monitor panel." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.initialState)), "Initial Monitor Active State" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.initialState)), $"Whether the parking monitor is active when the game starts. The monitor must be 'active' or 'paused' to record data."},

				{ m_Setting.GetEnumValueLocaleID(Setting.InitialValue.ACTIVE), "Active" },
				{ m_Setting.GetEnumValueLocaleID(Setting.InitialValue.PAUSED), "Paused" },
				{ m_Setting.GetEnumValueLocaleID(Setting.InitialValue.STOPPED), "Stopped" },

			};
		}

		public void Unload()
		{

		}
	}
}
