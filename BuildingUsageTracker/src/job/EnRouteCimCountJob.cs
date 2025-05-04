using Colossal;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	//[BurstCompile]
	public struct EnRouteCimCountJob : IJobChunk
	{
		[ReadOnly]
		public Entity searchTarget;
		[ReadOnly]
		public Entity searchTarget2;
		[ReadOnly]
		public bool hasTarget2;
		[ReadOnly]
		public ComponentTypeHandle<Target> targetHandle;
		[ReadOnly]
		public ComponentTypeHandle<Resident> residentHandle;
		[ReadOnly]
		public ComponentLookup<TravelPurpose> travelPurposeLookup;
		[ReadOnly]
		public ComponentLookup<HouseholdMember> householdMemberLookup;
		[ReadOnly]
		public ComponentLookup<Household> householdLookup;
		[ReadOnly]
		public EntityTypeHandle entityHandle;
		public NativeCounter.Concurrent totalCount;
		public NativeCounter.Concurrent workerCount;
		public NativeCounter.Concurrent studentCount;
		public NativeCounter.Concurrent touristCount;
		public NativeCounter.Concurrent healthcareCount;
		public NativeCounter.Concurrent emergencyCount;
		public NativeCounter.Concurrent jailCount;
		public NativeCounter.Concurrent goingHomeCount;
		public NativeCounter.Concurrent otherCount;
		public NativeCounter.Concurrent shoppingCount;
		public NativeCounter.Concurrent liesureCount;
		public NativeCounter.Concurrent movingInCount;

		[ReadOnly]
		public bool returnEntities;
		public NativeList<Entity> resultEntities;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetHandle);
			bool hasResident = chunk.Has<Resident>();
			NativeArray<Resident> residents = hasResident ? chunk.GetNativeArray(ref this.residentHandle) : default;
			NativeArray<Entity> entities = this.returnEntities ? chunk.GetNativeArray(this.entityHandle) : default;

			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			int totalCount = 0;
			int workerCount = 0;
			int studentCount = 0;
			int touristCount = 0;
			int liesureCount = 0;
			int healthcareCount = 0;
			int emergencyCount = 0;
			int jailCount = 0;
			int goingHomeCount = 0;
			int shoppingCount = 0;
			int movingInCount = 0;
			int otherCount = 0;
			while (chunkIterator.NextEntityIndex(out var i))
			{
				Target target = targets[i];
				if (target.m_Target == this.searchTarget || (this.hasTarget2 && target.m_Target == this.searchTarget2))
				{
					bool shouldCount = true;
					if (hasResident) 
					{
						if ((residents[i].m_Flags & ResidentFlags.Arrived) == 0)
						{
							if (this.travelPurposeLookup.TryGetComponent(residents[i].m_Citizen, out var travelPurpose))
							{
								switch (travelPurpose.m_Purpose)
								{
									case Purpose.GoingToWork:
										++workerCount;
										break;
									case Purpose.GoingToSchool:
										++studentCount;
										break;
									case Purpose.Leisure:
										++liesureCount;
										break;
									case Purpose.Traveling:
									case Purpose.Sightseeing:
										++touristCount;
										break;
									case Purpose.Hospital:
										++healthcareCount;
										break;
									case Purpose.EmergencyShelter:
										++emergencyCount;
										break;
									case Purpose.GoingToJail:
									case Purpose.GoingToPrison:
										++jailCount;
										break;
									case Purpose.GoingHome:
										{
											if (this.householdMemberLookup.TryGetComponent(residents[i].m_Citizen, out var householdMember) &&
												this.householdLookup.TryGetComponent(householdMember.m_Household, out var household) &&
												(household.m_Flags & HouseholdFlags.MovedIn) == 0)
											{
												++movingInCount;
											}
											else
											{
												++goingHomeCount;
											}
										}
										break;
									case Purpose.Shopping:
										++shoppingCount;
										break;
									default:
										++otherCount;
										break;
								}
							}
						}
						else
						{
							shouldCount = false;
						}
					}

					if (shouldCount)
					{
						++totalCount;
						if (this.returnEntities)
						{
							this.resultEntities.Add(entities[i]);
						}
					}
				}
			}

			this.totalCount.Increment(totalCount);
			this.workerCount.Increment(workerCount);
			this.studentCount.Increment(studentCount);
			this.touristCount.Increment(touristCount);
			this.liesureCount.Increment(liesureCount);
			this.healthcareCount.Increment(healthcareCount);
			this.emergencyCount.Increment(jailCount);
			this.jailCount.Increment(emergencyCount);
			this.goingHomeCount.Increment(goingHomeCount);
			this.movingInCount.Increment(movingInCount);
			this.shoppingCount.Increment(shoppingCount);
			this.otherCount.Increment(otherCount);
		}
	}
}
