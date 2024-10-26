using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using System.Collections.Generic;

namespace EmploymentTracker
{
	[FileLocation(nameof(EmploymentTracker))]
	[SettingsUIGroupOrder(routeHighlightTypes, infoPanelOptions, objectHighlightTypes, routeHighlightTypes, routeHighlightOptions)]
	[SettingsUIShowGroupName(routeHighlightTypes, infoPanelOptions, objectHighlightTypes, routeHighlightOptions)]
	public class EmploymentTrackerSettings : ModSetting
	{
		public const string kSection = "Main";

		public const string routeHighlightOptions = "Route Highlight Options";
		public const string routeHighlightTypes = "Route Highlight Types";
		public const string infoPanelOptions = "Selected Object Info Panel Options";
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
			this.routeOpacityMultilier = .7f;
			this.threadBatchSize = 16;

			this.incomingRoutes = true;
			this.incomingRoutesTransit = true;
			this.highlightSelectedTransitVehiclePassengerRoutes = true;
			this.highlightSelected = true;
			this.showCountsOnInfoPanel = true;
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

		[SettingsUISlider(min = .5f, max = 20f, step = .5f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public float pedestrianRouteWidth { get; set; }

		[SettingsUISlider(min = .1f, max = 10f, step = .1f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public float routeOpacity { get; set; }
		[SettingsUISlider(min = .01f, max = 1, step = .01f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public float routeOpacityMultilier { get; set; }

		[SettingsUISlider(min = 1, max = 1024, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
		[SettingsUISection(kSection, routeHighlightOptions)]
		public int threadBatchSize { get; set; }


		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool highlightSelected { get; set; }
		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool incomingRoutes { get; set; }
		[SettingsUISection(kSection, routeHighlightTypes)]
		[SettingsUIDisableByCondition(typeof(EmploymentTrackerSettings), nameof(hideSubIncomingOption))]
		public bool incomingRoutesTransit { get; set; }
		[SettingsUISection(kSection, routeHighlightTypes)]
		public bool highlightSelectedTransitVehiclePassengerRoutes { get; set; }
		[SettingsUISection(kSection, infoPanelOptions)]
		public bool showCountsOnInfoPanel { get; set; }

		private bool hideSubIncomingOption => !this.incomingRoutes;

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
			this.routeOpacityMultilier = .1f;
			this.threadBatchSize = 16;

			this.incomingRoutes = true;
			this.incomingRoutesTransit = true;
			this.highlightSelectedTransitVehiclePassengerRoutes = true;
			this.highlightSelected = true;
			this.showCountsOnInfoPanel = true;
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

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.routeOpacityMultilier)), "Route Opacity Overlap Multiplier" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.routeOpacityMultilier)), $"Multiplier increasing brightness of segments with multiple commuters." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), "Vehicle Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), $"The width of the route display line for vehicles." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), "Pedestrian Route Segment Line Width" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), $"The width of the route display line for pedestrians." },
				
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.threadBatchSize)), "Thread Batch Size" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.threadBatchSize)), $"Advanced: the number of entities per highlight batch." },

				//Info Panel Options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.infoPanelOptions), "Selected Object Info Panel Options" },
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.showCountsOnInfoPanel)), "Show Incoming Cim Count" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.showCountsOnInfoPanel)), $"Display the number of cims en-route to or passing through the selected object on the info panel." },
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
				{ this.settings.GetSettingsLocaleID(), "路线标记器（Route Highlighter)" },
				{ this.settings.GetOptionTabLocaleID(EmploymentTrackerSettings.kSection), "Main" },


				//Feature toggles
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightTypes), "路线显示选项 (测试中)" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightSelected)), "所选对象路线" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightSelected)), $"显示所选人或非公共交通工具的当前路线。" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightSelectedTransitVehiclePassengerRoutes)), "所选交通所有乘客路线" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightSelectedTransitVehiclePassengerRoutes)), $"Display current routes of all CIMs in the selected vehicle." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.incomingRoutes)), "正在前往所选建筑路线" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.incomingRoutes)), $"Display routes of all objects en-route to the selected building. Has a non-trivial performance impact." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.incomingRoutesTransit)), "包括公共交通" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.incomingRoutesTransit)), $"Include public transit when incoming highlighting routes. This feature is an experimental work-in-progress." },
				
				//Object highlight options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.objectHighlightTypes), "目标显示选项  " },
				 
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), "乘客目的地" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightDestinations)), $"显示所选交通工具（私人交通工具或者公共交通）所有乘客的目的地。"},

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), "居民工作地" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightWorkplaces)), $"Highlight the workplace buildings for every resident of the selected residential building." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), "员工住址" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeResidences)), $"显示所选建筑所有员工的住址。" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), "所选建筑员工" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightEmployeeCommuters)), $"Highlight citizens and vehicles in-transit to the selected workplace building." },
			
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), "学生住址" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.highlightStudentResidences)), $"显示所选学校所有学生的住址。" },


				//Route highlight options 
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.routeHighlightOptions), "路线显示选项  " },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), "叠加线透明度" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.routeOpacity)), $"路线叠加线的透明度。" },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.routeOpacityMultilier)), "叠加线透明度系数" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.routeOpacityMultilier)), $"Multiplier increasing brightness of segments with multiple commuters." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), "车辆路线部分叠加线宽度" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.vehicleRouteWidth)), $"The width of the route display line for vehicles." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), "行人路线部分叠加线宽度" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.pedestrianRouteWidth)), $"The width of the route display line for pedestrians." },

				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.threadBatchSize)), "Thread Batch Size" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.threadBatchSize)), $"Advanced: the number of entities per highlight batch." },
				
				//Info Panel Options
				{ this.settings.GetOptionGroupLocaleID(EmploymentTrackerSettings.infoPanelOptions), "Selected Object Info Panel Options" },
				{ this.settings.GetOptionLabelLocaleID(nameof(EmploymentTrackerSettings.showCountsOnInfoPanel)), "Show Incoming Cim Count" },
				{ this.settings.GetOptionDescLocaleID(nameof(EmploymentTrackerSettings.showCountsOnInfoPanel)), $"Display the number of cims en-route to or passing through the selected object on the info panel." },
				
				{"EmploymentTracker_" + "Selected Object Route", "所选对象路线" },
				{"EmploymentTracker_" + "On", "开" },
				{"EmploymentTracker_" + "Off", "关" },
				{"EmploymentTracker_" + "Quick-toggle", "快速切换" },
				{"EmploymentTracker_" + "All (shift+e)", "全部（shift+e）" },
				{"EmploymentTracker_" + "Routes (shift+v)", "路线（shift+v）" },
				{"EmploymentTracker_" + "Buildings (shift+b)", "建筑物（shift+b）" },
				{"EmploymentTracker_" + "Road Segment Tool (shift+r)", "路段工具（shift+r）" },
				{"EmploymentTracker_" + "Students' Residences", "学生住址" },
				{"EmploymentTracker_" + "Residents' Workplaces", "居民工作地" },
				{"EmploymentTracker_" + "Employee Residences", "员工住址" },
				{"EmploymentTracker_" + "Passenger Destinations", "乘客目的地" },
				{"EmploymentTracker_" + "Transit Passenger Routes", "公共交通乘客路线" },
				{"EmploymentTracker_" + "Incoming Routes (Transit)", "正在前往路线（公共交通）" },
				{"EmploymentTracker_" + "Incoming Routes", "正在前往路线" },
				{"EmploymentTracker_" + "Highlight Routes", "高亮显示路线" },
				{"EmploymentTracker_" + "Route Highlighter", "路线显示器" },
				{"EmploymentTracker_" + "Highlight Buildings", "高亮显示建筑物" },  
				
				{"EmploymentTracker_" + "Route Highlighter Road Segment Tool (shift+r)", "路线显示器路段工具（shift+r）" },   
				{"EmploymentTracker_" + "Route Highlighter Settings", "路线显示器设置" },   
				{"EmploymentTracker_" + "Lane Selection", "车道选择"},   
			};
		}

		public void Unload()
		{

		}
	}
}
