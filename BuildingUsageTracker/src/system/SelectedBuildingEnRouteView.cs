using Colossal;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Tools;
using Game.Vehicles;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	partial class SelectedBuildingEnRouteView : SelectedBuildingInfoSection
	{
		private EntityQuery enrouteCitizenQuery;
		private EntityQuery enrouteVehicleQuery;
		private bool showEntities = false;
		private Counters counters = new Counters { json = "{}" };

		protected override void OnCreate()
		{
			base.OnCreate();

			this.enrouteCitizenQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Human>(),
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

			AddBinding(new TriggerBinding<bool>("BuildingUsageTracker", "toggleShowEnrouteEntityList", s => { this.showEntities = s; this.uf.ForceUpdate(); }));
		}

		protected override void update(Entity selectedEntity)
		{
			this.counters.init(this.showEntities);
			EnRouteCimCountJob job = new EnRouteCimCountJob();
			job.searchTarget = selectedEntity;
			if (EntityManager.TryGetBuffer<Renter>(selectedEntity, true, out var renterBuffer) && renterBuffer.Length > 0)
			{
				job.searchTarget2 = renterBuffer[0].m_Renter;
				job.hasTarget2 = true;
			}

			if (this.showEntities)
			{
				job.entityHandle = SystemAPI.GetEntityTypeHandle();
				job.returnEntities = true;
			}

			this.counters.setJobCounters(ref job);
			job.targetHandle = SystemAPI.GetComponentTypeHandle<Target>(true);
			job.residentHandle = SystemAPI.GetComponentTypeHandle<Resident>(true);
			job.travelPurposeLookup = SystemAPI.GetComponentLookup<TravelPurpose>(true);
			job.householdMemberLookup = SystemAPI.GetComponentLookup<HouseholdMember>(true);
			job.householdLookup = SystemAPI.GetComponentLookup<Household>(true);

			var jobHandle = this.showEntities ? JobChunkExtensions.Schedule(job, this.enrouteCitizenQuery, default) :
				JobChunkExtensions.ScheduleParallel(job, this.enrouteCitizenQuery, default);

			jobHandle.Complete();
			this.counters.disposeAndBuild();
		}

		private struct Counters
		{
			public NativeCounter totalCount;
			public NativeCounter workerCount;
			public NativeCounter studentCount;
			public NativeCounter touristCount;
			public NativeCounter healthcareCount;
			public NativeCounter emergencyCount;
			public NativeCounter jailCount;
			public NativeCounter goingHomeCount;
			public NativeCounter otherCount;
			public NativeCounter shoppingCount;
			public NativeCounter liesureCount;
			public NativeCounter movingInCount;
			public NativeList<Entity> entities;
			public string json;

			public void init(bool returnEntities)
			{
				this.totalCount = new NativeCounter(Allocator.TempJob);
				this.workerCount = new NativeCounter(Allocator.TempJob);
				this.studentCount = new NativeCounter(Allocator.TempJob);
				this.touristCount = new NativeCounter(Allocator.TempJob);
				this.healthcareCount = new NativeCounter(Allocator.TempJob);
				this.emergencyCount = new NativeCounter(Allocator.TempJob);
				this.jailCount = new NativeCounter(Allocator.TempJob);
				this.goingHomeCount = new NativeCounter(Allocator.TempJob);
				this.otherCount = new NativeCounter(Allocator.TempJob);
				this.shoppingCount = new NativeCounter(Allocator.TempJob);
				this.liesureCount = new NativeCounter(Allocator.TempJob);
				this.movingInCount = new NativeCounter(Allocator.TempJob);

				if (returnEntities)
				{
					this.entities = new NativeList<Entity>(Allocator.TempJob);
				}
			}

			public void setJobCounters(ref EnRouteCimCountJob job)
			{
				job.totalCount = this.totalCount.ToConcurrent();
				job.workerCount = this.workerCount.ToConcurrent();
				job.studentCount = this.studentCount.ToConcurrent();
				job.touristCount = this.touristCount.ToConcurrent();
				job.healthcareCount = this.healthcareCount.ToConcurrent();
				job.emergencyCount = this.emergencyCount.ToConcurrent();
				job.jailCount = this.jailCount.ToConcurrent();
				job.goingHomeCount = this.goingHomeCount.ToConcurrent();
				job.otherCount = this.otherCount.ToConcurrent();
				job.shoppingCount = this.shoppingCount.ToConcurrent();
				job.liesureCount = this.liesureCount.ToConcurrent();
				job.movingInCount = this.movingInCount.ToConcurrent();

				if (this.entities.IsCreated)
				{
					job.resultEntities = this.entities;
				}
			}

			public void disposeAndBuild()
			{
				this.json = "{\"totalCount\":" + this.totalCount.Count +
				Utils.jsonFieldC("workerCount", this.workerCount) +
				Utils.jsonFieldC("studentCount", this.studentCount) +
				Utils.jsonFieldC("touristCount", this.touristCount) +
				Utils.jsonFieldC("healthcareCount", this.healthcareCount) +
				Utils.jsonFieldC("emergencyCount", this.emergencyCount) +
				Utils.jsonFieldC("jailCount", this.jailCount) +
				Utils.jsonFieldC("goingHomeCount", this.goingHomeCount) +
				Utils.jsonFieldC("other", this.otherCount) +
				Utils.jsonFieldC("shoppingCount", this.shoppingCount) +
				Utils.jsonFieldC("liesureCount", this.liesureCount) +
				Utils.jsonFieldC("movingInCount", this.movingInCount);

				if (this.entities.IsCreated)
				{
					this.json += Utils.jsonArray("entities", this.entities);
					this.entities.Dispose();
				}

				this.json += "}";

				this.totalCount.Dispose();
				this.workerCount.Dispose();
				this.studentCount.Dispose();
				this.touristCount.Dispose();
				this.healthcareCount.Dispose();
				this.emergencyCount.Dispose();
				this.jailCount.Dispose();
				this.goingHomeCount.Dispose();
				this.otherCount.Dispose();
				this.shoppingCount.Dispose();
				this.liesureCount.Dispose();
				this.movingInCount.Dispose();
			}
		}

		protected override void selectionChanged()
		{
			this.showEntities = false;
		}

		protected override string group => this.counters.json;
	}
}
