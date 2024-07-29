using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using System.Collections.Generic;

namespace NoTrafficDespawn
{
	[FileLocation(nameof(NoTrafficDespawn))]
	[SettingsUIGroupOrder(kToggleGroup, despawnTypeGroup)]
	[SettingsUIShowGroupName(despawnTypeGroup)]
	public class TrafficDespawnSettings : ModSetting
	{
		public const string kSection = "Main";

		public const string kToggleGroup = "Toggle";

		public const string despawnTypeGroup = "DespawnTypes";

		public TrafficDespawnSettings(IMod mod) : base(mod)
		{
			//this.trafficDespawnDisabled = false;
			this.despawnBehavior = DespawnBehavior.Vanilla;
			this.highlightStuckObjects = true;
			this.deadlockLingerFrames = 400;
			this.deadlockSearchDepth = 100;
			this.maxStuckObjectRemovalCount = 1;
			this.maxStuckObjectSpeed = 3;
			this.despawnAll = true;
			this.despawnCommercialVehicles = true;
			this.despawnPedestrians = true;
			this.despawnPersonalVehicles = true;
			this.despawnPublicTransit = true;
			this.despawnServiceVehicles = true;
			this.despawnTaxis = true;
		}

		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIMultilineText]
		public string generalInfo => string.Empty;

		//[SettingsUISection(kSection, kToggleGroup)]
		//public bool trafficDespawnDisabled { get; set; }


		[SettingsUISection(kSection, kToggleGroup)]
		public DespawnBehavior despawnBehavior { get; set; } = DespawnBehavior.Vanilla;

		private bool disableDespawnOptions => this.despawnBehavior == DespawnBehavior.Vanilla;

		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnOptions))]
		public bool highlightStuckObjects { get; set; }


		[SettingsUISlider(min = 0, max = 10000, step = 5, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnOptions))]
		public int deadlockLingerFrames { get; set; }

		[SettingsUISlider(min = 100, max = 10000, step = 5, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnOptions))]
		public int deadlockSearchDepth { get; set; }

		[SettingsUISlider(min = 0, max = 1000, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnOptions))]
		public int maxStuckObjectRemovalCount { get; set; }

		[SettingsUISlider(min = 1, max = 128, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(kSection, kToggleGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnOptions))]
		public int maxStuckObjectSpeed { get; set; }



		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableAllDespawnOption))]
		public bool despawnAll { get; set; }

		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnTypeOptions))]
		public bool despawnCommercialVehicles { get; set; }

		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnTypeOptions))]
		public bool despawnPedestrians { get; set; }

		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnTypeOptions))]
		public bool despawnPersonalVehicles { get; set; }

		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnTypeOptions))]
		public bool despawnPublicTransit { get; set; }

		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnTypeOptions))]
		public bool despawnServiceVehicles { get; set; }

		[SettingsUISection(kSection, despawnTypeGroup)]
		[SettingsUIDisableByCondition(typeof(TrafficDespawnSettings), nameof(disableDespawnTypeOptions))]
		public bool despawnTaxis { get; set; }

		private bool disableDespawnTypeOptions => this.disableDespawnOptions || this.despawnAll || (this.despawnBehavior == DespawnBehavior.NoDespawn);
		private bool disableAllDespawnOption => this.disableDespawnOptions || (this.despawnBehavior == DespawnBehavior.NoDespawn);

		public override void SetDefaults()
		{
			//this.trafficDespawnDisabled = false;
			this.despawnBehavior = DespawnBehavior.Vanilla;
			this.highlightStuckObjects = true;
			this.deadlockLingerFrames = 400;
			this.deadlockSearchDepth = 100;
			this.maxStuckObjectRemovalCount = 1;
			this.maxStuckObjectSpeed = 3;
			this.despawnAll = true;
			this.despawnCommercialVehicles = true;
			this.despawnPedestrians = true;
			this.despawnPersonalVehicles = true;
			this.despawnPublicTransit = true;
			this.despawnServiceVehicles = true;
			this.despawnTaxis = true;
		}
	}

	public enum DespawnBehavior
	{
		Vanilla,
		DespawnDeadlocksOnly,
		DespawnAny,
		NoDespawn
	}

	public class LocaleEN : IDictionarySource
	{
		private readonly TrafficDespawnSettings m_Setting;
		public LocaleEN(TrafficDespawnSettings setting)
		{
			m_Setting = setting;
		}
		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ m_Setting.GetSettingsLocaleID(), "No Vehicle Despawn" },
				{ m_Setting.GetOptionTabLocaleID(TrafficDespawnSettings.kSection), "Main" },

				{ m_Setting.GetOptionGroupLocaleID(TrafficDespawnSettings.kToggleGroup), "No Vehicle/Traffic Despawn Settings" },
				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.generalInfo)), 
					"This mod configures the automatic despawn of vehicles the game has detected as 'stuck'. The default non-modded behavior is to check if a chain of cars planning" +
					" to utilize the same pathing node either results in a circular chain without at least one vehicle moving faster than 6m/s, or a chain exceeding 100 vehicles with none" +
					" exceeding 6m/s. The recommended usage of this mod disables the vehicle chain length check, and set a deadlock removal frame count." +
					" It applies to all objects (cars, cims, transit); it is not recommended to disable despawning completely." },

				//{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.trafficDespawnDisabled)), "Disable Traffic Despawn" },
				//{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.trafficDespawnDisabled)), $"Disable vehicles from despawning when they are unable to make progress with pathing. Affects both road traffic and public transport." },

				//{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.trafficDespawnDisabled)), "Mod Active" },
				//{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.trafficDespawnDisabled)), $"Enable/disable this mod; default is disabled." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnBehavior)), "Vehicle Despawn Method" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnBehavior)), $"The general logic to use to determine whether an object should be considered stuck. 'Vanilla' deactivates this mod and reverts to the non-modded behavior." },

				{ m_Setting.GetEnumValueLocaleID(DespawnBehavior.Vanilla), "Vanilla (default)" },
				{ m_Setting.GetEnumValueLocaleID(DespawnBehavior.DespawnDeadlocksOnly), "Despawn Deadlocks Only (Recommended)" },
				{ m_Setting.GetEnumValueLocaleID(DespawnBehavior.DespawnAny), "Despawn Deadlocks or Long Chains" },
				{ m_Setting.GetEnumValueLocaleID(DespawnBehavior.NoDespawn), "Disable Vehicle Despawn (NOT recommended)" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.highlightStuckObjects)), "Highlight Stuck Objects" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.highlightStuckObjects)), $"Highlight objects that are detected as stuck." },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.deadlockLingerFrames)), "Deadlocked Vehicle Removal Wait Frames" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.deadlockLingerFrames)), $"The number of frames the simulation will wait before removing vehicles it detects to be stuck. Increasing this number may allow the simulation to recover on its own, but the vanilla default is 0." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.deadlockSearchDepth)), "Vehicle Chain Search Depth" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.deadlockSearchDepth)), $"The number of vehicles the simulation will traverse to find a circular loop. Reaching the limit with 'desawpn explicit deadlocks only' set will do nothing, otherwise the vehicle will be marked as stuck." },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.maxStuckObjectRemovalCount)), "Max Per-frame Stuck Vehicle Removal Count" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.maxStuckObjectRemovalCount)), $"The maximum number of stuck vehicles the game will move per frame. Decreasing this value increases the chance the simulation will recover from deadlocks on its own with fewer vehicles removed." },
				
				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.maxStuckObjectSpeed)), "Max Stuck Vehicle Speed" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.maxStuckObjectSpeed)), $"The maximum speed at which the game will consider an object in a blocking path to be fast enough to not cause upstream traffic to be stuck." },
			
				//Despawn Typesa
				{m_Setting.GetOptionGroupLocaleID(TrafficDespawnSettings.despawnTypeGroup), "Despawn Object Types" },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnAll)), "All" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnAll)), $"Despawn stuck objects of any type." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnCommercialVehicles)), "Commercial Vehicles" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnCommercialVehicles)), $"Despawn stuck commercial vehicles such as delivery trucks." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnPedestrians)), "Pedestrians" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnPedestrians)), $"Despawn stuck pedestrians." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnPersonalVehicles)), "Personal Vehicles" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnPersonalVehicles)), $"Despawn stuck personal vehicles." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnServiceVehicles)), "Service / Misc Vehicles" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnServiceVehicles)), $"Despawn stuck service vehicles such ambulances, garbage trucks, etc and other vehicles not covered by other categories." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnTaxis)), "Taxis" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnTaxis)), $"Despawn stuck taxis." },

				{ m_Setting.GetOptionLabelLocaleID(nameof(TrafficDespawnSettings.despawnPublicTransit)), "Public Transit" },
				{ m_Setting.GetOptionDescLocaleID(nameof(TrafficDespawnSettings.despawnPublicTransit)), $"Despawn stuck public transit vehicles (buses, trams, trains, etc)." },

			};
		}

		public void Unload()
		{

		}
	}

	public class SettingsStruct
	{

	}
}
