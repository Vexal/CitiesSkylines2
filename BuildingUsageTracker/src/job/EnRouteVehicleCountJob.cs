using Colossal;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	[BurstCompile]
	public struct EnRouteVehicleCountJob : IJobChunk
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
		public EntityTypeHandle entityHandle;
		public NativeCounter.Concurrent totalCount;
		public NativeCounter.Concurrent serviceCount;
		public NativeCounter.Concurrent deliveryCount;
		public NativeCounter.Concurrent personalCarCount;
		public NativeCounter.Concurrent taxiCount;
		public NativeCounter.Concurrent otherCount;

		[ReadOnly]
		public bool returnEntities;
		public NativeList<Entity> resultEntities;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetHandle);
			NativeArray<Entity> entities = this.returnEntities ? chunk.GetNativeArray(this.entityHandle) : default;

			bool isDelivery = chunk.Has<DeliveryTruck>();
			bool isPersonalCar = chunk.Has<PersonalCar>();
			bool isTaxi = chunk.Has<Taxi>();
			bool isServiceVehicle = chunk.Has<PostVan>() || chunk.Has<PoliceCar>() || chunk.Has<Hearse>() ||
				chunk.Has<GarbageTruck>() || chunk.Has<Ambulance>() || chunk.Has<FireEngine>();

			int totalCount = 0;
			int serviceCount = 0;
			int deliveryCount = 0;
			int personalCarCount = 0;
			int taxiCount = 0;
			int otherCount = 0;
			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (chunkIterator.NextEntityIndex(out var i))
			{
				Target target = targets[i];
				if (target.m_Target == this.searchTarget || (this.hasTarget2 && target.m_Target == this.searchTarget2))
				{
					++totalCount;
					if (isServiceVehicle)
					{
						++serviceCount;
					}
					else if (isDelivery)
					{
						++deliveryCount;
					}
					else if (isPersonalCar)
					{
						++personalCarCount;
                    }
					else if (isTaxi)
					{
						++taxiCount;
					}
					else
					{
						++otherCount;
					}

					if (this.returnEntities)
					{
						this.resultEntities.Add(entities[i]);
					}
				}
			}

			this.totalCount.Increment(totalCount);
			this.serviceCount.Increment(serviceCount);
			this.deliveryCount.Increment(deliveryCount);
			this.personalCarCount.Increment(personalCarCount);
			this.taxiCount.Increment(taxiCount);
			this.otherCount.Increment(otherCount);
		}
	}
}
