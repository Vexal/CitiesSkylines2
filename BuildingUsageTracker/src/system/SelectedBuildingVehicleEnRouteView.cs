using Colossal;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Buildings;
using Game.Common;
using Game.Pathfind;
using Game.Tools;
using Game.Vehicles;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	partial class SelectedBuildingVehicleEnRouteView : SelectedBuildingInfoSection
	{
        public static readonly string EXPAND_DETAILS_NAME = "enrouteVehicleView";
        private EntityQuery enrouteVehicleQuery;
		private ValueBinding<string> enrouteCountBinding;
		private Counters counters = new Counters { json = "{}" };

		protected override void OnCreate()
		{
            OnCreate(EXPAND_DETAILS_NAME, Mod.SETTINGS.showDetailedEnrouteVehicleCounts);
			this.enrouteCountBinding = new ValueBinding<string>(MOD_NAME, "enrouteVehicleCountBinding", counters.json);
			this.otherView = World.GetOrCreateSystemManaged<SelectedBuildingEnRouteView>();
			AddBinding(this.enrouteCountBinding);

			this.enrouteVehicleQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Vehicle>(),
				ComponentType.ReadOnly<Target>(),
			},
				Any = new ComponentType[]
			{

			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>()
				}
			});

			AddBinding(new TriggerBinding<bool>("BuildingUsageTracker", "toggleShowEnrouteVehicleEntityList", s => {this.toggleEntities(s); }));
			AddBinding(new TriggerBinding<string>("BuildingUsageTracker", "selectEnrouteVehicleEntity", s => { this.toolSystem.selected = Utils.entity(s) ; }));

            Mod.SETTINGS.onSettingsApplied += s => { if (s is Setting setting) this.showDetails.Update(setting.showDetailedEnrouteVehicleCounts); };
        }

		protected override void update(Entity selectedEntity)
		{
			this.counters.init(this.showEntities);
			EnRouteVehicleCountJob job = new EnRouteVehicleCountJob();
			job.searchTarget = selectedEntity;
			if (EntityManager.TryGetBuffer<Renter>(selectedEntity, true, out var renterBuffer) && renterBuffer.Length > 0)
			{
				job.searchTarget2 = renterBuffer[0].m_Renter;
				job.hasTarget2 = true;
			}

			bool isParkingStructure = EntityManager.isParkingStructure(selectedEntity);
			if (isParkingStructure)
			{
				job.pathTargets = new NativeHashSet<Entity>(5, Allocator.TempJob);

				this.addSubObjectsConnectedRoutes(ref job.pathTargets, selectedEntity);
				this.addConnectedRoutes(ref job.pathTargets, selectedEntity);
				this.addParkingSpots(ref job.pathTargets, selectedEntity);

				if (job.pathTargets.Count > 0)
				{
					job.checkPathElements = true;
					job.pathHandle = SystemAPI.GetBufferTypeHandle<PathElement>(true);
					job.pathOwnerHandle = SystemAPI.GetComponentTypeHandle<PathOwner>(true);
				}
			}

			if (this.showEntities)
			{
				job.entityHandle = SystemAPI.GetEntityTypeHandle();
				job.returnEntities = true;
			}

			this.counters.setJobCounters(ref job);
			job.targetHandle = SystemAPI.GetComponentTypeHandle<Target>(true);

			var jobHandle = this.showEntities ? JobChunkExtensions.Schedule(job, this.enrouteVehicleQuery, default) :
				JobChunkExtensions.ScheduleParallel(job, this.enrouteVehicleQuery, default);

			jobHandle.Complete();
			this.counters.disposeAndBuild();
			this.enrouteCountBinding.Update(this.counters.json);
		}

		private struct Counters
		{
			public NativeCounter totalCount;
			public NativeCounter serviceCount;
			public NativeCounter deliveryCount;
			public NativeCounter personalCarCount;
			public NativeCounter taxiCount;
			public NativeCounter otherCount;
			public NativeList<Entity> entities;
			public string json;

			public void init(bool returnEntities)
			{
				this.totalCount = new NativeCounter(Allocator.TempJob);
				this.serviceCount = new NativeCounter(Allocator.TempJob);
				this.deliveryCount = new NativeCounter(Allocator.TempJob);
				this.personalCarCount = new NativeCounter(Allocator.TempJob);
				this.taxiCount = new NativeCounter(Allocator.TempJob);
				this.otherCount = new NativeCounter(Allocator.TempJob);

				if (returnEntities)
				{
					this.entities = new NativeList<Entity>(Allocator.TempJob);
				}
				else
				{
					this.entities = default;
				}
			}

			public void setJobCounters(ref EnRouteVehicleCountJob job)
			{
				job.totalCount = this.totalCount.ToConcurrent();
				job.serviceCount = this.serviceCount.ToConcurrent();
				job.deliveryCount = this.deliveryCount.ToConcurrent();
				job.personalCarCount = this.personalCarCount.ToConcurrent();
				job.taxiCount = this.taxiCount.ToConcurrent();
				job.otherCount = this.otherCount.ToConcurrent();

				if (this.entities.IsCreated)
				{
					job.resultEntities = this.entities;
				}
			}

			public void disposeAndBuild()
			{
				this.json = "{\"totalCount\":" + this.totalCount.Count +
				Utils.jsonFieldC("serviceCount", this.serviceCount) +
				Utils.jsonFieldC("deliveryCount", this.deliveryCount) +
				Utils.jsonFieldC("personalCarCount", this.personalCarCount) +
				Utils.jsonFieldC("taxiCount", this.taxiCount) +
				Utils.jsonFieldC("other", this.otherCount);

				if (this.entities.IsCreated)
				{
					this.json += Utils.jsonArray("entities", this.entities);
					this.entities.Dispose();
				}

				this.json += "}";

				this.totalCount.Dispose();
				this.serviceCount.Dispose();
				this.deliveryCount.Dispose();
				this.taxiCount.Dispose();
				this.personalCarCount.Dispose();
				this.otherCount.Dispose();
			}
		}

		protected override void selectionChanged()
		{
			this.showEntities = false;
			this.counters.json = "{}";
			this.enrouteCountBinding.Update("{}");
		}

		protected override string group => this.counters.json;

		protected override bool shouldBeVisible(Entity selectedEntity)
		{
			return Mod.SETTINGS.showEnrouteVehicleCounts && base.shouldBeVisible(selectedEntity);
		}

        protected override void updateExpandDetailsSetting(bool expand)
        {
            Mod.SETTINGS.showDetailedEnrouteVehicleCounts = expand;
            Mod.SETTINGS.ApplyAndSave();
        }
    }
}
