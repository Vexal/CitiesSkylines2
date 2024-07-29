using Colossal;
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
	public struct CountWorkersJob : IJobChunk
	{
		[ReadOnly]
		public ComponentTypeHandle<Worker> workerHandle;
		[ReadOnly]
		public ComponentTypeHandle<TravelPurpose> travelPurposeHandle;
		[ReadOnly]
		public ComponentTypeHandle<Citizen> citizenHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentBuilding> currentBuildingHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentTransport> currentTransportHandle;
		[ReadOnly]
		public ComponentTypeHandle<HouseholdMember> householdMemberHandle;
		[ReadOnly]
		public ComponentLookup<OutsideConnection> outsideConnectionLookup;
		[ReadOnly]
		public ComponentLookup<Moving> movingLookup;
		[ReadOnly]
		public ComponentLookup<Unspawned> unspawnedLookup;
		[ReadOnly]
		public ComponentLookup<PropertyRenter> propertyRenterLookup;

		public NativeCounter.Concurrent totalCimsInCityLimits;
		public NativeCounter.Concurrent totalCimsOutsideCity;
		public NativeCounter.Concurrent cimsGoingHome;
		public NativeCounter.Concurrent cimsShopping;
		public NativeCounter.Concurrent foreignCimsInCity;
		public NativeCounter.Concurrent nativeCimsOutsideCity;
		public NativeCounter.Concurrent cimsActive;
		public NativeCounter.Concurrent homelessCims;

		public NativeCounter.Concurrent dayShiftWorkers;
		public NativeCounter.Concurrent nightShiftWorkers;
		public NativeCounter.Concurrent eveningShiftWorkers;
		public NativeCounter.Concurrent totalWorkers;
		public NativeCounter.Concurrent totalWorkersWorking;
		public NativeCounter.Concurrent cimsGoingToWork;
		public NativeCounter.Concurrent outsideWorkers;
		public NativeCounter.Concurrent localWorkers;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Worker> workers = chunk.GetNativeArray(ref this.workerHandle);

			bool hasPurpose = chunk.Has<TravelPurpose>();
			bool hasCitizen = chunk.Has<Citizen>();
			bool hasCurrentBuilding = chunk.Has<CurrentBuilding>();
			bool hasCurrentTransport = chunk.Has<CurrentTransport>();
			bool hasHousehold = chunk.Has<HouseholdMember>();

			NativeArray<TravelPurpose> purposes = hasPurpose ? chunk.GetNativeArray(ref this.travelPurposeHandle) : default;
			NativeArray<Citizen> citizens = hasCitizen ? chunk.GetNativeArray(ref this.citizenHandle) : default;
			NativeArray<CurrentBuilding> currentBuildings = hasCitizen ? chunk.GetNativeArray(ref this.currentBuildingHandle) : default;
			NativeArray<CurrentTransport> currentTransports = hasCurrentTransport ? chunk.GetNativeArray(ref this.currentTransportHandle) : default;
			NativeArray<HouseholdMember> householdMembers = hasHousehold ? chunk.GetNativeArray(ref this.householdMemberHandle) : default;

			int dayShiftWorkersT = 0;
			int nightShiftWorkersT = 0;
			int eveningShiftWorkersT = 0;
			int totalWorkersT = 0;
			int totalWorkersWorkingT = 0;
			int cimsGoingToWorkT = 0;
			int outsideWorkersT = 0;
			int localWorkersT = 0;
			int totalCimsInCityLimitsT = 0;
			int totalCimsOutsideCityT = 0;
			int cimsGoingHomeT = 0;
			int cimsGoingToSchoolT = 0;
			int cimsShoppingT = 0;
			int foreignCimsInCity = 0;
			int nativeCimsOutsideCity = 0;
			int cimsAtSchool = 0;
			int cimsActive = 0;
			int homelessCims = 0;

			var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (enumerator.NextEntityIndex(out int i))
			{
				Worker worker = workers[i];
				
				if (worker.m_Shift == Workshift.Day)
				{
					++dayShiftWorkersT;
				}
				else if (worker.m_Shift == Workshift.Evening)
				{
					++eveningShiftWorkersT;
				}
				else if (worker.m_Shift == Workshift.Night)
				{
					++nightShiftWorkersT;
				}

				++totalWorkersT;

				if (hasPurpose)
				{
					if (purposes[i].m_Purpose == Purpose.Working)
					{
						++totalWorkersWorkingT;
					}
					else if (purposes[i].m_Purpose == Purpose.GoingToWork)
					{
						++cimsGoingToWorkT;
					}
					else if (purposes[i].m_Purpose == Purpose.GoingHome)
					{
						++cimsGoingHomeT;
					}
					else if (purposes[i].m_Purpose == Purpose.Shopping)
					{
						++cimsShoppingT;
					}
				}
				else
				{
					if (hasCurrentBuilding && this.propertyRenterLookup.TryGetComponent(worker.m_Workplace, out PropertyRenter propertyRenter))
					{
						if (currentBuildings[i].m_CurrentBuilding == propertyRenter.m_Property) {
							++totalWorkersWorkingT;
						}
					}
				}

				bool isCommuter = false;
				if (hasCitizen)
				{
					if ((citizens[i].m_State & CitizenFlags.Commuter) > 0)
					{
						++outsideWorkersT;
						isCommuter = true;
					}
					else
					{
						++localWorkersT;
						isCommuter = false;
					}
				}

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

				if (hasCurrentBuilding || hasCurrentTransport)
				{
					if (!isOutsideCity)
					{
						++totalCimsInCityLimitsT;
						if (hasCitizen && isCommuter)
						{
							++foreignCimsInCity;
						}

						if (hasCurrentTransport)
						{
							++cimsActive;
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
			}

			this.dayShiftWorkers.Increment(dayShiftWorkersT);
			this.eveningShiftWorkers.Increment(eveningShiftWorkersT);
			this.nightShiftWorkers.Increment(nightShiftWorkersT);
			this.totalWorkers.Increment(totalWorkersT);
			this.totalWorkersWorking.Increment(totalWorkersWorkingT);
			this.cimsGoingToWork.Increment(cimsGoingToWorkT);
			this.outsideWorkers.Increment(outsideWorkersT);
			this.localWorkers.Increment(localWorkersT);
			if (totalCimsInCityLimitsT > 0)
			{
				this.totalCimsInCityLimits.Increment(totalCimsInCityLimitsT);
			}
			if (totalCimsOutsideCityT > 0)
			{
				this.totalCimsOutsideCity.Increment(totalCimsOutsideCityT);
			}
			if (cimsGoingHomeT > 0)
			{
				this.cimsGoingHome.Increment(cimsGoingHomeT);
			}
			if (cimsShoppingT > 0)
			{
				this.cimsShopping.Increment(cimsShoppingT);
			}
			if (cimsActive > 0)
			{
				this.cimsActive.Increment(cimsActive);
			}
			if (homelessCims > 0)
			{
				this.homelessCims.Increment(homelessCims);
			}

			this.foreignCimsInCity.Increment(foreignCimsInCity);
			this.nativeCimsOutsideCity.Increment(nativeCimsOutsideCity);
		}

		public void cleanup()
		{
			this.dayShiftWorkersN.Dispose();
			this.nightShiftWorkersN.Dispose();
			this.eveningShiftWorkersN.Dispose();
			this.totalWorkersN.Dispose();
			this.totalWorkersWorkingN.Dispose();
			this.cimsGoingToWorkN.Dispose();
			this.outsideWorkersN.Dispose();
			this.localWorkersN.Dispose();
		}

		public NativeCounter dayShiftWorkersN;
		public NativeCounter nightShiftWorkersN;
		public NativeCounter eveningShiftWorkersN;
		public NativeCounter totalWorkersN;
		public NativeCounter totalWorkersWorkingN;
		public NativeCounter cimsGoingToWorkN;
		public NativeCounter outsideWorkersN;
		public NativeCounter localWorkersN;

		public void init()
		{
			this.dayShiftWorkersN = new NativeCounter(Allocator.TempJob);
			this.nightShiftWorkersN = new NativeCounter(Allocator.TempJob);
			this.eveningShiftWorkersN = new NativeCounter(Allocator.TempJob);
			this.totalWorkersN = new NativeCounter(Allocator.TempJob);
			this.totalWorkersWorkingN = new NativeCounter(Allocator.TempJob);
			this.cimsGoingToWorkN = new NativeCounter(Allocator.TempJob);
			this.outsideWorkersN = new NativeCounter(Allocator.TempJob);
			this.localWorkersN = new NativeCounter(Allocator.TempJob);

			this.dayShiftWorkers = this.dayShiftWorkersN.ToConcurrent();
			this.nightShiftWorkers = this.nightShiftWorkersN.ToConcurrent();
			this.eveningShiftWorkers = this.eveningShiftWorkersN.ToConcurrent();
			this.totalWorkers =	this.totalWorkersN.ToConcurrent();
			this.totalWorkersWorking = this.totalWorkersWorkingN.ToConcurrent();
			this.cimsGoingToWork = this.cimsGoingToWorkN.ToConcurrent();
			this.outsideWorkers = this.outsideWorkersN.ToConcurrent();
			this.localWorkers =	this.localWorkersN.ToConcurrent();
		}
	}
}
