using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using System.Collections.Generic;

namespace EmploymentTracker
{
	[FileLocation(nameof(EmploymentTracker))]
	[SettingsUIGroupOrder(routeHighlightTypes, objectHighlightTypes, routeHighlightTypes, routeHighlightOptions)]
	[SettingsUIShowGroupName(routeHighlightTypes, objectHighlightTypes, routeHighlightOptions)]
	public class EmploymentTrackerSettings : ModSetting
	{
		public const string kSection = "Main";

		public const string routeHighlightOptions = "Route Highlight Options";
		public const string routeHighlightTypes = "Route Highlight Types";
		public const string objectHighlightTypes = "Object Highlight Types";

		public EmploymentTrackerSettings(IMod mod) : base(mod)
		{
			this.highlightDestinations = true;
			this.highlightWorkplaces = true;
			this.highlightStudentResidences = true;
			this.highlightEmployeeCommuters = true;
			this.highlightEmployeeResidences = true;
			//this.highlightRoutes = true;
			this.pedestrianRouteWidth = 2f;
			this.vehicleRouteWidth = 4f;
			this.routeOpacity = .7f;

			this.incomingRoutes = true;
			this.incomingRoutesTransit = true;
			this.highlightSelectedTransitVehiclePassengerRoutes = true;
			this.highlightSelected = true;
		}

		//[SettingsUISection(kSection, objectHighlightTypes)]
		//public bool highlightRoutes { get; set; }

		[SettingsUISection(kSection, objectHighlightTypes)]
		public bool highlightWorkplaces { get; set; }
		
		[SettingsUISection(kSection, objectHighlightTypes)]
		public bool highlightEmployeeResidences { get; set; }
		
		[SettingsUISection(kSection, objectHighlightTypes)]
		public bool highlightEmployeeCommuters { get; set; }
		
		[SettingsUISection(kSection, objectHighlightTypes)]
		public bool highlightStudentResidences { get; set; }
		
		[SettingsUISection(kSection, objectHighlightTypes)]
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


		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool highlightSelected { get; set; }
		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool incomingRoutes { get; set; }
		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool incomingRoutesTransit { get; set; }
		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool highlightSelectedTransitVehiclePassengerRoutes { get; set; }

		public override void SetDefaults()
		{
			this.highlightDestinations = true;
			this.highlightWorkplaces = true;
			//this.highlightRoutes = true;
			this.highlightStudentResidences = true;
			this.highlightEmployeeCommuters = true;
			this.highlightEmployeeResidences = true;
			this.pedestrianRouteWidth = 2f;
			this.vehicleRouteWidth = 4f;
			this.routeOpacity = .7f;

			this.incomingRoutes = true;
			this.incomingRoutesTransit = true;
			this.highlightSelectedTransitVehiclePassengerRoutes = true;
			this.highlightSelected = true;
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
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightTypes), "Route Highlight Toggles (Experimental)" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightSelected)), "Selected Object Route" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightSelected)), $"Display current route of selected CIM or non-public transit vehicles." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightSelectedTransitVehiclePassengerRoutes)), "Selected Vehicle Passenger Routes" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightSelectedTransitVehiclePassengerRoutes)), $"Display current routes of all CIMs in the selected vehicle." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.incomingRoutes)), "Active Routes to Selected Building" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.incomingRoutes)), $"Display routes of all objects en-route to the selected building. Has a non-trivial performance impact." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.incomingRoutesTransit)), "Include En-route Public Transit" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.incomingRoutesTransit)), $"Include public transit when highlighting incoming routes. This feature is an experimental work-in-progress." },
				
				//Object highlight options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.objectHighlightTypes), "Object Highlight Toggles" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), "Passenger Destinations" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), $"Highlight the destinations of passengers in the selected vehicle (personal vehicles or transit)." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), "Building Residents' Workplaces" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), $"Highlight the workplace buildings for every resident of the selected residential building." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), "Employee Residences" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), $"Highlight residence buildings for all employees of the selected building." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), "Employee Commuters" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), $"Highlight citizens and vehicles in-transit to the selected workplace building." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), "Student Residences" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), $"Highlight residence buildings for all students of the selected school." },


				//Route highlight options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightOptions), "Route Highlight Options" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), "Route Opacity" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), $"Opacity of route overlay lines." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), "Vehicle Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), $"The width of the route display line for vehicles." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), "Pedestrian Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), $"The width of the route display line for pedestrians." },

				{"Selected Object Route", "Selected Object Route" },
			};
		}

		public void Unload()
		{

		}
	}

	public class LocaleSC : IDictionarySource
	{
		private readonly EmploymentTrackerSettings settings;
		public LocaleSC(EmploymentTrackerSettings setting)
		{
			this.settings = setting;
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ this.settings.GetSettingsLocaleID(), "路径标记器 （Route Highlighter)" },
				{ this.settings.GetOptionTabLocaleID(EmploymentTrackerSettings.kSection), "Main" },


				//Feature toggles
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightTypes), "路径显示选项 (测试中)" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightSelected)), "所选对象路径" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightSelected)), $"显示所选人或非公共交通工具的当前路径。" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightSelectedTransitVehiclePassengerRoutes)), "所选交通所有乘客路径" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightSelectedTransitVehiclePassengerRoutes)), $"Display current routes of all CIMs in the selected vehicle." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.incomingRoutes)), "正在前往所选建筑路径" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.incomingRoutes)), $"Display routes of all objects en-route to the selected building. Has a non-trivial performance impact." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.incomingRoutesTransit)), "包括公共交通" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.incomingRoutesTransit)), $"Include public transit when incoming highlighting routes. This feature is an experimental work-in-progress." },
				
				//Object highlight options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.objectHighlightTypes), "Object Highlight Toggles" },
				 
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), "乘客目的地" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), $"显示所选交通工具 （私人交通工具或者公共交通）所有乘客的目的地。" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), "Building Residents' Workplaces" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), $"Highlight the workplace buildings for every resident of the selected residential building." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), "员工住址" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), $"显示所选建筑所有员工的住址。" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), "Employee Commuters" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), $"Highlight citizens and vehicles in-transit to the selected workplace building." },
			
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), "学生住址" },
				
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), $"显示所选学校所有学生的住址。" },


				//Route highlight options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightOptions), "Route Highlight Options" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), "Route Opacity" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), $"Opacity of route overlay lines." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), "Vehicle Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), $"The width of the route display line for vehicles." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), "Pedestrian Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), $"The width of the route display line for pedestrians." },

				{"Selected Object Route", "Selected Object Route" },
			};
		}

		public void Unload()
		{

		}
	}
}
