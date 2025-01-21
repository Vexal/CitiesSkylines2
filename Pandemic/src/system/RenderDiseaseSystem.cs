using Game;
using Game.Citizens;
using Game.Common;
using Game.Objects;
using Game.Rendering;
using Game.SceneFlow;
using Game.Tools;
using Unity.Entities;
using Unity.Jobs;

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
				ComponentType.ReadOnly<Citizen>(),
				ComponentType.ReadOnly<CurrentTransport>(),
				ComponentType.ReadOnly<CurrentDisease>(),
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
			if (GameManager.instance.gameMode != GameMode.Game || !Mod.settings.modEnabled)
			{
				return;
			}

			if (Mod.settings.showContagiousCircle)
			{
				this.renderDiseaseEffect();
			}
		}

		private void renderDiseaseEffect()
		{
			ComputeDiseaseSpreadParametersJob spreadParametersJob = this.pandemicSpreadSystem.initDiseaseSpreadParamsJob(this.diseaseCitizenHumanEntityQuery);

			JobHandle spreadJobHandle = spreadParametersJob.ScheduleParallel(this.diseaseCitizenHumanEntityQuery, default);
			RenderDiseaseJob job = new RenderDiseaseJob();
			job.positions = spreadParametersJob.diseasePositions;
			job.radius = spreadParametersJob.diseaseRadiusSq;
			job.overlayBuffer = this.overlayRenderSystem.GetBuffer(out JobHandle dependencies);
			job.count = spreadParametersJob.rc;
			job.opacity = Mod.settings.contagiousGraphicOpacity;
			JobHandle dependentHandles = JobHandle.CombineDependencies(spreadJobHandle, dependencies);
			var renderJobHandle = job.Schedule(dependentHandles);
			this.overlayRenderSystem.AddBufferWriter(renderJobHandle);
			spreadParametersJob.cleanup(renderJobHandle);
		}
	}
}
