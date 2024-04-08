using Game.Common;
using Game.Creatures;
using Game.Vehicles;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace EmploymentTracker
{
	[BurstCompile]
	public struct EntitySelectJob : IJob
	{
		[ReadOnly]
		public Entity input;
		public NativeList<Entity> results;
		[ReadOnly]
		public ComponentLookup<Target> targetLookup;
		[ReadOnly]
		public ComponentLookup<Animal> animalLookup;
		[ReadOnly]
		public ComponentLookup<Controller> controllerLookup;
		[ReadOnly]
		public BufferLookup<Passenger> passengerLookup;
		[ReadOnly]
		public BufferLookup<LayoutElement> layoutElementLookup;

		public void Execute()
		{
			this.executeInternal(input);
		}

		private void executeInternal(Entity entity)
		{
			if (this.passengerLookup.HasBuffer(entity))
			{
				//Vehicle has multiple cars (such as a train)
				if (this.handleForVehicleController(entity))
				{
					//selected car is the controller
					return;
				}
				else if (this.handleForSubVehicle(entity))
				{
					//selected a car not controlling the overall vehicle
					return;
				}
				else
				{
					//vehicle only has one element
					this.handleForPassengers(entity);
				}
			}
			else if (this.targetLookup.HasComponent(entity))
			{
				this.results.Add(entity);
			}
		}

		private void handleForPassengers(Entity entity)
		{
			if (this.passengerLookup.TryGetBuffer(entity, out var passengers))
			{
				for (int i = 0; i < passengers.Length; i++)
				{
					if (!this.animalLookup.HasComponent(passengers[i].m_Passenger))
					{
						this.results.Add(passengers[i].m_Passenger);
					}
				}
			}
		}

		private bool handleForVehicleController(Entity entity)
		{
			if (this.layoutElementLookup.TryGetBuffer(entity, out var layoutElements))
			{
				for (int i = 0; i < layoutElements.Length; i++)
				{
					this.handleForPassengers(layoutElements[i].m_Vehicle);
				}

				return true;
			}

			return false;
		}

		private bool handleForSubVehicle(Entity entity)
		{
			if (this.controllerLookup.TryGetComponent(entity, out var controller))
			{
				//selected a car not controlling the overall vehicle
				this.handleForVehicleController(controller.m_Controller);
				return true;
			}

			return false;
		}
	}
}
