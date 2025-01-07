using Colossal;
using Game.Buildings;
using Game.Citizens;
using Game.Creatures;
using Game.Objects;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pandemic
{
	[BurstCompile]
	public struct ComputeDiseaseSpreadParametersJob : IJobChunk
	{
		[ReadOnly]
		public ComponentTypeHandle<Citizen> citizenHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentTransport> currentTransportHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentDisease> currentDiseaseHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentBuilding> currentBuildingHandle;
		[ReadOnly]
		public ComponentLookup<CurrentVehicle> currentVehicleLookup;
		[ReadOnly]
		public ComponentLookup<Transform> transformLookup;
		[ReadOnly]
		public ComponentLookup<TripSource> tripSourceLookup;
		[ReadOnly]
		public ComponentLookup<Disease> diseaseLookup;
		[ReadOnly]
		public ComponentLookup<Hospital> hospitalLookup;
		[ReadOnly]
		public bool masksRequired;
		[ReadOnly]
		public int maskAversionModifier;
		[ReadOnly]
		public float maskSpreadModifier;

		public NativeArray<float3> diseasePositions;
		public NativeArray<float> diseaseRadiusSq;
		public NativeArray<float> spreadChance;
		public NativeArray<Entity> diseases;
		public NativeCounter.Concurrent resultCounter;
		[ReadOnly]
		public NativeCounter rc;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Citizen> citizens = chunk.GetNativeArray(ref this.citizenHandle);
			NativeArray<CurrentTransport> currentTransports = chunk.GetNativeArray(ref this.currentTransportHandle);
			NativeArray<CurrentDisease> currentDiseases = chunk.GetNativeArray(ref this.currentDiseaseHandle);

			bool isStudent = chunk.Has<Game.Citizens.Student>();
			bool isInBuilding = chunk.Has<CurrentBuilding>();
			NativeArray<CurrentBuilding> currentBuildings = isInBuilding ? chunk.GetNativeArray(ref this.currentBuildingHandle) : default;

			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (chunkIterator.NextEntityIndex(out var i))
			{
				if (this.currentVehicleLookup.HasComponent(currentTransports[i].m_CurrentTransport))
				{
					continue;
				}

				if (!this.transformLookup.TryGetComponent(currentTransports[i].m_CurrentTransport, out Transform t))
				{
					continue;
				}

				if (!this.diseaseLookup.TryGetComponent(currentDiseases[i].disease, out var disease))
				{
					continue;
				}

				if (isInBuilding && this.hospitalLookup.HasComponent(currentBuildings[i].m_CurrentBuilding))
				{
					continue;
				}

				if (this.tripSourceLookup.HasComponent(currentTransports[i].m_CurrentTransport))
				{
					continue;
				}

				int index = this.resultCounter.Increment(1);
				this.diseasePositions[index] = t.m_Position;
				this.diseases[index] = currentDiseases[i].disease;
				this.spreadChance[index] = disease.baseSpreadChance * currentDiseases[i].progression;

				float radius = disease.baseSpreadRadius * currentDiseases[i].progression;

				if (this.masksRequired)
				{
					if (PandemicSpreadSystem.citizenWearsMask(citizens[i], this.maskAversionModifier, isStudent))
					{
						radius *= this.maskSpreadModifier;
					}
				}

				this.diseaseRadiusSq[index] = radius * radius;
			}
		}

		public void cleanup(JobHandle j)
		{
			this.diseasePositions.Dispose(j);
			this.diseaseRadiusSq.Dispose(j);
			this.diseases.Dispose(j);
			this.spreadChance.Dispose(j);
		}

		public void cleanup()
		{
			this.diseasePositions.Dispose();
			this.diseaseRadiusSq.Dispose();
			this.diseases.Dispose();
			this.rc.Dispose();
			this.spreadChance.Dispose();
		}
	}
}
