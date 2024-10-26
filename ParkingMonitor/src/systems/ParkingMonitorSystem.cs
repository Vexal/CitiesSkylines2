using Game;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Pathfind;
using Game.Simulation;
using Game.Tools;
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

namespace ParkingMonitor
{
	public partial class ParkingMonitorSystem : GameSystemBase
	{
		private EntityCommandBufferSystem entityCommandBufferSystem;
		private TimeSystem timeSystem;
		private EntityQuery movingVehiclesQuery;
		private EntityQuery obsoleteParkingQuery;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.entityCommandBufferSystem = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			this.timeSystem = World.GetOrCreateSystemManaged<TimeSystem>();

			this.movingVehiclesQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Vehicle>(),
				ComponentType.ReadOnly<PersonalCar>(),
				ComponentType.ReadOnly<Target>(),
			},
				Any = new ComponentType[]
			{
				ComponentType.ReadOnly<PathOwner>(),
				ComponentType.ReadOnly<PathElement>(),
				ComponentType.ReadOnly<CarNavigationLane>(),

			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>(),
				ComponentType.ReadOnly<ParkedCar>(),
				ComponentType.ReadOnly<Unspawned>(),
				}
			});

			this.obsoleteParkingQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<ParkingTarget>(),
			},
				Any = new ComponentType[]
			{
				ComponentType.ReadOnly<ParkedCar>(),
				ComponentType.ReadOnly<Unspawned>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>(),
				}
			});
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
		}

		protected override void OnUpdate()
		{
			DateTime currentTime = this.timeSystem.GetCurrentDateTime();
			long timeTicks = currentTime.Ticks;

			FindParkingTargetsJob job = new FindParkingTargetsJob { 
				commandBuffer = this.entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
				targetTypeHandle = SystemAPI.GetComponentTypeHandle<Target>(),
				entityHandle = SystemAPI.GetEntityTypeHandle(),
				pathOwnerHandle = SystemAPI.GetComponentTypeHandle<PathOwner>(),
				parkingTargetHandle = SystemAPI.GetComponentTypeHandle<ParkingTarget>(),
				pathElementHandle = SystemAPI.GetBufferTypeHandle<PathElement>(),
				carNavigationHandle = SystemAPI.GetBufferTypeHandle<CarNavigationLane>(),
				ownerLookup = SystemAPI.GetComponentLookup<Owner>(),
				parkingFacilityLookup = SystemAPI.GetComponentLookup<ParkingFacility>(),
				parkingLaneLookup = SystemAPI.GetComponentLookup<ParkingLane>(),
				failedParkingAttemptsLookup = SystemAPI.GetBufferLookup<FailedParkingAttempt>(),
				currentTime = timeTicks
			};

			base.Dependency = JobChunkExtensions.ScheduleParallel(job, this.movingVehiclesQuery, base.Dependency);
			this.entityCommandBufferSystem.AddJobHandleForProducer(base.Dependency);

			NativeArray<Entity> parkedCars = this.obsoleteParkingQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity e in parkedCars)
			{
				EntityManager.RemoveComponent<ParkingTarget>(e);
				EntityManager.AddComponent<Updated>(e);
			}
		}
	}

	//[BurstCompile]
	public struct FindParkingTargetsJob : IJobChunk
	{
		public EntityCommandBuffer.ParallelWriter commandBuffer;

		[ReadOnly]
		public EntityTypeHandle entityHandle;
		[ReadOnly]
		public ComponentTypeHandle<PathOwner> pathOwnerHandle;
		[ReadOnly]
		public ComponentTypeHandle<ParkingTarget> parkingTargetHandle;
		[ReadOnly]
		public ComponentTypeHandle<Target> targetTypeHandle;
		[ReadOnly]
		public BufferTypeHandle<PathElement> pathElementHandle;
		[ReadOnly]
		public BufferTypeHandle<CarNavigationLane> carNavigationHandle;
		[ReadOnly]
		public ComponentLookup<Owner> ownerLookup;
		[ReadOnly]
		public ComponentLookup<ParkingFacility> parkingFacilityLookup;
		[ReadOnly]
		public ComponentLookup<ParkingLane> parkingLaneLookup;
		public BufferLookup<FailedParkingAttempt> failedParkingAttemptsLookup;
		[ReadOnly]
		public long currentTime;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetTypeHandle);

			bool hasPathfind = chunk.Has<PathOwner>() && chunk.Has<PathElement>();
			bool hasCarLanes = chunk.Has<CarNavigationLane>();

			NativeArray<PathOwner> pathOwners = hasPathfind ? chunk.GetNativeArray(ref this.pathOwnerHandle) : default;
			BufferAccessor<PathElement> pathElementsAccessor = hasPathfind ? chunk.GetBufferAccessor(ref this.pathElementHandle) : default;
			BufferAccessor<CarNavigationLane> carNavigationLaneAccessor = hasCarLanes ? chunk.GetBufferAccessor(ref this.carNavigationHandle) : default;
			
			NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);

			bool alreadyHasParkingTarget = chunk.Has<ParkingTarget>();

			NativeArray<ParkingTarget> parkingTargets = alreadyHasParkingTarget ? chunk.GetNativeArray(ref this.parkingTargetHandle) : default;

			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

			while (chunkIterator.NextEntityIndex(out int i))
			{
				bool foundParking = false;
				Entity pathingTarget = default;
				if (hasCarLanes)
				{
					DynamicBuffer<CarNavigationLane> carNavigationLanes = carNavigationLaneAccessor[i];
					for (int pathIndex = 0; pathIndex < carNavigationLanes.Length; pathIndex++)
					{
						CarNavigationLane navigationLane = carNavigationLanes[pathIndex];
						if (this.ownerLookup.TryGetComponent(navigationLane.m_Lane, out Owner owner))
						{
							if (this.parkingFacilityLookup.HasComponent(owner.m_Owner) || this.parkingLaneLookup.HasComponent(navigationLane.m_Lane))
							{
								pathingTarget = owner.m_Owner;
								foundParking = true;
								break;
							}
						}
					}
				}

				if (hasPathfind && !foundParking)
				{
					PathOwner pathOwner = pathOwners[i];
					DynamicBuffer<PathElement> pathElements = pathElementsAccessor[i];

					for (int pathIndex = pathOwner.m_ElementIndex; pathIndex < pathElements.Length; pathIndex++)
					{
						PathElement pathElement = pathElements[pathIndex];
						if (pathElement.m_Target != null && pathElement.m_Target != Entity.Null && this.ownerLookup.TryGetComponent(pathElement.m_Target, out Owner owner))
						{
							if (this.parkingFacilityLookup.HasComponent(owner.m_Owner) || this.parkingLaneLookup.HasComponent(pathElement.m_Target))
							{
								pathingTarget = owner.m_Owner;
								foundParking = true;
								break;
							}
						}
					}
				}

				if (foundParking)
				{
					while (this.ownerLookup.TryGetComponent(pathingTarget, out Owner newOwner))
					{
						pathingTarget = newOwner.m_Owner;
					}

					if (alreadyHasParkingTarget)
					{
						if (parkingTargets[i].currentTarget != pathingTarget)
						{
							ParkingTarget parkingTarget = parkingTargets[i];
							parkingTarget.currentTarget = pathingTarget;
							if (parkingTarget.currentDestination == Entity.Null)
							{
								//for backwards compatibility
								parkingTarget.attemptCount += 1;
								parkingTarget.currentDestination = targets[i].m_Target;
							}
							else if (parkingTarget.currentDestination == targets[i].m_Target)
							{
								parkingTarget.attemptCount += 1;
								if (parkingTarget.attemptCount > 1)
								{
									DynamicBuffer<FailedParkingAttempt> parkingAttemptBuffer;
									//if (!this.failedParkingAttemptsLookup.TryGetBuffer(pathingTarget, out parkingAttemptBuffer))
									if (!this.failedParkingAttemptsLookup.HasBuffer(pathingTarget))
									{
										parkingAttemptBuffer = this.commandBuffer.AddBuffer<FailedParkingAttempt>(unfilteredChunkIndex, pathingTarget);
									}

									//parkingAttemptBuffer.Add(new FailedParkingAttempt(this.currentTime));
									this.commandBuffer.AppendToBuffer(unfilteredChunkIndex, pathingTarget, new FailedParkingAttempt(this.currentTime));
								}
							}
							else
							{
								parkingTarget.attemptCount = 1;
								parkingTarget.currentDestination = targets[i].m_Target;
							}

							parkingTargets[i] = parkingTarget;
						}
					}
					else
					{
						this.commandBuffer.AddComponent(unfilteredChunkIndex, entities[i], new ParkingTarget(pathingTarget, 1, targets[i].m_Target));
					}
				}
			}
		}

		//private void getOwningBuilding
	}
}
