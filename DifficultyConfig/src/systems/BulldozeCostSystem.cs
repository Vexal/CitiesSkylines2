using Colossal.Entities;
using Game;
using Game.City;
using Game.Common;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.UI.Localization;
using Game.UI.Tooltip;
using Game.UI.Widgets;
using Unity.Collections;
using Unity.Entities;

namespace DifficultyConfig
{
	internal partial class BulldozeCostSystem : TooltipSystemBase
	{
		private DifficultySettings settings;
		private ToolSystem toolSystem;
		private BulldozeToolSystem bulldozeToolSystem;
		private CitySystem citySystem;

		private StringTooltip costTooltip;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.toolSystem = World.GetExistingSystemManaged<ToolSystem>();
			this.bulldozeToolSystem = World.GetExistingSystemManaged<BulldozeToolSystem>();

			StringTooltip val = new StringTooltip
			{
				icon = "Media/Game/Icons/Money.svg",
				path = "BulldozeCostToolSystem"//,
				//path = PathSegment.op_Implicit("bulldozeCostTool")
			};
			/*((NumberTooltip<int>)val).unit = "money";
			((IconTooltip)val).color = (TooltipColor)2;
			((LabelIconTooltip)val).label = LocalizedString.Id("EconomyFixer.BULLDOZECOST_LABEL");*/
			this.costTooltip = val;
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			this.settings = Mod.INSTANCE.settings();

			
		}

		int frameCount = 0;
		protected override void OnUpdate()
		{
			if (this.toolSystem.activeTool == this.bulldozeToolSystem)
			{
				this.costTooltip.value = "Bulldoze cost: " + this.frameCount++;
				this.AddMouseTooltip(this.costTooltip);
				//this.bulldozeToolSystem.
			}
		}
	}
}
