using Colossal;
using Game.Citizens;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	[BurstCompile]
	public struct BuildingOccupantCountJob : IJobChunk
	{
		[ReadOnly]
		public Entity searchTarget;
		[ReadOnly]
		public ComponentTypeHandle<CurrentBuilding> currentBuildingHandle;
		[ReadOnly]
		public ComponentTypeHandle<TravelPurpose> travelPurposeHandle;
		public NativeCounter.Concurrent results;
		public NativeCounter.Concurrent workerCount;
		public NativeCounter.Concurrent studentCount;
		public NativeCounter.Concurrent touristCount;
		public NativeCounter.Concurrent healthcareCount;
		public NativeCounter.Concurrent emergencyCount;
		public NativeCounter.Concurrent jailCount;
		public NativeCounter.Concurrent sleepCount;
		public NativeCounter.Concurrent otherCount;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<CurrentBuilding> buildings = chunk.GetNativeArray(ref this.currentBuildingHandle);
			bool hasTravelPurpose = chunk.Has<TravelPurpose>();
			NativeArray<TravelPurpose> travelPurposes;
			travelPurposes = hasTravelPurpose ? chunk.GetNativeArray(ref this.travelPurposeHandle) : default;

			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			int count = 0;
			int workerCount = 0;
			int studentCount = 0;
			int touristCount = 0;
			int healthcareCount = 0;
			int emergencyCount = 0;
			int jailCount = 0;
			int sleepCount = 0;
			int otherCount = 0;
			while (chunkIterator.NextEntityIndex(out var i))
			{
				CurrentBuilding building = buildings[i];
				if (building.m_CurrentBuilding == this.searchTarget)
				{
					++count;
					if (hasTravelPurpose)
					{
						TravelPurpose travelPurpose = travelPurposes[i];
						switch (travelPurpose.m_Purpose)
						{
							case Purpose.Working:
								++workerCount;
								break;
							case Purpose.Studying:
								++studentCount;
								break;
							case Purpose.VisitAttractions:
								++touristCount;
								break;
							case Purpose.InHospital:
								++healthcareCount;
								break;
							case Purpose.InEmergencyShelter:
								++emergencyCount;
								break;
							case Purpose.InJail:
							case Purpose.InPrison:
								++jailCount;
								break;
							case Purpose.Sleeping:
								++sleepCount;
								break;
							default:
								++otherCount;
								break;
						}
					}
				}
			}

			this.results.Increment(count);
			this.workerCount.Increment(workerCount);
			this.studentCount.Increment(studentCount);
			this.touristCount.Increment(touristCount);
			this.healthcareCount.Increment(healthcareCount);
			this.emergencyCount.Increment(jailCount);
			this.jailCount.Increment(emergencyCount);
			this.sleepCount.Increment(sleepCount);
			this.otherCount.Increment(otherCount);
		}
	}
}
