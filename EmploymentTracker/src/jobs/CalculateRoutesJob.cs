using Colossal;
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
		public ComponentLookup<HumanCurrentLane> humanLaneLookup;
		[ReadOnly]
		public ComponentLookup<CarCurrentLane> carLaneLookup;
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
		public NativeArray<NativeHashMap<CurveDef, int>> results;
		public NativeCounter.Concurrent curveCounter;

		public void Execute(int start, int count)
		{
			int batchIndex = start / batchSize;

			int added = 0;
			for (int i = start; i < start + count; ++i)
			{
				added += this.writeEntityRoute(this.input[i], batchIndex);
			}

			if (added > 0)
			{
				this.curveCounter.Increment(added);
			}
		}

		private int writeEntityRoute(Entity entity, int batchIndex)
		{
			if (!this.isValidEntity(entity))
			{
				return 0;
			}

			int added = 0;

			if (this.pathOnwerLookup.TryGetComponent(entity, out PathOwner pathOwner))
			{
				if (this.pathElementLookup.TryGetBuffer(entity, out DynamicBuffer<PathElement> pathElements))
				{
					for (int i = pathOwner.m_ElementIndex; i < pathElements.Length; ++i)
					{
						PathElement element = pathElements[i];
						if (this.curveLookup.TryGetComponent(element.m_Target, out Curve curve))
						{
							this.write(this.getCurveDef(element.m_Target, curve.m_Bezier, element.m_TargetDelta, true), batchIndex);
							++added;
						}
						else if (this.ownerLookup.TryGetComponent(element.m_Target, out Owner owner))
						{
							if (this.incomingRoutesTransit && this.routeLaneLookup.HasComponent(element.m_Target) &&
								this.getTransitWaypoints(element.m_Target, pathElements, i, out i, out Waypoint waypoint1, out Waypoint waypoint2))
							{
								if (i >= pathOwner.m_ElementIndex)
								{
									if (this.routeSegmentLookup.TryGetBuffer(owner.m_Owner, out DynamicBuffer<RouteSegment> routeSegmentBuffer))
									{
										bool wrapAround = waypoint1.m_Index > waypoint2.m_Index;

										if (wrapAround)
										{
											added += this.getTrackRouteCurves(batchIndex, waypoint1.m_Index, routeSegmentBuffer.Length, routeSegmentBuffer, 3);
											added += this.getTrackRouteCurves(batchIndex, 0, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, 3);
										}
										else
										{
											added += this.getTrackRouteCurves(batchIndex, waypoint1.m_Index, math.min(waypoint2.m_Index, routeSegmentBuffer.Length), routeSegmentBuffer, 3);
										}
									}
								}
							}
						}
					}
				}
			}

			added += this.getRouteNavigationCurves(entity, batchIndex);

			return added;
		}

		private int getTrackRouteCurves(int batchIndex, int startSegment, int endSegment, DynamicBuffer<RouteSegment> routeSegmentBuffer, byte type = 3)
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
							this.write(new CurveDef(curve.m_Bezier, type), batchIndex);
							++writeCount;
						}
					}
				}
			}

			return writeCount;
		}

		private int getRouteNavigationCurves(Entity entity, int batchIndex)
		{
			int writeCount = 0;
			if (this.carNavigationLaneSegmentLookup.TryGetBuffer(entity, out DynamicBuffer<CarNavigationLane> pathElements))
			{
				/*Bezier4x3 prevCurve = default;
				for (int i = 0; i < pathElements.Length; i++)
				{
					if (this.curveLookup.TryGetComponent(pathElements[i].m_Lane, out Curve curve))
					{
						//this.write(this.getCurveDef(pathElements[i].m_Lane, curve.m_Bezier, pathElements[i].m_CurvePosition, true), batchIndex);
						if (i > 0)
						{
							this.write(new CurveDef(MathUtils.Join(prevCurve, curve.m_Bezier), 4), batchIndex);
						}
						prevCurve = MathUtils.Cut(curve.m_Bezier, pathElements[i].m_CurvePosition);
						this.write(new CurveDef(prevCurve, 4), batchIndex);
						++writeCount;
					}
				}*/
				Bezier4x3 prevCurve = default;
				for (int i = 0; i < pathElements.Length; i++)
				{
					if (this.curveLookup.TryGetComponent(pathElements[i].m_Lane, out Curve curve))
					{
						this.write(new CurveDef(MathUtils.Cut(curve.m_Bezier, pathElements[i].m_CurvePosition), 4), batchIndex);
						++writeCount;
					}
				}
			}
			if (this.humanLaneLookup.TryGetComponent(entity, out HumanCurrentLane humanLane) && this.curveLookup.TryGetComponent(humanLane.m_Lane, out Curve humanCurve))
			{
				this.write(new CurveDef(MathUtils.Cut(humanCurve.m_Bezier, humanLane.m_CurvePosition), 2), batchIndex);
				++writeCount;
			}
			if (this.carLaneLookup.TryGetComponent(entity, out CarCurrentLane carLane) && this.curveLookup.TryGetComponent(carLane.m_Lane, out Curve carCurve))
			{
				//this.write(new CurveDef(carCurve.m_Bezier, 4), batchIndex);
				this.write(new CurveDef(MathUtils.Cut(carCurve.m_Bezier, carLane.m_CurvePosition.xy), 4), batchIndex);
				++writeCount;
			}
			
			return writeCount;
		}

		private CurveDef getCurveDef(Entity entity, Bezier4x3 curve, float2 delta, bool definitelyIsNotTransit = false)
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
			else if (!definitelyIsNotTransit && this.trackLaneLookup.HasComponent(entity))
			{
				type = 3;
			}

			return new CurveDef(MathUtils.Cut(curve, delta), type);
		}

		private bool getTransitWaypoints(Entity target, DynamicBuffer<PathElement> pathElements, int startIndex, out int endIndex, out Waypoint waypoint1, out Waypoint waypoint2)
		{
			if (this.waypointLookup.TryGetComponent(target, out waypoint1))
			{
				for (endIndex = startIndex + 1; endIndex < pathElements.Length; endIndex++)
				{
					if (this.waypointLookup.TryGetComponent(pathElements[endIndex].m_Target, out waypoint2))
					{
						return true;
					}
				}
			}

			waypoint1 = default;
			waypoint2 = default;
			endIndex = startIndex;
			return false;
		}

		private bool isValidEntity(Entity e)
		{
			return this.storageInfoLookup.Exists(e) && !this.deletedLookup.HasComponent(e);
		}

		private void write(CurveDef resultCurve, int batchIndex)
		{
			NativeHashMap<CurveDef, int> resultCurves = this.results[batchIndex];

			if (resultCurves.ContainsKey(resultCurve))
			{
				++resultCurves[resultCurve];
			}
			else
			{
				resultCurves[resultCurve] = 1;
			}
		}
	}
}
