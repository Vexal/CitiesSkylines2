using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using System.Collections.Generic;

namespace EmploymentTracker
{
	[FileLocation(nameof(EmploymentTracker))]
	[SettingsUIGroupOrder(featureOptionsGroup, routeHighlightOptions)]
	[SettingsUIShowGroupName(featureOptionsGroup, routeHighlightOptions)]
	public class EmploymentTrackerSettings : ModSetting
	{
		public const string kSection = "Main";

		public const string routeHighlightOptions = "Route Highlight Options";
		public const string featureOptionsGroup = "Active Highlighters";

		public EmploymentTrackerSettings(IMod mod) : base(mod)
		{
			this.highlightDestinations = true;
			this.highlightWorkplaces = true;
			this.highlightStudentResidences = true;
			this.highlightEmployeeCommuters = true;
			this.highlightEmployeeResidences = true;
			this.highlightRoutes = true;
			this.pedestrianRouteWidth = 2f;
			this.vehicleRouteWidth = 4f;
			this.routeOpacity = .7f;
		}

		[SettingsUISection(kSection, featureOptionsGroup)]
		public bool highlightRoutes { get; set; }

		[SettingsUISection(kSection, featureOptionsGroup)]
		public bool highlightWorkplaces { get; set; }
		
		[SettingsUISection(kSection, featureOptionsGroup)]
		public bool highlightEmployeeResidences { get; set; }
		
		[SettingsUISection(kSection, featureOptionsGroup)]
		public bool highlightEmployeeCommuters { get; set; }
		
		[SettingsUISection(kSection, featureOptionsGroup)]
		public bool highlightStudentResidences { get; set; }
		
		[SettingsUISection(kSection, featureOptionsGroup)]
		public bool highlightDestinations { get; set; }

		[SettingsUISlider(min = .5f, max = 20f, step = .5f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public float vehicleRouteWidth { get; set; }

		[SettingsUISlider(min = .5f, max = 100f, step = .5f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public float pedestrianRouteWidth { get; set; }

		[SettingsUISlider(min = .1f, max = 10f, step = .1f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public float routeOpacity { get; set; }

		public override void SetDefaults()
		{
			this.highlightDestinations = true;
			this.highlightWorkplaces = true;
			this.highlightRoutes = true;
			this.highlightStudentResidences = true;
			this.highlightEmployeeCommuters = true;
			this.highlightEmployeeResidences = true;
			this.pedestrianRouteWidth = 2f;
			this.vehicleRouteWidth = 4f;
			this.routeOpacity = .7f;
		}
	}

	public class LocaleEN : IDictionarySource
	{
		private readonly EmploymentTrackerSettings settings;
		public LocaleEN(EmploymentTrackerSettings setting)
		{
			this.settings = setting;
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ this.settings.GetSettingsLocaleID(), "CIM Route Highlighter" },
				{ this.settings.GetOptionTabLocaleID(EmploymentTrackerSettings.kSection), "Main" },


				//Feature toggles
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.featureOptionsGroup), "Highlight Toggles" },
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightRoutes)), "Routes" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightRoutes)), $"Display current route of selected vehicle or CIM. Routes can change if the object encounters obstacles." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), "Passenger Destinations" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), $"Highlight the destinations of passengers in the selected vehicle (personal vehicles or transit)." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), "Building Residents' Workplaces" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), $"Highlight the workplace buildings for every resident of the selected residential building." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), "Employee Residences" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), $"Highlight residence buildings for all employees of the selected building." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), "Employee Commuters" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), $"Highlight citizens and vehicles in-transit to the selected workplace building." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), "Student Residences" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), $"Highlight residence buildings for all students of the selected building." },

				//Route highlight options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightOptions), "Route Highlight Options" },
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), "Route Opacity" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), $"Opacity of route overlay lines." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), "Vehicle Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), $"The width of the route display line for vehicles." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), "Pedestrian Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), $"The width of the route display line for pedestrians." },
			};
		}

		public void Unload()
		{

		}
	}
}
