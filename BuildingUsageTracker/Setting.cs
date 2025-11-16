using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace BuildingUsageTracker
{
	[FileLocation(nameof(BuildingUsageTracker))]
	[SettingsUIGroupOrder(kButtonGroup)]
	[SettingsUIShowGroupName(kButtonGroup)]
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "displayOptions";

		public Setting(IMod mod) : base(mod)
		{
			this.SetDefaults();
		}

		public override void SetDefaults()
		{
			this.showEnrouteCimCounts = true;
			this.showEnrouteVehicleCounts = true;
			this.showBuildingOccupancy = true;
            this.showDetailedBuildingOccupancy = true;
            this.showDetailedEnrouteCimCounts = true;
            this.showDetailedEnrouteVehicleCounts = true;
		}

		[SettingsUISection(kSection, kButtonGroup)]
        public bool showEnrouteCimCounts { get; set; }

		[SettingsUISection(kSection, kButtonGroup)]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(disableDetailedEC))]
        public bool showDetailedEnrouteCimCounts { get; set; }

		[SettingsUISection(kSection, kButtonGroup)]
		public bool showEnrouteVehicleCounts { get; set; }

        [SettingsUISection(kSection, kButtonGroup)]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(disableDetailedVEC))]
        public bool showDetailedEnrouteVehicleCounts { get; set; }

        [SettingsUISection(kSection, kButtonGroup)]
		public bool showBuildingOccupancy { get; set; }

        [SettingsUISection(kSection, kButtonGroup)]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(disableDetailedOC))]
        public bool showDetailedBuildingOccupancy { get; set; }

        private bool disableDetailedEC => !this.showEnrouteCimCounts;
        private bool disableDetailedVEC => !this.showEnrouteVehicleCounts;
        private bool disableDetailedOC => !this.showBuildingOccupancy;
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
				{ m_Setting.GetSettingsLocaleID(), "En-route / Occupancy Counts" },
				{ m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Show Display Options" },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.showEnrouteCimCounts)), "En-route Cim Counts" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.showEnrouteCimCounts)), $"Display counts and details of the number of cims traveling to the selected object" },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.showEnrouteVehicleCounts)), "En-route Vehicle Counts" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.showEnrouteVehicleCounts)), $"Display counts and details of the number of vehicles traveling to the selected object" },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.showBuildingOccupancy)), "Building Occupant Counts" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.showBuildingOccupancy)), $"Display counts and details of the number of cims currently inside the selected building" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.showDetailedEnrouteCimCounts)), "Detailed En-route Cim Counts" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.showDetailedEnrouteCimCounts)), $"Display detailed list of en-route cim counts." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.showDetailedEnrouteVehicleCounts)), "Detailed En-route Vehicle Counts" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.showDetailedEnrouteVehicleCounts)), $"Display detailed list of en-route vehicle counts." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.showDetailedBuildingOccupancy)), "Detailed Building Occupant Counts" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.showDetailedBuildingOccupancy)), $"Display detailed list of building occupant counts." },
            };
		}

		public void Unload()
		{

		}
	}
}
