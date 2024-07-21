using Colossal;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Objects;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using static Game.UI.InGame.VehiclesSection;

namespace CimCensus
{
	public partial class StatisticsCalculationSystem : UISystemBase
	{
		private ValueBinding<string> dataBindings;
		private Dictionary<string, string> dataValues = new Dictionary<string, string>();
		private List<string> dataOrderings = new List<string>();

		private EntityQuery getAllCimsQuery;
		private EntityQuery getAllVehiclesQuery;
		private EntityQuery employeeQuery;
		private EntityQuery parkedCarQuery;
		private EntityQuery taxiQuery;
		private EntityQuery workerQuery;

		protected override void OnCreate()
		{
			base.OnCreate();


			this.dataBindings = new ValueBinding<string>("CimCensus", "dataBindings", "");
			AddBinding(this.dataBindings);

			this.getAllCimsQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{

			},
				Any = new ComponentType[]
				{
					ComponentType.ReadOnly<Human>(),
					ComponentType.ReadOnly<CurrentBuilding>(),
					ComponentType.ReadOnly<CurrentTransport>(),
					ComponentType.ReadOnly<Resident>(),
					ComponentType.ReadOnly<TravelPurpose>(),
					ComponentType.ReadOnly<Worker>(),
				},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Vehicle>(),
				ComponentType.ReadOnly<Hidden>(),
				}
			}
			
			);

			this.getAllVehiclesQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Vehicle>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Hidden>(),
				ComponentType.ReadOnly<ParkedCar>(),
				}
			});

			this.employeeQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Employee>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Hidden>(),
				}
			});

			this.parkedCarQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<ParkedCar>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Hidden>(),
				}
			});

			this.taxiQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Taxi>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Hidden>(),
				ComponentType.ReadOnly<ParkedCar>(),
				}
			});

			this.workerQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Worker>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Hidden>()
				}
			});
		}

		protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);
			this.dataOrderings.Clear();
			this.dataValues.Clear();
		}

		long frameCount = 0;
		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (this.frameCount++ % 16 == 0)
			{
				var clock = new Stopwatch();
				clock.Start();
				//this.handleCims();
				this.handleWorkers();
				/*this.handleTaxis();
				this.handleParkedCars();

				NativeArray<Entity> allVehicles = this.getAllVehiclesQuery.ToEntityArray(Allocator.Temp);

				int dummyTraffic = 0;
				int realTraffic = 0;
				int deliveryTraffic = 0;
				//int parkedVehicles = 0;
				int personalVehicles = 0;
				int dispatchedAmbulances = 0;
				foreach (Entity vehicle in allVehicles)
				{
					if (EntityManager.HasComponent<ParkedCar>(vehicle))
					{
						//parkedVehicles++;
					}
					else
					{
						if (EntityManager.TryGetComponent(vehicle, out PersonalCar personalCar))
						{
							if (personalCar.m_State.HasFlag(PersonalCarFlags.DummyTraffic))
							{
								dummyTraffic++;
							}
							else
							{
								realTraffic++;
								++personalVehicles;
							}
						}
						else if (EntityManager.TryGetComponent(vehicle, out DeliveryTruck deliveryTruck))
						{
							if (deliveryTruck.m_State.HasFlag(DeliveryTruckFlags.DummyTraffic))
							{
								dummyTraffic++;
							}
							else
							{
								realTraffic++;
								deliveryTraffic++;
							}
						}

						if (EntityManager.TryGetComponent(vehicle, out Ambulance ambulance))
						{
							if (ambulance.m_State.HasFlag(AmbulanceFlags.Dispatched) ||
								ambulance.m_State.HasFlag(AmbulanceFlags.AtTarget) ||
								ambulance.m_State.HasFlag(AmbulanceFlags.Transporting))
							{
								++dispatchedAmbulances;
							}
						}
					}
				}

				this.setData("Total Vehicles", allVehicles.Length);
				this.setData("Active Real Vehicles", realTraffic);
				this.setData("Dummy Traffic Vehicles", dummyTraffic);
				this.setData("Active Personal Vehicles", personalVehicles);
				this.setData("Active Delivery Vehicles", deliveryTraffic);
				//this.setData("Parked Vehicles", parkedVehicles);
				this.setData("Active Ambulances", dispatchedAmbulances);*/

				clock.Stop();
				this.setData("Time (ms)", clock.ElapsedMilliseconds.ToString());
				this.updateBindings();
			}
		}

		private void handleCims()
		{
			NativeArray<Entity> allCims = this.getAllCimsQuery.ToEntityArray(Allocator.Temp);

			int totalCims = 0;
			int residentsInVehicles = 0;
			int residentsSpawned = 0;
			int cimsInBuildings = 0;
			//int cimsGoingToWork = 0;
			int cimsMoving = 0;
			/*int dayShiftWorkers = 0;
			int nightShiftWorkers = 0;
			int eveningShiftWorkers = 0;
			int totalWorkers = 0;
			int totalWorkersWorking = 0;*/
			int totalResidents = 0;

			Dictionary<Purpose, int> travelPurposes = new Dictionary<Purpose, int>();

			foreach (Entity cim in allCims)
			{
				bool hasVehicle = false;
				if (hasVehicle = EntityManager.TryGetComponent(cim, out CurrentVehicle currentVehicle))
				{

				}
				bool isDummy = false;
				if (hasVehicle && EntityManager.TryGetComponent(currentVehicle.m_Vehicle, out PersonalCar personalCar))
				{
					isDummy = personalCar.m_State.HasFlag(PersonalCarFlags.DummyTraffic);
				}
				if (isDummy)
				{
					continue;
				}

				if (EntityManager.TryGetComponent(cim, out Resident resident))
				{
					if (resident.m_Flags.HasFlag(ResidentFlags.DummyTraffic))
					{
						continue;
					}
					else if (resident.m_Flags.HasFlag(ResidentFlags.InVehicle))
					{
						residentsInVehicles++;
					}

					++totalResidents;
				}
				bool alreadyCounted = false;
				bool outsideBuilding = false;
				if (alreadyCounted = outsideBuilding = EntityManager.TryGetComponent(cim, out CurrentTransport currentTransport))
				{
					++residentsSpawned;
					++totalCims;
				}
				if (EntityManager.TryGetComponent(cim, out CurrentBuilding currentBuilding))
				{
					if (!EntityManager.HasComponent<OutsideConnection>(currentBuilding.m_CurrentBuilding))
					{
						if (!outsideBuilding)
						{
							++cimsInBuildings;
						}
						if (!alreadyCounted)
						{
							++totalCims;
						}
					}
				}
				/*bool isWorker = false;
				if (isWorker = EntityManager.TryGetComponent(cim, out Worker worker))
				{
					if (worker.m_Shift == Workshift.Day)
					{
						++dayShiftWorkers;
					}
					else if (worker.m_Shift == Workshift.Evening)
					{
						++eveningShiftWorkers;
					}
					else if (worker.m_Shift == Workshift.Night)
					{
						++nightShiftWorkers;
					}

					++totalWorkers;
				}*/
				if (EntityManager.TryGetComponent(cim, out TravelPurpose travelPurpose))
				{
					++cimsMoving;
					if (!travelPurposes.TryGetValue(travelPurpose.m_Purpose, out int purposeCount))
					{

					}

					travelPurposes[travelPurpose.m_Purpose] = purposeCount + 1;

					/*if (isWorker)
					{
						if (travelPurpose.m_Purpose == Purpose.Working)
						{
							++totalWorkersWorking;
						}
						else if (travelPurpose.m_Purpose == Purpose.GoingToWork)
						{
							++cimsGoingToWork;
						}
					}*/
				}

			}

			/*NativeArray<Entity> employers = this.employeeQuery.ToEntityArray(Allocator.Temp);

			int totalEmployers = employers.Length;

			foreach (Entity employer in employers)
			{
				if (EntityManager.TryGetBuffer<Employee>(employer, true, out var employees))
				{
					foreach (Employee employee in employees)
					{

					}
				}
			}*/
			this.setData("Total Cims", totalCims);
			this.setData("Total Residents", totalResidents);
			this.setData("Cims in Vehicles", residentsInVehicles);
			this.setData("Cims Active", residentsSpawned);
			this.setData("Cims in Buildings", cimsInBuildings);
			this.setData("Cims Moving", cimsMoving);

			foreach (var tp in travelPurposes)
			{
				this.setData("Cims - " + tp.Key.ToString(), tp.Value);
			}

			allCims.Dispose();
		}

		private void handleWorkers()
		{

			/*NativeArray<Entity> entities = this.workerQuery.ToEntityArray(Allocator.Temp);

			int dayShiftWorkers = 0;
			int nightShiftWorkers = 0;
			int eveningShiftWorkers = 0;
			int totalWorkers = 0;
			int totalWorkersWorking = 0;
			int cimsGoingToWork = 0;
			int outsideWorkers = 0;
			int localWorkers = 0;

			foreach (Entity entity in entities)
			{
				if (EntityManager.TryGetComponent(entity, out Worker worker))
				{
					if (worker.m_Shift == Workshift.Day)
					{
						++dayShiftWorkers;
					}
					else if (worker.m_Shift == Workshift.Evening)
					{
						++eveningShiftWorkers;
					}
					else if (worker.m_Shift == Workshift.Night)
					{
						++nightShiftWorkers;
					}

					++totalWorkers;
				}*/

				/*if (EntityManager.TryGetComponent(entity, out TravelPurpose travelPurpose))
				{

					if (travelPurpose.m_Purpose == Purpose.Working)
					{
						++totalWorkersWorking;
					}
					else if (travelPurpose.m_Purpose == Purpose.GoingToWork)
					{
						++cimsGoingToWork;
					}
				}

				if (EntityManager.TryGetComponent(entity, out Citizen citizen))
				{
					if (citizen.m_State.HasFlag(CitizenFlags.Commuter))
					{
						++outsideWorkers;
					}
					else
					{
						++localWorkers;
					}
				}*//*
			}*/

			CountWorkersJob job = new CountWorkersJob();
			job.init();

			/*job.dayShiftWorkers = new NativeCounter(Allocator.TempJob);
			job.nightShiftWorkers = new NativeCounter(Allocator.TempJob);
			job.eveningShiftWorkers = new NativeCounter(Allocator.TempJob);
			job.totalWorkers = new NativeCounter(Allocator.TempJob);
			job.totalWorkersWorking = new NativeCounter(Allocator.TempJob);
			job.cimsGoingToWork = new NativeCounter(Allocator.TempJob);
			job.outsideWorkers = new NativeCounter(Allocator.TempJob);
			job.localWorkers = new NativeCounter(Allocator.TempJob);*/
			job.workerHandle = EntityManager.GetComponentTypeHandle<Worker>(true);
			job.travelPurposeHandle = EntityManager.GetComponentTypeHandle<TravelPurpose>(true);
			job.citizenHandle = EntityManager.GetComponentTypeHandle<Citizen>(true);

			Unity.Jobs.JobHandle jobHandle = job.Schedule(this.workerQuery, default);

			jobHandle.Complete();

			this.setData("Workers - Total", job.totalWorkers);
			this.setData("Workers - Source (local/outside)", job.localWorkers, job.outsideWorkers);
			this.setData("Workers - At Work", job.totalWorkersWorking);
			this.setData("Workers - Going to Work", job.cimsGoingToWork);
			this.setData("Workers - Shifts (day/evening/night)", job.dayShiftWorkers, job.eveningShiftWorkers, job.nightShiftWorkers);



			job.cleanup();

			/*job.dayShiftWorkers.Dispose();
			job.nightShiftWorkers.Dispose();
			job.eveningShiftWorkers.Dispose();
			job.totalWorkers.Dispose();
			job.totalWorkersWorking.Dispose();
			job.cimsGoingToWork.Dispose();
			job.outsideWorkers.Dispose();
			job.localWorkers.Dispose();*/

		}

		private void handleParkedCars()
		{
			NativeArray<Entity> entities = this.parkedCarQuery.ToEntityArray(Allocator.Temp);

			int parkedCars = 0;
			foreach (Entity entity in entities)
			{
				++parkedCars;
			}

			this.setData("Vehicles - Parked", parkedCars);
		}

		private void handleTaxis()
		{
			NativeArray<Entity> taxis = this.taxiQuery.ToEntityArray(Allocator.Temp);

			int outsideTaxis = 0;
			int transportingTaxis = 0;
			int taxiPassengerCount = 0;
			foreach (Entity taxi in taxis)
			{
				if (EntityManager.TryGetComponent<Taxi>(taxi, out var taxiComponent))
				{
					if (taxiComponent.m_State.HasFlag(TaxiFlags.Transporting))
					{
						++transportingTaxis;
					}
					if (taxiComponent.m_State.HasFlag(TaxiFlags.FromOutside))
					{
						++outsideTaxis;
					}
				}

				if (EntityManager.TryGetBuffer<Passenger>(taxi, true, out var passengers))
				{
					taxiPassengerCount += passengers.Length;
				}
			}

			this.setData("Taxis - Total", taxis.Length);
			this.setData("Taxis - Transporting", transportingTaxis);
			this.setData("Taxis - Passengers", taxiPassengerCount);
			this.setData("Taxis - From Outside", outsideTaxis);
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

			this.dataBindings.Update(string.Join(":", bindingList));
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
}
