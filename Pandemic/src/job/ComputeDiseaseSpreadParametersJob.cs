using Colossal;
using Game.Citizens;
using Game.Creatures;
using Game.Objects;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Pandemic
{
	//[BurstCompile]
	public struct ComputeDiseaseSpreadParametersJob : IJobChunk
	{
		[ReadOnly]
		public ComponentTypeHandle<Citizen> citizenHandle;
		[ReadOnly]
		public ComponentTypeHandle<CurrentTransport> currentTransportHandle;
		[ReadOnly]
		public ComponentLookup<CurrentVehicle> currentVehicleLookup;
		[ReadOnly]
		public ComponentLookup<Transform> transformLookup;
		[ReadOnly]
		public bool masksRequired;
		[ReadOnly]
		public int maskAversionModifier;
		[ReadOnly]
		public float maskSpreadModifier;
		[ReadOnly]
		public float baseSpreadRadius;

		public NativeArray<float3> diseasePositions;
		public NativeArray<float> diseaseRadiusSq;
		public NativeCounter.Concurrent resultCounter;
		[ReadOnly]
		public NativeCounter rc;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Citizen> citizens = chunk.GetNativeArray(ref this.citizenHandle);
			NativeArray<CurrentTransport> currentTransports = chunk.GetNativeArray(ref this.currentTransportHandle);

			bool isStudent = chunk.Has<Student>();

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

				int index = this.resultCounter.Increment(1);
				this.diseasePositions[index] = t.m_Position;
				float radius = this.baseSpreadRadius;

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
	}
}
