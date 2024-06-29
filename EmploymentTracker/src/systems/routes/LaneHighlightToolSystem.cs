using Game.Areas;
using Game.Common;
using Game;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Net;
using Unity.Jobs;
using Unity.Entities;
using Colossal.Entities;
using static UnityEngine.GraphicsBuffer;

namespace EmploymentTracker
{
	public partial class LaneHighlightToolSystem : ToolBaseSystem
	{
		public override string toolID => "LaneHighlightTool";
		public Entity selectedLane = default;


		public override PrefabBase GetPrefab()
		{
			return null;
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			return false;
		}

		public override void InitializeRaycast()
		{
			base.InitializeRaycast();
			m_ToolRaycastSystem.typeMask = TypeMask.Net | TypeMask.Lanes;
			m_ToolRaycastSystem.netLayerMask = Layer.All;
			m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground;

			//m_ToolRaycastSystem.raycastFlags |= RaycastFlags.SubElements | RaycastFlags.Decals | RaycastFlags.NoMainElements;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			this.Enabled = false;
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var vvv = base.OnUpdate(inputDeps);
			if (GetRaycastResult(out Entity entity, out RaycastHit hit))
			{
				if (EntityManager.TryGetBuffer<Game.Net.SubLane>(entity, true, out var laneBuffer))
				{
					Mod.log.Info("tst " + entity.ToString());
					for (int i = 0; i < laneBuffer.Length && i < 1; ++i)
					{
						this.selectedLane = laneBuffer[i].m_SubLane;
					}
					this.selectedLane = entity;
				}
			} else
			{
				//this.selectedLane = default;
			}

			return vvv;
		}
	}
}
