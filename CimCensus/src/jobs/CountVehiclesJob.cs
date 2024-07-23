using Colossal;
using Game.Objects;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CimCensus
{
	[BurstCompile]
	public struct CountVehiclesJob : IJobChunk
	{
		[ReadOnly]
		public ComponentTypeHandle<PersonalCar> personalCarHandle;
		[ReadOnly]
		public ComponentTypeHandle<DeliveryTruck> deliveryTruckHandle;
		[ReadOnly]
		public ComponentTypeHandle<ParkedCar> parkedCarHandle;
		[ReadOnly]
		public ComponentTypeHandle<Ambulance> ambualnceHandle;
		[ReadOnly]
		public ComponentTypeHandle<PublicTransport> publicTransoprtHandle;
		[ReadOnly]
		public ComponentTypeHandle<CargoTransport> cargoTransportHandle;
		[ReadOnly]
		public ComponentLookup<Game.Net.OutsideConnection> outsideConnectionLookup;

		public NativeCounter.Concurrent dummyTraffic;
		public NativeCounter.Concurrent realTraffic;
		public NativeCounter.Concurrent deliveryTraffic;
		public NativeCounter.Concurrent parkedVehicles;
		public NativeCounter.Concurrent personalVehicles;
		public NativeCounter.Concurrent dispatchedAmbulances;

		private readonly static AmbulanceFlags validAmbulances = AmbulanceFlags.Dispatched | AmbulanceFlags.Transporting | AmbulanceFlags.AtTarget;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{

			bool hasParked = chunk.Has<ParkedCar>();
			bool hasPersonalCar = chunk.Has<PersonalCar>();
			bool hasDeliveryTruck = chunk.Has<DeliveryTruck>();
			bool hasAmbulance = chunk.Has<Ambulance>();
			bool hasPublicTransport = chunk.Has<PublicTransport>();
			bool hasUnspawned = chunk.Has<Unspawned>();
			bool hasCargo = chunk.Has<CargoTransport>();

			NativeArray<PersonalCar> personalCars = hasPersonalCar ? chunk.GetNativeArray(ref this.personalCarHandle) : default;
			NativeArray<ParkedCar> parkedCars = hasPersonalCar ? chunk.GetNativeArray(ref this.parkedCarHandle) : default;
			NativeArray<DeliveryTruck> deliveryTrucks = hasDeliveryTruck ? chunk.GetNativeArray(ref this.deliveryTruckHandle) : default;
			NativeArray<Ambulance> ambulances = hasAmbulance ? chunk.GetNativeArray(ref this.ambualnceHandle) : default;
			NativeArray<PublicTransport> publicTransports = hasPublicTransport ? chunk.GetNativeArray(ref this.publicTransoprtHandle) : default;
			NativeArray<CargoTransport> cargoTransports = hasCargo ? chunk.GetNativeArray(ref this.cargoTransportHandle) : default;

			int dummyTraffic = 0;
			int realTraffic = 0;
			int deliveryTraffic = 0;
			int parkedVehicles = 0;
			int personalVehicles = 0;
			int dispatchedAmbulances = 0;

			var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (enumerator.NextEntityIndex(out int i))
			{
				if (hasParked)
				{
					if (!this.outsideConnectionLookup.HasComponent(parkedCars[i].m_Lane))
					{
						++parkedVehicles;
					}

					continue;
				}

				if (hasUnspawned)
				{
					continue;
				}

				if (hasPersonalCar)
				{
					if ((personalCars[i].m_State & PersonalCarFlags.DummyTraffic) > 0)
					{
						++dummyTraffic;
						continue;
					}

					++personalVehicles;
				}
				else if (hasDeliveryTruck)
				{
					if ((deliveryTrucks[i].m_State & DeliveryTruckFlags.DummyTraffic) > 0)
					{
						++dummyTraffic;
						continue;
					}

					++deliveryTraffic;
				}
				else if (hasPublicTransport)
				{
					if ((publicTransports[i].m_State & PublicTransportFlags.DummyTraffic) > 0)
					{
						++dummyTraffic;
						continue;
					}
				}
				else if (hasCargo)
				{
					if ((cargoTransports[i].m_State & CargoTransportFlags.DummyTraffic) > 0)
					{
						++dummyTraffic;
						continue;
					}
				}

				if (hasAmbulance && (ambulances[i].m_State & validAmbulances) > 0)
				{
					++dispatchedAmbulances;
				}

				++realTraffic;
			}

			this.dummyTraffic.Increment(dummyTraffic);
			this.realTraffic.Increment(realTraffic);
			this.deliveryTraffic.Increment(deliveryTraffic);
			this.parkedVehicles.Increment(parkedVehicles);
			this.personalVehicles.Increment(personalVehicles);
			this.dispatchedAmbulances.Increment(dispatchedAmbulances);
		}
	}
}
