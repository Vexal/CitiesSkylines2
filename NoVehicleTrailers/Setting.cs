using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using System.Collections.Generic;

namespace NoVehicleTrailers
{
	[FileLocation(nameof(NoVehicleTrailers))]
	[SettingsUIGroupOrder(kButtonGroup)]
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "Button";
		internal NoVehicleTrailersSystem noVehicleTrailersSystem;

		public Setting(IMod mod) : base(mod)
		{
			this.disableCarTrailers = false;
		}

		[SettingsUISection(kSection, kButtonGroup)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(disableOption))]
		public bool disableCarTrailers { get; set; }

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(kSection, kButtonGroup)]
		public bool deleteCarTrailersButton { set { this.noVehicleTrailersSystem.deletePersonalTrailers(); } }

		public override void SetDefaults()
		{
			this.disableCarTrailers = false;
		}

		private bool disableOption => true;
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
				{ m_Setting.GetSettingsLocaleID(), "No Vehicle Trailers" },
				{ m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.disableCarTrailers)), "Disable Car Trailers" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.disableCarTrailers)), $"Prevents the spawning of new car trailers for vehicles. May have gameplay implications for large families." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(Setting.deleteCarTrailersButton)), "Delete Existing Car Trailers" },
				{ m_Setting.GetOptionDescLocaleID(nameof(Setting.deleteCarTrailersButton)), $"Delete existing personal car trailers on the map." },
				{ m_Setting.GetOptionWarningLocaleID(nameof(Setting.deleteCarTrailersButton)), "Delete all existing personal car trailers on the map?" },

				

			};
		}

		public void Unload()
		{

		}
	}
}
