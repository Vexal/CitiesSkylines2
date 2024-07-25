using Colossal;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.Companies;
using Game.Objects;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CimCensus
{
	[BurstCompile]
	public struct CountCimsJob : IJobChunk
	{
		[ReadOnly]
		public ComponentTypeHandle<Citizen> citizenHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentBuilding> currentBuildingHandle;
		[ReadOnly]
		public ComponentTypeHandle<TravelPurpose> travelPurposeHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentTransport> currentTransportHandle;
		[ReadOnly]
		public ComponentTypeHandle<HouseholdMember> householdMemberHandle;
		[ReadOnly]
		public ComponentTypeHandle<Game.Citizens.Student> studentTypeHandle;
		[ReadOnly]
		public ComponentLookup<OutsideConnection> outsideConnectionLookup;
		[ReadOnly]
		public ComponentLookup<Moving> movingLookup;
		[ReadOnly]
		public ComponentLookup<Unspawned> unspawnedLookup;
		[ReadOnly]
		public ComponentLookup<MovingAway> movingAwayLookup;
		[ReadOnly]
		public ComponentLookup<CommuterHousehold> commuterHouseholdLookup;
		[ReadOnly]
		public ComponentLookup<PropertySeeker> propertySeekerLookup;
		[ReadOnly]
		public ComponentLookup<PropertyRenter> propertyRenterLookup;

		public NativeCounter.Concurrent totalCimsInCityLimits;
		public NativeCounter.Concurrent totalCimsOutsideCity;
		public NativeCounter.Concurrent totalExtraCims;
		public NativeCounter.Concurrent foreignCimsInCity;
		public NativeCounter.Concurrent nativeCimsOutsideCity;
		public NativeCounter.Concurrent cimsGoingHome;
		public NativeCounter.Concurrent cimsGoingToSchool;
		public NativeCounter.Concurrent cimsShopping;
		public NativeCounter.Concurrent cimsAtSchool;
		public NativeCounter.Concurrent cimsAtSchoolInsideCity;
		public NativeCounter.Concurrent cimsAtSchoolOutsideCity;
		public NativeCounter.Concurrent cimsActive;
		public NativeCounter.Concurrent homelessCims;
		public NativeCounter.Concurrent totalStudents;

		public NativeCounter.Concurrent residentsInVehiclesC;
		public NativeCounter.Concurrent residentsSpawnedC;
		public NativeCounter.Concurrent cimsInBuildingsC;
		public NativeCounter.Concurrent cimsMovingC;
		public NativeCounter.Concurrent totalResidentsC;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Citizen> citizens = chunk.GetNativeArray(ref this.citizenHandle);

			bool hasPurpose = chunk.Has<TravelPurpose>();
			bool hasCurrentBuilding = chunk.Has<CurrentBuilding>();
			bool hasCurrentTransport = chunk.Has<CurrentTransport>();
			bool hasHousehold = chunk.Has<HouseholdMember>();
			bool hasStudent = chunk.Has<Game.Citizens.Student>();

			NativeArray<TravelPurpose> purposes = hasPurpose ? chunk.GetNativeArray(ref this.travelPurposeHandle) : default;
			NativeArray<CurrentBuilding> currentBuildings = hasCurrentBuilding ? chunk.GetNativeArray(ref this.currentBuildingHandle) : default;
			NativeArray<CurrentTransport> currentTransports = hasCurrentTransport ? chunk.GetNativeArray(ref this.currentTransportHandle) : default;
			NativeArray<HouseholdMember> householdMembers = hasHousehold ? chunk.GetNativeArray(ref this.householdMemberHandle) : default;
			NativeArray<Game.Citizens.Student> students = hasStudent ? chunk.GetNativeArray(ref this.studentTypeHandle) : default;

			int totalCimsInCityLimitsT = 0;
			int totalCimsOutsideCityT = 0;
			int extraCimsT = 0;
			int foreignCimsInCity = 0;
			int nativeCimsOutsideCity = 0;
			int cimsGoingHomeT = 0;
			int cimsGoingToSchoolT = 0;
			int cimsShoppingT = 0;
			int cimsAtSchool = 0;
			int cimsAtSchoolInsideCity = 0;
			int cimsAtSchoolOutsideCity = 0;
			int cimsActive = 0;
			int homelessCims = 0;

			int residentsInVehicles = 0;
			int residentsSpawned = 0;
			int cimsInBuildings = 0;
			int cimsMoving = 0;
			int totalStudents = 0;

			var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (enumerator.NextEntityIndex(out int i))
			{

				bool isCommuter = (citizens[i].m_State & CitizenFlags.Commuter) > 0;
				bool isOutsideCity = false;

				if (hasCurrentBuilding)
				{
					isOutsideCity |= this.outsideConnectionLookup.HasComponent(currentBuildings[i].m_CurrentBuilding);
				}

				if (hasCurrentTransport && !isOutsideCity)
				{
					isOutsideCity |= this.movingLookup.HasComponent(currentTransports[i].m_CurrentTransport) &&
						this.unspawnedLookup.HasComponent(currentTransports[i].m_CurrentTransport);
				}
				
				if (hasCurrentBuilding)
				{
					if (isOutsideCity)
					{
						if (isCommuter ||
							hasHousehold && 
							(this.movingAwayLookup.HasComponent(householdMembers[i].m_Household) ||
							this.commuterHouseholdLookup.HasComponent(householdMembers[i].m_Household) ||
							this.propertySeekerLookup.HasComponent(householdMembers[i].m_Household)))
						{
							++extraCimsT;
							continue;
						}					
					}
				}
				else if (hasCurrentTransport)
				{
					if (isOutsideCity)
					{
						if (isCommuter ||
							hasHousehold && 
							(this.movingAwayLookup.HasComponent(householdMembers[i].m_Household) ||
							this.commuterHouseholdLookup.HasComponent(householdMembers[i].m_Household) ||
							this.propertySeekerLookup.HasComponent(householdMembers[i].m_Household)))
						{
							++extraCimsT;
							continue;
						}
					}				
				}

				if (hasCurrentBuilding)
				{
					if (hasStudent && students[i].m_School == currentBuildings[i].m_CurrentBuilding)
					{
						++cimsAtSchool;
						if (isOutsideCity)
						{
							++cimsAtSchoolOutsideCity;
						}
						else
						{
							++cimsAtSchoolInsideCity;
						}
					}
				}
				if (hasCurrentBuilding || hasCurrentTransport)
				{
					if (!isOutsideCity)
					{
						++totalCimsInCityLimitsT;
						if (isCommuter)
						{
							++foreignCimsInCity;
						}
						if (hasCurrentTransport)
						{
							++cimsActive;
						} 
						else
						{
							++cimsInBuildings;
						}

						if (hasHousehold && !this.propertyRenterLookup.HasComponent(householdMembers[i].m_Household))
						{
							++homelessCims;
						}
					}
					else
					{
						++totalCimsOutsideCityT;
						++nativeCimsOutsideCity;
					}
				}

				if (hasPurpose)
				{
					if (purposes[i].m_Purpose == Purpose.GoingHome)
					{
						++cimsGoingHomeT;
					}
					else if (purposes[i].m_Purpose == Purpose.GoingToSchool)
					{
						++cimsGoingToSchoolT;
					}
					else if (purposes[i].m_Purpose == Purpose.Shopping)
					{
						++cimsShoppingT;
					}
				}

				if (hasStudent)
				{
					++totalStudents;
				}
			}

			if (totalCimsInCityLimitsT > 0)
			{
				this.totalCimsInCityLimits.Increment(totalCimsInCityLimitsT);
			}
			if (totalCimsOutsideCityT > 0)
			{
				this.totalCimsOutsideCity.Increment(totalCimsOutsideCityT);
			}
			if (extraCimsT > 0)
			{
				this.totalExtraCims.Increment(extraCimsT);
			}
			if (cimsGoingToSchoolT > 0)
			{
				this.cimsGoingToSchool.Increment(cimsGoingToSchoolT);
			}
			if (cimsGoingHomeT > 0)
			{
				this.cimsGoingHome.Increment(cimsGoingHomeT);
			}
			if (cimsShoppingT > 0)
			{
				this.cimsShopping.Increment(cimsShoppingT);
			}
			if (cimsAtSchool > 0)
			{
				this.cimsAtSchool.Increment(cimsAtSchool);
			}
			if (cimsActive > 0)
			{
				this.cimsActive.Increment(cimsActive);
			}
			if (homelessCims > 0)
			{
				this.homelessCims.Increment(homelessCims);
			}
			if (cimsAtSchoolInsideCity > 0)
			{
				this.cimsAtSchoolInsideCity.Increment(cimsAtSchoolInsideCity);
			}
			if (cimsAtSchoolOutsideCity > 0)
			{
				this.cimsAtSchoolOutsideCity.Increment(cimsAtSchoolOutsideCity);
			}
			if (totalStudents > 0)
			{
				this.totalStudents.Increment(totalStudents);
			}

			this.foreignCimsInCity.Increment(foreignCimsInCity);
			this.nativeCimsOutsideCity.Increment(nativeCimsOutsideCity);

			/*this.residentsInVehiclesC.Increment(residentsInVehicles);
			this.residentsSpawnedC.Increment(residentsSpawned);
			this.cimsInBuildingsC.Increment(cimsInBuildings);
			this.cimsMovingC.Increment(cimsMoving);
			this.totalResidentsC.Increment(totalResidents);*/
		}
	}
}
