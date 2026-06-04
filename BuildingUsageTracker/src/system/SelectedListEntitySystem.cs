using Colossal.UI.Binding;
using Game.Rendering;
using Game.Tools;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace BuildingUsageTracker
{
	internal partial class SelectedListEntitySystem : UISystemBase
	{
		protected CameraUpdateSystem cameraUpdateSystem;
		protected ToolSystem toolSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			AddBinding(new TriggerBinding<string>("BuildingUsageTracker", "focusEnrouteEntity", s => { this.focusEntity(Utils.entity(s)); }));
			AddBinding(new TriggerBinding<string>("BuildingUsageTracker", "selectEnrouteEntity", s => { this.toolSystem.selected = Utils.entity(s); }));
		}


		protected void focusEntity(Entity entity)
		{
			if (entity != Entity.Null && cameraUpdateSystem.orbitCameraController != null && entity != cameraUpdateSystem.orbitCameraController.followedEntity)
			{
				cameraUpdateSystem.orbitCameraController.followedEntity = entity;
				cameraUpdateSystem.orbitCameraController.TryMatchPosition(cameraUpdateSystem.activeCameraController);
				cameraUpdateSystem.activeCameraController = cameraUpdateSystem.orbitCameraController;
			}

			if (entity == Entity.Null && cameraUpdateSystem.activeCameraController == cameraUpdateSystem.orbitCameraController)
			{
				cameraUpdateSystem.gamePlayController.TryMatchPosition(cameraUpdateSystem.orbitCameraController);
				cameraUpdateSystem.activeCameraController = cameraUpdateSystem.gamePlayController;
			}
		}

	}
}
