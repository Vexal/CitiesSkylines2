using Colossal.Mathematics;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Net;
using Game.Pathfind;
using Game.Routes;
using Game.Vehicles;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace EmploymentTracker
{
	public struct CalculateRoutesJob : IJobParallelFor
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
		public BufferLookup<PathElement> pathElementLookup;
		[ReadOnly]
		public BufferLookup<RouteSegment> routeSegmentLookup;
		[ReadOnly]
		public BufferLookup<CarNavigationLane> carNavigationLaneSegmentLookup;

		[NativeSetThreadIndex]
		int threadId;

		public NativeStream.Writer results;

		public void Execute(int index)
		{
			results.BeginForEachIndex(this.threadId);
			Entity entity = this.input[index];

			this.writeEntityRoute(entity);

			results.EndForEachIndex();
		}

		private void writeEntityRoute(Entity entity)
		{
			//Highlight the path of a selected citizen inside a vehicle
			if (this.currentVehicleLookup.TryGetComponent(entity, out CurrentVehicle vehicle))
			{
				this.writeEntityRoute(vehicle.m_Vehicle);
				return;
			}
			else if (this.currentTransportLookup.TryGetComponent(entity, out CurrentTransport currentTransport))
			{
				this.writeEntityRoute(currentTransport.m_CurrentTransport);
				return;
			}

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
							results.Write(this.getCurveDef(element.m_Target, curve.m_Bezier, element.m_TargetDelta));

						}
						else if (this.ownerLookup.TryGetComponent(element.m_Target, out Owner owner))
						{
							if (this.routeLaneLookup.HasComponent(element.m_Target) &&
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

		private void getTrackRouteCurves(int startSegment, int endSegment, DynamicBuffer<RouteSegment> routeSegmentBuffer, byte type = 3)
		{
			for (int trackInd = startSegment; trackInd < endSegment; trackInd++)
			{
				RouteSegment routeSegment = routeSegmentBuffer[trackInd];
				if (this.pathElementLookup.TryGetBuffer(routeSegment.m_Segment, out DynamicBuffer<PathElement> trackCurves))
				{
					for (int i = 0; i < trackCurves.Length; i++)
					{						
						if (this.curveLookup.TryGetComponent(trackCurves[i].m_Target, out Curve curve))
						{
							results.Write(new CurveDef(curve.m_Bezier, type));
						}					
					}
				}
			}
		}

		private void getRouteNavigationCurves(Entity entity)
		{
			if (this.carNavigationLaneSegmentLookup.TryGetBuffer(entity, out DynamicBuffer<CarNavigationLane> pathElements) && !pathElements.IsEmpty)
			{
				for (int i = 0; i < pathElements.Length; i++)
				{
					if (this.curveLookup.TryGetComponent(pathElements[i].m_Lane, out Curve curve))
					{
						results.Write(this.getCurveDef(pathElements[i].m_Lane, curve.m_Bezier, pathElements[i].m_CurvePosition));
					}
				}
			}
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
	}
}
