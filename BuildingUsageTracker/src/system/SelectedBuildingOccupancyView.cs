using Colossal;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	partial class SelectedBuildingOccupancyView : SelectedBuildingInfoSection
	{
		private EntityQuery buildingOccupantQuery;
		private BuildingOccupancy occupancy = default;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.buildingOccupantQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<CurrentBuilding>(),
				ComponentType.ReadOnly<Citizen>(),
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
		}

		protected override void update(Entity selectedEntity)
		{
			var resultCounter = new NativeCounter(Allocator.TempJob);
			var workerResults = new NativeCounter(Allocator.TempJob);
			var studentResults = new NativeCounter(Allocator.TempJob);
			var touristResults = new NativeCounter(Allocator.TempJob);
			var healthcareResults = new NativeCounter(Allocator.TempJob);
			var emergencyResults = new NativeCounter(Allocator.TempJob);
			var jailResults = new NativeCounter(Allocator.TempJob);
			var sleepCount = new NativeCounter(Allocator.TempJob);
			var otherCount = new NativeCounter(Allocator.TempJob);

			BuildingOccupantCountJob job = new BuildingOccupantCountJob();
			job.searchTarget = selectedEntity;
			job.currentBuildingHandle = SystemAPI.GetComponentTypeHandle<CurrentBuilding>(true);
			job.travelPurposeHandle = SystemAPI.GetComponentTypeHandle<TravelPurpose>(true);
			job.results = resultCounter.ToConcurrent();
			job.workerCount = workerResults.ToConcurrent();
			job.studentCount = studentResults.ToConcurrent();
			job.touristCount = touristResults.ToConcurrent();
			job.healthcareCount = healthcareResults.ToConcurrent();
			job.emergencyCount = emergencyResults.ToConcurrent();
			job.jailCount = jailResults.ToConcurrent();
			job.sleepCount = sleepCount.ToConcurrent();
			job.otherCount = otherCount.ToConcurrent();

			var jobHandle = JobChunkExtensions.ScheduleParallel(job, this.buildingOccupantQuery, default);

			jobHandle.Complete();
			this.occupancy.totalCount = resultCounter.Count;
			this.occupancy.workerCount = workerResults.Count;
			this.occupancy.studentCount = studentResults.Count;
			this.occupancy.touristCount = touristResults.Count;
			this.occupancy.healthcareCount = healthcareResults.Count;
			this.occupancy.emergencyCount = emergencyResults.Count;
			this.occupancy.jailCount = jailResults.Count;
			this.occupancy.sleepCount = sleepCount.Count;
			this.occupancy.otherCount = otherCount.Count;
			resultCounter.Dispose();
			workerResults.Dispose();
			studentResults.Dispose();
			touristResults.Dispose();
			healthcareResults.Dispose();
			emergencyResults.Dispose();
			jailResults.Dispose();
			sleepCount.Dispose();
			otherCount.Dispose();
		}

		protected override void selectionChanged()
		{
			this.occupancy = new BuildingOccupancy();
		}

		private struct BuildingOccupancy
		{
			public int totalCount;
			public int workerCount;
			public int studentCount;
			public int touristCount;
			public int healthcareCount;
			public int emergencyCount;
			public int jailCount;
			public int sleepCount;
			public int otherCount;
		}

		protected override string group => this.buildOccupancyCountText();
		private string buildOccupancyCountText()
		{
			return "{\"occupantCount\":" + this.occupancy.totalCount + 
				Utils.jsonFieldC("workers", this.occupancy.workerCount) +
				Utils.jsonFieldC("students", this.occupancy.studentCount) +
				Utils.jsonFieldC("tourists", this.occupancy.touristCount) +
				Utils.jsonFieldC("patients", this.occupancy.healthcareCount) +
				Utils.jsonFieldC("emergency", this.occupancy.emergencyCount) +
				Utils.jsonFieldC("inmates", this.occupancy.jailCount) +
				Utils.jsonFieldC("sleepers", this.occupancy.sleepCount) +
				Utils.jsonFieldC("other", this.occupancy.otherCount) +
				"}";
		}
		protected override bool shouldBeVisible(Entity selectedEntity)
		{
			return Mod.SETTINGS.showBuildingOccupancy && base.shouldBeVisible(selectedEntity);
		}
	}
}
