using Colossal.Mathematics;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Objects;
using Game.Pathfind;
using Game.Routes;
using Game.Tools;
using Game.Vehicles;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace EmploymentTracker
{
	[BurstCompile]
	public struct CalculateRoutesJob : IJobParallelForBatch
	{
		[ReadOnly]
		public NativeList<Entity> input;
		//public NativeArray<Entity> input;
		[ReadOnly]
		public ComponentLookup<PathOwner> pathOnwerLookup;
		[ReadOnly]
		public ComponentLookup<Curve> curveLookup;
		[ReadOnly]
		public ComponentLookup<Owner> ownerLookup;
		[ReadOnly]
		public ComponentLookup<RouteLane> routeLaneLookup;
		[ReadOnly]
		public ComponentLookup<Waypoint> waypointLookup;
		[ReadOnly]
		public ComponentLookup<PedestrianLane> pedestrianLaneLookup;
		[ReadOnly]
		public ComponentLookup<TrackLane> trackLaneLookup;
		[ReadOnly]
		public ComponentLookup<SecondaryLane> secondaryLaneLookup;
		[ReadOnly]
		public ComponentLookup<CurrentVehicle> currentVehicleLookup;
		[ReadOnly]
		public ComponentLookup<CurrentTransport> currentTransportLookup;
		[ReadOnly]
		public ComponentLookup<Deleted> deletedLookup;
		[ReadOnly]
		public ComponentLookup<Unspawned> unspawnedLookup;
		[ReadOnly]
		public EntityStorageInfoLookup storageInfoLookup;
		[ReadOnly]
		public BufferLookup<PathElement> pathElementLookup;
		[ReadOnly]
		public BufferLookup<RouteSegment> routeSegmentLookup;
		[ReadOnly]
		public BufferLookup<CarNavigationLane> carNavigationLaneSegmentLookup;
		[ReadOnly]
		public int batchSize;
		[ReadOnly]
		public bool incomingRoutesTransit;
		[ReadOnly]
		public SelectionType selectionType;
		[ReadOnly]
		public Entity leader;

		[NativeSetThreadIndex]
		int threadId;

		[NativeDisableParallelForRestriction]
		public NativeStream.Writer results;

		public void Execute(int start, int count)
		{
			int batchIndex = start / batchSize;

			results.BeginForEachIndex(batchIndex);

			for (int i = start; i < start + count; ++i)
			{
				Entity entity = this.input[i];
				int writeCount = this.writeEntityRoute(entity);
				//Mod.log.Info("Entity " + i + ": " + entity.ToString() + " write count: " + writeCount + " thread id: " + this.threadId + " batch index: " + batchIndex);
			}

			results.EndForEachIndex();
		}

		private int writeEntityRoute(Entity entity)
		{
			if (!this.isValidEntity(entity))
			{
				return 0;
			}

			//Highlight the path of a selected citizen inside a vehicle
			/*if (this.currentVehicleLookup.TryGetComponent(entity, out CurrentVehicle vehicle))
			{
				return this.writeEntityRoute(vehicle.m_Vehicle);
			}
			else if (this.currentTransportLookup.TryGetComponent(entity, out CurrentTransport currentTransport))
			{
				return this.writeEntityRoute(currentTransport.m_CurrentTransport);
			}
			*/

			int writeCount = 0;

			if (this.pathOnwerLookup.TryGetComponent(entity, out PathOwner pathOwner))
			{
				if (this.pathElementLookup.TryGetBuffer(entity, out DynamicBuffer<PathElement> pathElements))
				{
					//Mod.log.Info("Path element count: " + pathElements.Length + " index: " + index + " thread id: " + this.threadId);
					
					for (int i = pathOwner.m_ElementIndex; i < pathElements.Length; ++i)
					{
						PathElement element = pathElements[i];
						if (this.curveLookup.TryGetComponent(element.m_Target, out Curve curve))
						{
							this.writeResult(this.getCurveDef(element.m_Target, curve.m_Bezier, element.m_TargetDelta), element.m_Target);
							++writeCount;
							//results.Write(this.getCurveDef(element.m_Target, curve.m_Bezier, element.m_TargetDelta));

						}
						else if (this.ownerLookup.TryGetComponent(element.m_Target, out Owner owner))
						{
							if (this.incomingRoutesTransit && this.routeLaneLookup.HasComponent(element.m_Target) &&
								i < pathElements.Length - 1 &&
								this.waypointLookup.TryGetComponent(element.m_Target, out Waypoint waypoint1) &&
								this.waypointLookup.TryGetComponent(pathElements[i + 1].m_Target, out Waypoint waypoint2))
							{
								if (i >= pathOwner.m_ElementIndex)
								{
									if (this.routeSegmentLookup.TryGetBuffer(owner.m_Owner, out DynamicBuffer<RouteSegment> routeSegmentBuffer))
									{
										bool wrapAround = waypoint1.m_Index > waypoint2.m_Index;

										if (wrapAround)
										{
											writeCount += this.getTrackRouteCurves(waypoint1.m_Index, routeSegmentBuffer.Length, routeSegmentBuffer, 3);
											writeCount += this.getTrackRouteCurves(0, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, 3);
										}
										else
										{
											writeCount += this.getTrackRouteCurves(waypoint1.m_Index, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, 3);
										}
									}
								}
							}
						}
					}
				}
			}

			writeCount += this.getRouteNavigationCurves(entity);
			return writeCount;
		}

		private int getTrackRouteCurves(int startSegment, int endSegment, DynamicBuffer<RouteSegment> routeSegmentBuffer, byte type = 3)
		{
			int writeCount = 0;
			for (int trackInd = startSegment; trackInd < endSegment; trackInd++)
			{
				RouteSegment routeSegment = routeSegmentBuffer[trackInd];
				if (this.pathElementLookup.TryGetBuffer(routeSegment.m_Segment, out DynamicBuffer<PathElement> trackCurves))
				{
					for (int i = 0; i < trackCurves.Length; i++)
					{						
						if (this.curveLookup.TryGetComponent(trackCurves[i].m_Target, out Curve curve))
						{
							//results.Write(new CurveDef(curve.m_Bezier, type));
							this.writeResult(new CurveDef(curve.m_Bezier, type));
							++writeCount;
						}
					}
				}
			}

			return writeCount;
		}

		private int getRouteNavigationCurves(Entity entity)
		{
			int writeCount = 0;
			if (this.carNavigationLaneSegmentLookup.TryGetBuffer(entity, out DynamicBuffer<CarNavigationLane> pathElements) && !pathElements.IsEmpty)
			{
				for (int i = 0; i < pathElements.Length; i++)
				{
					if (this.curveLookup.TryGetComponent(pathElements[i].m_Lane, out Curve curve))
					{
						++writeCount;
						this.writeResult(this.getCurveDef(pathElements[i].m_Lane, curve.m_Bezier, pathElements[i].m_CurvePosition));
						//results.Write(this.getCurveDef(pathElements[i].m_Lane, curve.m_Bezier, pathElements[i].m_CurvePosition));
					}
				}
			}

			return writeCount;
		}

		private CurveDef getCurveDef(Entity entity, Bezier4x3 curve, float2 delta)
		{
			byte type = 1;
			if (this.pedestrianLaneLookup.HasComponent(entity))
			{
				type = 2;
			}
			else if (this.secondaryLaneLookup.HasComponent(entity))
			{
				type = 0;
			}
			else if (this.trackLaneLookup.HasComponent(entity))
			{
				type = 3;
			}

			//if ((delta.x != 1f && delta.y != 1f) || (delta.x == 1f && delta.y == 1f))
			{
				return new CurveDef(MathUtils.Cut(curve, delta), type);
			}
			//else
			{
				//return new CurveDef(curve, type);
			}
		}

		private void writeResult(CurveDef curveDef, Entity e=default(Entity))
		{
			//Mod.log.Info("Writing " + curveDef.GetType() + " thread id: " + this.threadId + " (entity: " + e.ToString() + ")");
			results.Write<CurveDef>(curveDef);
		}

		private bool isValidEntity(Entity e)
		{
			return this.storageInfoLookup.Exists(e) && !this.deletedLookup.HasComponent(e) && !this.unspawnedLookup.HasComponent(e);
		}
	}
}
