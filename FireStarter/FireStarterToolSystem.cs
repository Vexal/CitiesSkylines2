using Game.Prefabs;
using Game.Tools;
using Game.UI.Tooltip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireStarter
{
	public partial class FireStarterToolSystem : ToolBaseSystem
	{
		public override string toolID => "Fire Starter";

		public override PrefabBase GetPrefab()
		{
			return null;
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			if (this.m_ToolSystem.activeTool != this)
			{
				return false;
			}

			//m_ToolSystem.

			return true;
		}


	}
}
