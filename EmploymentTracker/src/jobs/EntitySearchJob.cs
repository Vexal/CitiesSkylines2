using Game.Buildings;
using Game.Common;
using Game.Creatures;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EmploymentTracker
{
	[BurstCompile]
	public struct EntitySearchJob : IJobChunk
	{
		[ReadOnly]
		public Entity searchTarget;
		/*[ReadOnly]
		public ComponentLookup<Target> targetLookup;
		[ReadOnly]
		public ComponentLookup<PublicTransport> publicTransportLookup;
		[ReadOnly]
		public ComponentLookup<Train> trainLookup;
		[ReadOnly]
		public ComponentLookup<Watercraft> boatLookup;
		[ReadOnly]
		public ComponentLookup<Airplane> planeLookup;
		[ReadOnly]
		public ComponentLookup<Car> carLookup;
		[ReadOnly]
		public ComponentLookup<Building> buildingLookup;
		[ReadOnly]
		public ComponentLookup<Human> humanLookup;
		[ReadOnly]
		public ComponentLookup<Animal> animalLookup;
		[ReadOnly]
		public ComponentLookup<CurrentVehicle> currentVehicleLookup;*/

		[ReadOnly]
		public ComponentTypeHandle<Target> targetHandle;
		[ReadOnly]
		public EntityTypeHandle entityHandle;

		public NativeList<Entity> results;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetHandle);
			NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);
			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
			while (chunkIterator.NextEntityIndex(out var i))
			{
				Target entityTarget = targets[i];
				if (entityTarget.m_Target == this.searchTarget)
				{
					this.results.Add(entities[i]);
				}
			}

			/*for (int i = 0; i < chunk.Count; i++)
			{
				Target entityTarget = targets[i];
				if (entityTarget.m_Target == this.searchTarget)
				{
					this.results.Add(entities[i]);
				}
			}*/
		}

		/*private SelectionType getEntityRouteType(Entity e)
		{
			if (this.publicTransportLookup.HasComponent(e))
			{
				return this.getTransitType(e);
			}
			if (this.buildingLookup.HasComponent(e))
			{
				return SelectionType.BUILDING;
			}
			else if (this.carLookup.HasComponent(e))
			{
				return SelectionType.CAR;
			}
			else if (this.humanLookup.HasComponent(e))
			{
				if (this.currentVehicleLookup.TryGetComponent(e, out CurrentVehicle currentVehicle))
				{
					if (this.publicTransportLookup.HasComponent(currentVehicle.m_Vehicle))
					{
						return this.getTransitType(currentVehicle.m_Vehicle);
					}
					else if (this.carLookup.HasComponent(currentVehicle.m_Vehicle))
					{
						return SelectionType.CAR_OCCUPANT;
					}
				}
				else
				{
					return SelectionType.HUMAN;
				}
			}
			else if (this.animalLookup.HasComponent(e))
			{
				return SelectionType.ANIMAL;
			}

			return SelectionType.UNKNOWN;
		}

		private SelectionType getTransitType(Entity e)
		{
			if (this.trainLookup.HasComponent(e))
			{
				return SelectionType.TRAIN;
			}
			else if (this.boatLookup.HasComponent(e))
			{
				return SelectionType.BOAT;
			}
			else if (this.planeLookup.HasComponent(e))
			{
				return SelectionType.AIRPLANE;
			}
			else if (this.carLookup.HasComponent(e))
			{
				return SelectionType.TRANSIT;
			}

			return SelectionType.UNKNOWN;
		}*/

		/*private SelectionType getEntityRouteType(Entity e)
		{
			if (this.publicTransportLookup.HasComponent(e))
			{
				return this.getTransitType(e);
			}
			if (this.buildingLookup.HasComponent(e))
			{
				return SelectionType.BUILDING;
			}
			else if (this.carLookup.HasComponent(e))
			{
				return SelectionType.CAR;
			}
			else if (this.humanLookup.HasComponent(e))
			{
				if (this.currentVehicleLookup.TryGetComponent(e, out CurrentVehicle currentVehicle))
				{
					if (this.publicTransportLookup.HasComponent(currentVehicle.m_Vehicle))
					{
						return this.getTransitType(currentVehicle.m_Vehicle);
					}
					else if (this.carLookup.HasComponent(currentVehicle.m_Vehicle))
					{
						return SelectionType.CAR_OCCUPANT;
					}
				}
				else
				{
					return SelectionType.HUMAN;
				}
			}
			else if (this.animalLookup.HasComponent(e))
			{
				return SelectionType.ANIMAL;
			}

			return SelectionType.UNKNOWN;
		}

		private SelectionType getTransitType(Entity e)
		{
			if (this.trainLookup.HasComponent(e))
			{
				return SelectionType.TRAIN;
			}
			else if (this.boatLookup.HasComponent(e))
			{
				return SelectionType.BOAT;
			}
			else if (this.planeLookup.HasComponent(e))
			{
				return SelectionType.AIRPLANE;
			}
			else if (this.carLookup.HasComponent(e))
			{
				return SelectionType.TRANSIT;
			}

			return SelectionType.UNKNOWN;
		}*/
	}
}
