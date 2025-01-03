using Colossal;
using Colossal.Entities;
using Game;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
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
		private PandemicSpreadSystem pandemicSpreadSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
			this.pandemicSpreadSystem = World.GetOrCreateSystemManaged<PandemicSpreadSystem>();

			this.diseaseCitizenHumanEntityQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Contagious>(),
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentTransport>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Unspawned>()
				}
			});
		}
		protected override void OnUpdate()
		{
			this.renderDiseaseEffect();
		}

		private void renderDiseaseEffect()
		{
			/*NativeArray<CurrentTransport> diseasedTransports = this.diseaseCitizenHumanEntityQuery.ToComponentDataArray<CurrentTransport>(Allocator.Temp);
			NativeList<float3> positions = new NativeList<float3>(Allocator.TempJob);

			foreach (CurrentTransport t in diseasedTransports)
			{
				if (!EntityManager.HasComponent<CurrentVehicle>(t.m_CurrentTransport) &&
					EntityManager.TryGetComponent<Transform>(t.m_CurrentTransport, out var transform))
				{
					positions.Add(transform.m_Position);
				}
			}*/

			ComputeDiseaseSpreadParametersJob spreadParametersJob = this.pandemicSpreadSystem.initDiseaseSpreadParamsJob(this.diseaseCitizenHumanEntityQuery);

			JobHandle spreadJobHandle = spreadParametersJob.ScheduleParallel(this.diseaseCitizenHumanEntityQuery, default);
			RenderDiseaseJob job = new RenderDiseaseJob();
			job.positions = spreadParametersJob.diseasePositions;
			job.radius = spreadParametersJob.diseaseRadiusSq;
			job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
			job.count = spreadParametersJob.rc;
			JobHandle dependentHandles = JobHandle.CombineDependencies(spreadJobHandle, dependencies);
			var renderJobHandle = job.Schedule(dependentHandles);
			this.overlayRenderSystem.AddBufferWriter(renderJobHandle);
			spreadParametersJob.diseasePositions.Dispose(renderJobHandle);
			spreadParametersJob.diseaseRadiusSq.Dispose(renderJobHandle);
		}
	}
}
