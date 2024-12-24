using Game;
using Game.Citizens;
using Game.Common;
using Game.Rendering;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Pandemic
{
	internal partial class PandemicSystem : GameSystemBase
	{
		private EntityQuery diseaseEntityQuery;
		private OverlayRenderSystem overlayRenderSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();

			this.diseaseEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
				ComponentType.ReadWrite<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});
		}
		protected override void OnUpdate()
		{
			NativeArray<Entity> diseasedEntities = this.diseaseEntityQuery.ToEntityArray(Allocator.Temp);
		}
	}
}
