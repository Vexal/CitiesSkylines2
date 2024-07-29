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
		public ComponentTypeHandle<Controller> controllerHandle;
		[ReadOnly]
		public EntityTypeHandle entityHandle;
		[ReadOnly]
		public ComponentLookup<Game.Net.OutsideConnection> outsideConnectionLookup;
		[ReadOnly]
		public BufferTypeHandle<Passenger> passengerHandle;

		public NativeCounter.Concurrent dummyTraffic;
		public NativeCounter.Concurrent realTraffic;
		public NativeCounter.Concurrent deliveryTraffic;
		public NativeCounter.Concurrent parkedVehicles;
		public NativeCounter.Concurrent personalVehicles;
		public NativeCounter.Concurrent dispatchedAmbulances;
		public NativeCounter.Concurrent personalVehiclePassengers;
		public NativeCounter.Concurrent publicTransitPassengers;
		public NativeCounter.Concurrent taxiPassengers;
		public NativeCounter.Concurrent taxiCount;

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
			bool hasPassengers = chunk.Has<Passenger>();
			bool hasTrailer = chunk.Has<CarTrailer>();
			bool hasController = chunk.Has<Controller>();
			bool hasTaxi = chunk.Has<Taxi>();

			NativeArray<PersonalCar> personalCars = hasPersonalCar ? chunk.GetNativeArray(ref this.personalCarHandle) : default;
			NativeArray<ParkedCar> parkedCars = hasParked ? chunk.GetNativeArray(ref this.parkedCarHandle) : default;
			NativeArray<DeliveryTruck> deliveryTrucks = hasDeliveryTruck ? chunk.GetNativeArray(ref this.deliveryTruckHandle) : default;
			NativeArray<Ambulance> ambulances = hasAmbulance ? chunk.GetNativeArray(ref this.ambualnceHandle) : default;
			NativeArray<PublicTransport> publicTransports = hasPublicTransport ? chunk.GetNativeArray(ref this.publicTransoprtHandle) : default;
			NativeArray<CargoTransport> cargoTransports = hasCargo ? chunk.GetNativeArray(ref this.cargoTransportHandle) : default;
			NativeArray<Controller> controllers = hasController ? chunk.GetNativeArray(ref this.controllerHandle) : default;
			BufferAccessor<Passenger> passengers = hasPassengers ? chunk.GetBufferAccessor(ref this.passengerHandle) : default;

			NativeArray<Entity> entities = hasController ? chunk.GetNativeArray(this.entityHandle) : default;

			int dummyTraffic = 0;
			int realTraffic = 0;
			int deliveryTraffic = 0;
			int parkedVehicles = 0;
			int personalVehicles = 0;
			int dispatchedAmbulances = 0;
			int personalVehiclePassengers = 0;
			int publicTransitPassengers = 0;
			int taxiPassengers = 0;
			int taxiCount = 0;

			var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (enumerator.NextEntityIndex(out int i))
			{
				if (hasParked)
				{
					if (!hasTrailer && !this.outsideConnectionLookup.HasComponent(parkedCars[i].m_Lane))
					{
						++parkedVehicles;
					}

					continue;
				}

				if (hasUnspawned)
				{
					continue;
				}

				bool isLeader = true;
				if (hasPersonalCar)
				{
					if ((personalCars[i].m_State & PersonalCarFlags.DummyTraffic) > 0)
					{
						if (!hasTrailer)
						{
							++dummyTraffic;
						}

						continue;
					}

					if (!hasTrailer)
					{
						++personalVehicles;
					}

					if (hasPassengers)
					{
						personalVehiclePassengers += passengers[i].Length;
					}
				}

				if (hasTrailer)
				{
					continue;
				}
				
				if (hasDeliveryTruck)
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
					isLeader = !hasController || entities[i] == controllers[i].m_Controller;
					if ((publicTransports[i].m_State & PublicTransportFlags.DummyTraffic) > 0)
					{
						if (!isLeader)
						{
							++dummyTraffic;
						}

						continue;
					}

					if (hasPassengers)
					{
						publicTransitPassengers += passengers[i].Length;
					}
				}
				else if (hasTaxi)
				{
					if (hasPassengers)
					{
						taxiPassengers += passengers[i].Length;
					}

					taxiCount++;
				}
				else if (hasCargo)
				{
					isLeader = !hasController || entities[i] == controllers[i].m_Controller;
					if ((cargoTransports[i].m_State & CargoTransportFlags.DummyTraffic) > 0)
					{
						if (isLeader)
						{
							++dummyTraffic;
						}

						continue;
					}
				}

				if (hasAmbulance && (ambulances[i].m_State & validAmbulances) > 0)
				{
					++dispatchedAmbulances;
				}

				if (isLeader)
				{
					++realTraffic;
				}
			}

			this.dummyTraffic.Increment(dummyTraffic);
			this.realTraffic.Increment(realTraffic);
			this.deliveryTraffic.Increment(deliveryTraffic);
			this.parkedVehicles.Increment(parkedVehicles);
			this.personalVehicles.Increment(personalVehicles);
			this.dispatchedAmbulances.Increment(dispatchedAmbulances);
			this.personalVehiclePassengers.Increment(personalVehiclePassengers);
			this.publicTransitPassengers.Increment(publicTransitPassengers);
			this.taxiCount.Increment(taxiCount);
			this.taxiPassengers.Increment(taxiPassengers);
		}
	}
}
