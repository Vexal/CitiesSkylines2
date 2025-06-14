using Colossal;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Pathfind;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	[BurstCompile]
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
		public ComponentTypeHandle<CurrentVehicle> currentVehicleHandle;
		[ReadOnly]
		public ComponentLookup<TravelPurpose> travelPurposeLookup;
		[ReadOnly]
		public ComponentLookup<HouseholdMember> householdMemberLookup;
		[ReadOnly]
		public ComponentLookup<Household> householdLookup;
		[ReadOnly]
		public ComponentLookup<ParkingLane> parkingLaneLookup;
		[ReadOnly]
		public ComponentLookup<PathOwner> pathOwnerLookup;
		[ReadOnly]
		public BufferLookup<PathElement> pathLookup;
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
		public NativeCounter.Concurrent passingThroughCount;
		public NativeCounter.Concurrent inVehicleCount;

		[ReadOnly]
		public bool returnEntities;
		public NativeList<Entity> resultEntities;

		[ReadOnly]
		public bool checkPathElements;
		[ReadOnly]
		public bool isParkingStructure;
		[ReadOnly]
		public NativeHashSet<Entity> pathTargets;
		[ReadOnly]
		public BufferTypeHandle<PathElement> pathHandle;
		[ReadOnly]
		public ComponentTypeHandle<PathOwner> pathOwnerHandle;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetHandle);
			bool hasResident = chunk.Has<Resident>();
			bool checkPaths = this.checkPathElements && chunk.Has<PathOwner>() && chunk.Has<PathElement>();
			bool checkVehicle = this.checkPathElements && chunk.Has<CurrentVehicle>();
			NativeArray<Resident> residents = hasResident ? chunk.GetNativeArray(ref this.residentHandle) : default;
			NativeArray<CurrentVehicle> currentVehicles = checkVehicle ? chunk.GetNativeArray(ref this.currentVehicleHandle) : default;
			NativeArray<Entity> entities = this.returnEntities ? chunk.GetNativeArray(this.entityHandle) : default;

			BufferAccessor<PathElement> entityPaths = checkPaths ? chunk.GetBufferAccessor(ref this.pathHandle) : default;
			NativeArray<PathOwner> pathOwners = checkPaths ? chunk.GetNativeArray(ref this.pathOwnerHandle) : default;

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
			int passingThroughCount = 0;
			int inVehicleCount = 0;
			while (chunkIterator.NextEntityIndex(out var i))
			{
				Target target = targets[i];
				bool isExactTarget = target.m_Target == this.searchTarget ||
					(this.hasTarget2 && target.m_Target == this.searchTarget2);

				bool isPassingThrough = false;
				if (!isExactTarget)
				{
					if (checkPaths)
					{
						DynamicBuffer<PathElement> path = entityPaths[i];
						for (int pathIndex = pathOwners[i].m_ElementIndex; pathIndex < path.Length; ++pathIndex)
						{
							if (this.pathTargets.Contains(path[pathIndex].m_Target))
							{
								isPassingThrough = true;
								break;
							}
						}
					}
					if (checkVehicle && this.pathLookup.TryGetBuffer(currentVehicles[i].m_Vehicle, out var vehiclePath) && this.pathOwnerLookup.TryGetComponent(currentVehicles[i].m_Vehicle, out var vehiclePathOwner))
					{
						for (int pathIndex = vehiclePathOwner.m_ElementIndex; pathIndex < vehiclePath.Length; ++pathIndex)
						{
							if (this.pathTargets.Contains(vehiclePath[pathIndex].m_Target))
							{
								isPassingThrough = true;
								break;
							}
						}
					}
				}
				if (isExactTarget || isPassingThrough)
				{
					bool shouldCount = true;
					if (hasResident) 
					{
						if ((residents[i].m_Flags & ResidentFlags.Arrived) == 0)
						{
							if ((residents[i].m_Flags & ResidentFlags.InVehicle) > 0)
							{
								++inVehicleCount;
							}

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

						if (isPassingThrough)
						{
							++passingThroughCount;
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
			this.passingThroughCount.Increment(passingThroughCount);
			this.inVehicleCount.Increment(inVehicleCount);
		}
	}
}
