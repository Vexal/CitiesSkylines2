using Colossal.Mathematics;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Pathfind;
using Game.Routes;
using Game.Vehicles;
using Unity.Burst;
using Unity.Collections;
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

		[NativeDisableParallelForRestriction]
		public NativeStream.Writer results;

		public void Execute(int start, int count)
		{
			int batchIndex = start / batchSize;

			results.BeginForEachIndex(batchIndex);

			for (int i = start; i < start + count; ++i)
			{
				this.writeEntityRoute(this.input[i]);
			}

			results.EndForEachIndex();
		}

		private void writeEntityRoute(Entity entity)
		{
			if (!this.isValidEntity(entity))
			{
				return;
			}

			if (this.pathOnwerLookup.TryGetComponent(entity, out PathOwner pathOwner))
			{
				if (this.pathElementLookup.TryGetBuffer(entity, out DynamicBuffer<PathElement> pathElements))
				{
					for (int i = pathOwner.m_ElementIndex; i < pathElements.Length; ++i)
					{
						PathElement element = pathElements[i];
						if (this.curveLookup.TryGetComponent(element.m_Target, out Curve curve))
						{
							this.results.Write(this.getCurveDef(element.m_Target, curve.m_Bezier, element.m_TargetDelta));

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
											this.getTrackRouteCurves(waypoint1.m_Index, routeSegmentBuffer.Length, routeSegmentBuffer, 3);
											this.getTrackRouteCurves(0, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, 3);
										}
										else
										{
											this.getTrackRouteCurves(waypoint1.m_Index, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, 3);
										}
									}
								}
							}
						}
					}
				}
			}

			this.getRouteNavigationCurves(entity);
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
							this.results.Write(new CurveDef(curve.m_Bezier, type));
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
						this.results.Write(this.getCurveDef(pathElements[i].m_Lane, curve.m_Bezier, pathElements[i].m_CurvePosition));
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

			return new CurveDef(MathUtils.Cut(curve, delta), type);
		}

		private bool isValidEntity(Entity e)
		{
			return this.storageInfoLookup.Exists(e) && !this.deletedLookup.HasComponent(e);
		}
	}
}
