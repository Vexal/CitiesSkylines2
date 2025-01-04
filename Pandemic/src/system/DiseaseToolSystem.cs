using Colossal.Entities;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Pandemic
{
	internal partial class DiseaseToolSystem : UISystemBase
	{
		ToolSystem toolSystem;
		Entity selectedEntity;
		private HashSet<Entity> nextDiseaseTargets = new HashSet<Entity>();

		protected override void OnCreate()   
		{
			base.OnCreate();
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			Mod.INSTANCE.m_Setting.diseaseToolSystem = this;

			Mod.impartDiseaseAction.onInteraction += (_, phase) =>
			{
				if (GameManager.instance.gameMode == Game.GameMode.Game)
				{
					this.selectedEntity = this.getSelected();
					this.nextDiseaseTargets.Add(this.selectedEntity);
				}
			};
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			/*if (this.nextDiseaseTargets.Count > 0)
			{
				foreach (Entity entity in this.nextDiseaseTargets)
				{
					if (this.tryGetCitizenEntity(entity, out var citizen))
					{
						Mod.log.Info("applying disease to " + citizen.ToString());
						EntityManager.AddComponent<Cu>(citizen);
					}
				}

				this.reset();
			}*/
		}

		private void reset()
		{
			this.nextDiseaseTargets.Clear();
		}
		
		private Entity getSelected()
		{
			return toolSystem.selected;
		}

		private bool tryGetCitizenEntity(Entity target, out Entity citizen)
		{
			if (EntityManager.TryGetComponent<Game.Creatures.Resident>(target, out var resident) && EntityManager.Exists(resident.m_Citizen))
			{
				citizen = resident.m_Citizen;
				return true;
			}
			else
			{
				citizen = Entity.Null;
				return false;
			}
		}
	}
}
