using Colossal;
using Game.Citizens;
using Game.Companies;
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

		public NativeCounter dayShiftWorkers;
		public NativeCounter nightShiftWorkers;
		public NativeCounter eveningShiftWorkers;
		public NativeCounter totalWorkers;
		public NativeCounter totalWorkersWorking;
		public NativeCounter cimsGoingToWork;
		public NativeCounter outsideWorkers;
		public NativeCounter localWorkers;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Worker> workers = chunk.GetNativeArray(ref this.workerHandle);

			bool hasPurpose = chunk.Has<TravelPurpose>();
			bool hasCitizen = chunk.Has<Citizen>();

			NativeArray<TravelPurpose> purposes = hasPurpose ? chunk.GetNativeArray(ref this.travelPurposeHandle) : default;
			NativeArray<Citizen> citizens = hasCitizen ? chunk.GetNativeArray(ref this.citizenHandle) : default;

			int dayShiftWorkersT = 0;
			int nightShiftWorkersT = 0;
			int eveningShiftWorkersT = 0;
			int totalWorkersT = 0;
			int totalWorkersWorkingT = 0;
			int cimsGoingToWorkT = 0;
			int outsideWorkersT = 0;
			int localWorkersT = 0;

			for (int i = 0; i < workers.Length; i++)
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
				}

				if (hasCitizen)
				{
					if ((citizens[i].m_State & CitizenFlags.Commuter) > 0)
					{
						++outsideWorkersT;
					}
					else
					{
						++localWorkersT;
					}
				}
			}

			this.dayShiftWorkers.Count += dayShiftWorkersT;
			this.eveningShiftWorkers.Count += eveningShiftWorkersT;
			this.nightShiftWorkers.Count += nightShiftWorkersT;
			this.totalWorkers.Count += totalWorkersT;
			this.totalWorkersWorking.Count += totalWorkersWorkingT;
			this.cimsGoingToWork.Count += cimsGoingToWorkT;
			this.outsideWorkers.Count += outsideWorkersT;
			this.localWorkers.Count += localWorkersT;
		}

		public void cleanup()
		{
			this.dayShiftWorkers.Dispose();
			this.nightShiftWorkers.Dispose();
			this.eveningShiftWorkers.Dispose();
			this.totalWorkers.Dispose();
			this.totalWorkersWorking.Dispose();
			this.cimsGoingToWork.Dispose();
			this.outsideWorkers.Dispose();
			this.localWorkers.Dispose();
		}

		public void init()
		{
			this.dayShiftWorkers = new NativeCounter(Allocator.TempJob);
			this.nightShiftWorkers = new NativeCounter(Allocator.TempJob);
			this.eveningShiftWorkers = new NativeCounter(Allocator.TempJob);
			this.totalWorkers = new NativeCounter(Allocator.TempJob);
			this.totalWorkersWorking = new NativeCounter(Allocator.TempJob);
			this.cimsGoingToWork = new NativeCounter(Allocator.TempJob);
			this.outsideWorkers = new NativeCounter(Allocator.TempJob);
			this.localWorkers = new NativeCounter(Allocator.TempJob);
		}
	}
}
