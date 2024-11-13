using Colossal;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Pathfind;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace ParkingMonitor
{
	public partial class ParkingMonitorSystem : UISystemBase
	{
		private EntityCommandBufferSystem entityCommandBufferSystem;
		private TimeSystem timeSystem;
		private EntityQuery movingVehiclesQuery;
		private EntityQuery parkingFinderVehicleQuery;
		private EntityQuery obsoleteParkingQuery;
		private EntityQuery missingBufferParkingQuery;
		private ToolSystem toolSystem;
		private NameSystem nameSystem;
		private ValueBinding<string> dataBindings;
		private ValueBinding<string> parkingBindings;
		private ValueBinding<string> parkingType;
		private ValueBinding<string> districtSortOrder;
		private ValueBinding<int> defaultRowsPerDistrict;
		private ValueBinding<bool> autoRefreshActiveBinding;
		private ValueBinding<bool> enabledBinding;
		private Dictionary<string, string> dataValues = new Dictionary<string, string>();
		private List<string> dataOrderings = new List<string>();

		private bool clearNext = false;
		protected override void OnCreate()
		{
			base.OnCreate();
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			this.nameSystem = World.GetOrCreateSystemManaged<NameSystem>();

			this.dataBindings = new ValueBinding<string>("ParkingMonitor", "dataBindings", "");
			AddBinding(this.dataBindings);

			this.parkingBindings = new ValueBinding<string>("ParkingMonitor", "parkingBindings", "");
			AddBinding(this.parkingBindings);

			bool defaultAutoRefresh = Mod.INSTANCE.m_Setting.initialState == Setting.InitialValue.ACTIVE;
			bool defaultEnabled = Mod.INSTANCE.m_Setting.initialState != Setting.InitialValue.STOPPED;
			this.autoRefreshActiveBinding = new ValueBinding<bool>("ParkingMonitor", "autoRefreshActive", defaultAutoRefresh);
			AddBinding(this.autoRefreshActiveBinding);

			this.enabledBinding = new ValueBinding<bool>("ParkingMonitor", "enabled", defaultEnabled);
			AddBinding(this.enabledBinding);

			this.parkingType = new ValueBinding<string>("ParkingMonitor", "parkingType", "failedParking");
			AddBinding(this.parkingType);

			this.districtSortOrder = new ValueBinding<string>("ParkingMonitor", "districtSortOrder", Mod.INSTANCE.m_Setting.districtSortOrder.ToString());
			AddBinding(this.districtSortOrder);

			this.defaultRowsPerDistrict = new ValueBinding<int>("ParkingMonitor", "defaultRowsPerDistrict", Mod.INSTANCE.m_Setting.defaultRowsPerDistrict);
			AddBinding(this.defaultRowsPerDistrict);

			this.entityCommandBufferSystem = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			this.timeSystem = World.GetOrCreateSystemManaged<TimeSystem>();

			AddBinding(new TriggerBinding<string>("ParkingMonitor", "selectLot", s => {
				string[] entityDef = s.Split(':');
				Entity selected =new Entity{ Index = int.Parse(entityDef[0]), Version = int.Parse(entityDef[1])};

				if (EntityManager.Exists(selected))
				{
					this.toolSystem.selected = selected;
				}
			}));

			AddBinding(new TriggerBinding<string>("ParkingMonitor", "setParkingType", s => {
				this.parkingType.Update(s);
			}));
			
			AddBinding(new TriggerBinding<bool>("ParkingMonitor", "clear", s => {
				this.clearNext = true;
			}));
			
			AddBinding(new TriggerBinding<bool>("ParkingMonitor", "pause", s => {
				this.autoRefreshActiveBinding.Update(s);
				this.enabledBinding.Update(true);
			}));
			
			AddBinding(new TriggerBinding<bool>("ParkingMonitor", "enable", s => {
				if (!s)
				{
					this.clearNext = true;
				}

				this.autoRefreshActiveBinding.Update(true);

				this.enabledBinding.Update(s);
			}));

			Mod.INSTANCE.m_Setting.onSettingsApplied += setting =>
			{
				if (setting is Setting)
				{
					Setting s = (Setting) setting;
					this.districtSortOrder.Update(s.districtSortOrder.ToString());
					this.defaultRowsPerDistrict.Update(s.defaultRowsPerDistrict);
				}
			};

			this.movingVehiclesQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Vehicle>(),
				ComponentType.ReadOnly<PersonalCar>(),
				ComponentType.ReadOnly<Target>(),
				ComponentType.ReadOnly<ParkingTarget>(),
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

			this.parkingFinderVehicleQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<ParkingTarget>(),
			},
				Any = new ComponentType[]
			{

			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
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

			this.missingBufferParkingQuery = GetEntityQuery(new EntityQueryDesc
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
				ComponentType.ReadOnly<CarCurrentLane>(),

			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Building>(),
				ComponentType.ReadOnly<ParkedCar>(),
				ComponentType.ReadOnly<Unspawned>(),
				ComponentType.ReadOnly<ParkingTarget>()
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

		private int parkingCount = 0;
		private int totalVehicles = 0;
		private ulong frameCount = 0;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (this.clearNext)
			{
				this.clearNext = false;
				foreach (var e in this.parkingFinderVehicleQuery.ToEntityArray(Allocator.Temp))
				{
					EntityManager.RemoveComponent<ParkingTarget>(e);
					//EntityManager.AddComponent<Updated>(e);
				}

				this.updateParkingCounts();

				return;
			}

			if (!this.enabledBinding.value)
			{
				return;
			}

			DateTime currentTime = this.timeSystem.GetCurrentDateTime();
			long timeTicks = currentTime.Ticks;

			foreach (Entity e in this.obsoleteParkingQuery.ToEntityArray(Allocator.Temp))
			{
				EntityManager.RemoveComponent<ParkingTarget>(e);
				//EntityManager.AddComponent<Updated>(e);
			}

			foreach (var e in this.missingBufferParkingQuery.ToEntityArray(Allocator.Temp))
			{
				EntityManager.AddBuffer<ParkingTarget>(e);
				//EntityManager.AddComponent<Updated>(e);
			}
			

			FindParkingTargetsJob job = new FindParkingTargetsJob { 
				commandBuffer = this.entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
				targetTypeHandle = SystemAPI.GetComponentTypeHandle<Target>(true),
				entityHandle = SystemAPI.GetEntityTypeHandle(),
				pathOwnerHandle = SystemAPI.GetComponentTypeHandle<PathOwner>(true),
				pathElementHandle = SystemAPI.GetBufferTypeHandle<PathElement>(true),
				carNavigationHandle = SystemAPI.GetBufferTypeHandle<CarNavigationLane>(true),
				ownerLookup = SystemAPI.GetComponentLookup<Owner>(true),
				parkingFacilityLookup = SystemAPI.GetComponentLookup<ParkingFacility>(true),
				parkingLaneLookup = SystemAPI.GetComponentLookup<ParkingLane>(true),
				failedParkingAttemptsLookup = SystemAPI.GetBufferTypeHandle<ParkingTarget>(),
				currentTime = timeTicks
			};

			base.Dependency = JobChunkExtensions.ScheduleParallel(job, this.movingVehiclesQuery, base.Dependency);
			this.entityCommandBufferSystem.AddJobHandleForProducer(base.Dependency);
			base.Dependency.Complete();

			if (++this.frameCount % 60 == 0 && this.autoRefreshActiveBinding.value)
			{
				this.updateParkingCounts();
			}
		}

		private void updateParkingCounts()
		{
			NativeArray<Entity> drivingVehicles = this.parkingFinderVehicleQuery.ToEntityArray(Allocator.Temp);
			int vehiclesWithMultipleAttempts = 0;
			int newTotalVehicles = 0;
			NativeHashMap<Entity, int> attemptCount = new NativeHashMap<Entity, int>(128, Allocator.Temp);

			bool countFailures = this.parkingType.value == "failedParking";

			foreach (Entity e in drivingVehicles)
			{
				if (EntityManager.TryGetBuffer<ParkingTarget>(e, true, out var parkingTargets) && parkingTargets.Length > 0)
				{
					++newTotalVehicles;
					if (parkingTargets.Length > 1)
					{
						++vehiclesWithMultipleAttempts;
					}

					if (attemptCount.IsCreated && !parkingTargets.IsEmpty)
					{
						if (countFailures && parkingTargets.Length > 1)
						{
							for (int i = 0; i < parkingTargets.Length - 1; ++i)
							{
								if (attemptCount.ContainsKey(parkingTargets[i].currentTarget))
								{
									attemptCount[parkingTargets[i].currentTarget]++;
								}
								else
								{
									attemptCount[parkingTargets[i].currentTarget] = 1;
								}
							}

						}
						else if (!countFailures)
						{
							if (attemptCount.ContainsKey(parkingTargets[parkingTargets.Length - 1].currentTarget))
							{
								attemptCount[parkingTargets[parkingTargets.Length - 1].currentTarget]++;
							}
							else
							{
								attemptCount[parkingTargets[parkingTargets.Length - 1].currentTarget] = 1;
							}
						}
					}
				}
			}

			if (attemptCount.IsCreated)
			{
				List<ParkingLot> tmpParkingList = new List<ParkingLot>(attemptCount.Count);
				foreach (var parkingLot in attemptCount)
				{
					tmpParkingList.Add(new ParkingLot { entity = parkingLot.Key, count = parkingLot.Value });

				}

				tmpParkingList.Sort((a, b) => { return b.count - a.count; });
				int c = Math.Min(Mod.INSTANCE.m_Setting.parkingRowCount, tmpParkingList.Count);
				List<string> parkingStrings = new List<string>(c);
				for (int i = 0; i < c; i++)
				{
					parkingStrings.Add(this.parkingLotJson(tmpParkingList[i]));
				}

				this.parkingBindings.Update(string.Join("|", parkingStrings));
			}

			this.parkingCount = vehiclesWithMultipleAttempts;
			this.totalVehicles = newTotalVehicles;
			this.setData("Vehicles Looking For Parking", this.parkingCount);
			this.setData("Total Vehicles Planning to Park", this.totalVehicles);
			this.updateBindings();
		}

		struct ParkingLot
		{

			public Entity entity;
			public int count;
		}

		private string parkingLotJson(ParkingLot p)
		{
			int type = 0;
			if (EntityManager.HasComponent<ParkingFacility>(p.entity))
			{
				type = 1;
			}
			else if (EntityManager.HasComponent<Road>(p.entity))
			{
				type = 2;
			}
			else if (EntityManager.HasComponent<ResidentialProperty>(p.entity))
			{
				type = 3;
			}
			else if (EntityManager.HasComponent<OfficeProperty>(p.entity))
			{
				type = 4;
			}
			else if (EntityManager.HasComponent<IndustrialProperty>(p.entity))
			{
				type = 5;
			}
			else if (EntityManager.HasComponent<CommercialProperty>(p.entity))
			{
				type = 6;
			}

			string districtName = null;
			if (EntityManager.TryGetComponent<CurrentDistrict>(p.entity, out var district) && district.m_District != Entity.Null)
			{
				districtName = this.nameSystem.GetRenderedLabelName(district.m_District);
			}
			else if (EntityManager.TryGetComponent<BorderDistrict>(p.entity, out var borderDistrict))
			{
				if (borderDistrict.m_Left != Entity.Null)
				{
					districtName = this.nameSystem.GetRenderedLabelName(borderDistrict.m_Left);
				}
				else if (borderDistrict.m_Right != Entity.Null)
				{
					districtName = this.nameSystem.GetRenderedLabelName(borderDistrict.m_Right);
				}

			}

			return "{" +
				"\"key\":\"" + p.entity.Index.ToString() + ":" + p.entity.Version.ToString() + "\"" +
				",\"count\":" + p.count.ToString() +
				",\"type\":" + type.ToString() +
				",\"name\":\"" + this.nameSystem.GetRenderedLabelName(p.entity).Replace("\"", "'") + "\"" +
				(districtName != null ? ",\"district\":\"" + districtName.Replace("\"", "'") + "\"" : "") +
				"}";
		}

		private void updateBindings()
		{
			List<string> bindingList = new List<string>(this.dataValues.Count);
			foreach (var b in this.dataOrderings)
			{
				if (this.dataValues.ContainsKey(b))
				{
					bindingList.Add(b + "," + this.dataValues[b]);
				}
			}

			this.dataBindings.Update(string.Join("|", bindingList));
		}

		private void setData(string name, string value)
		{
			if (!this.dataValues.ContainsKey(name))
			{
				this.dataOrderings.Add(name);
			}

			this.dataValues[name] = value;
		}

		private void setData(string name, params int[] value)
		{
			List<string> str = new List<string>(value.Length);
			foreach (int i in value)
			{
				str.Add(i.ToString());
			}

			this.setData(name, string.Join(" / ", str));
		}

		private void setData(string name, params NativeCounter[] value)
		{
			List<string> str = new List<string>(value.Length);
			foreach (NativeCounter i in value)
			{
				str.Add(i.Count.ToString());
			}

			this.setData(name, string.Join(" / ", str));
		}
	}

	[BurstCompile]
	public struct FindParkingTargetsJob : IJobChunk
	{
		public EntityCommandBuffer.ParallelWriter commandBuffer;

		[ReadOnly]
		public EntityTypeHandle entityHandle;
		[ReadOnly]
		public ComponentTypeHandle<PathOwner> pathOwnerHandle;
		[ReadOnly]
		public ComponentTypeHandle<Target> targetTypeHandle;
		[ReadOnly]
		public ComponentTypeHandle<CarCurrentLane> carCurrentLaneHandle;
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
		public BufferTypeHandle<ParkingTarget> failedParkingAttemptsLookup;
		[ReadOnly]
		public long currentTime;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			NativeArray<Target> targets = chunk.GetNativeArray(ref this.targetTypeHandle);

			bool hasPathfind = chunk.Has<PathOwner>() && chunk.Has<PathElement>();
			bool hasCarLanes = chunk.Has<CarNavigationLane>();
			bool hasCarCurrentLane = chunk.Has<CarCurrentLane>();

			NativeArray<PathOwner> pathOwners = hasPathfind ? chunk.GetNativeArray(ref this.pathOwnerHandle) : default;
			NativeArray<CarCurrentLane> currentLanes = hasCarCurrentLane ? chunk.GetNativeArray(ref this.carCurrentLaneHandle) : default;
			BufferAccessor<PathElement> pathElementsAccessor = hasPathfind ? chunk.GetBufferAccessor(ref this.pathElementHandle) : default;
			BufferAccessor<CarNavigationLane> carNavigationLaneAccessor = hasCarLanes ? chunk.GetBufferAccessor(ref this.carNavigationHandle) : default;
			BufferAccessor<ParkingTarget> parkingTargetAccessor = chunk.GetBufferAccessor(ref this.failedParkingAttemptsLookup);
			
			NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);

			var chunkIterator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

			while (chunkIterator.NextEntityIndex(out int i))
			{
				bool foundParking = false;
				Entity parkingTarget = default;

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
								parkingTarget = owner.m_Owner;
								foundParking = true;
								break;
							}
						}
					}
				}

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
								parkingTarget = owner.m_Owner;
								foundParking = true;
								break;
							}
						}
					}
				}

				if (hasCarCurrentLane && !foundParking)
				{
					if (this.ownerLookup.TryGetComponent(currentLanes[i].m_Lane, out Owner owner))
					{
						parkingTarget = owner.m_Owner;
						foundParking = true;
					}
				}

				if (foundParking)
				{
					while (this.ownerLookup.TryGetComponent(parkingTarget, out Owner newOwner))
					{
						parkingTarget = newOwner.m_Owner;
					}


					DynamicBuffer<ParkingTarget> parkingTargets = parkingTargetAccessor[i];

					bool updated = false;
					if (parkingTargets.IsEmpty)
					{
						this.commandBuffer.AppendToBuffer(unfilteredChunkIndex, entities[i], new ParkingTarget(parkingTarget, targets[i].m_Target));
						updated = true;
					}
					else
					{
						if (parkingTargets[i].currentDestination != targets[i].m_Target)
						{
							this.commandBuffer.RemoveComponent<ParkingTarget>(unfilteredChunkIndex, entities[i]);
							this.commandBuffer.AddBuffer<ParkingTarget>(unfilteredChunkIndex, entities[i]);
							this.commandBuffer.AppendToBuffer(unfilteredChunkIndex, entities[i], new ParkingTarget(parkingTarget, targets[i].m_Target));
							updated = true;
						}
						else if (parkingTargets[parkingTargets.Length - 1].currentTarget != parkingTarget)
						{
							this.commandBuffer.AppendToBuffer(unfilteredChunkIndex, entities[i], new ParkingTarget(parkingTarget,
							targets[i].m_Target));
							updated = true;
						}
					}

					if (updated)
					{
						//this.commandBuffer.AddComponent<Updated>(unfilteredChunkIndex, entities[i]);
					}
				}
			}
		}
	}
}
