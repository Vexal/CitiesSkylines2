using Colossal.Entities;
using Game;
using Game.Citizens;
using Game.Common;
using Game.Objects;
using Game.Rendering;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pandemic
{
	internal partial class RenderDiseaseSystem : GameSystemBase
	{
		private EntityQuery diseaseCitizenHumanEntityQuery;
		private OverlayRenderSystem overlayRenderSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();

			this.diseaseCitizenHumanEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentTransport>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Unspawned>(),
				}
			});
		}
		protected override void OnUpdate()
		{
			this.renderDiseaseEffect();
			
		}

		private void renderDiseaseEffect()
		{
			NativeArray<CurrentTransport> diseasedTransports = this.diseaseCitizenHumanEntityQuery.ToComponentDataArray<CurrentTransport>(Allocator.Temp);
			NativeList<float3> positions = new NativeList<float3>(Allocator.TempJob);

			foreach (CurrentTransport t in diseasedTransports)
			{
				if (EntityManager.TryGetComponent<Transform>(t.m_CurrentTransport, out var transform))
				{
					positions.Add(transform.m_Position);
				}
			}

			if (positions.Length > 0)
			{
				RenderDiseaseJob job = new RenderDiseaseJob();
				job.positions = positions.AsArray();
				job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
				job.radius = Mod.INSTANCE.m_Setting.diseaseSpreadRadius;
				var renderJobHandle = job.Schedule(dependencies);
				this.overlayRenderSystem.AddBufferWriter(renderJobHandle);
				positions.Dispose(renderJobHandle);
			}
			else
			{
				positions.Dispose();
			}
		}
	}
}
