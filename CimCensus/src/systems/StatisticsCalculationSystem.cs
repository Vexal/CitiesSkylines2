using Colossal;
using Colossal.UI.Binding;
using Game;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Objects;
using Game.Tools;
using Game.UI;
using Game.Vehicles;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;

namespace CimCensus
{
	public partial class StatisticsCalculationSystem : UISystemBase
	{
		private ValueBinding<string> dataBindings;
		private Dictionary<string, string> dataValues = new Dictionary<string, string>();
		private List<string> dataOrderings = new List<string>();

		private EntityQuery getAllVehiclesQuery;
		private EntityQuery workerQuery;

		private EntityQuery cimsQuery;
		private bool isActive = false;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.dataBindings = new ValueBinding<string>("CimCensus", "dataBindings", "");
			AddBinding(this.dataBindings);

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
				ComponentType.ReadOnly<Controller>()
				}
			});

			this.cimsQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Citizen>()
			},
				Any = new ComponentType[]
				{

				},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<Hidden>(),
				ComponentType.ReadOnly<Worker>(),
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

			AddBinding(new TriggerBinding<bool>("CimCensus", "toggle", s => {
				this.isActive = s;
				this.frameCount = 0;
				if (s)
				{
					this.handleWorkers();
					this.updateBindings();
				}
			}));

			AddBinding(new TriggerBinding<bool>("CimCensus", "update", s => {
				this.handleWorkers();
				this.updateBindings();
			}));
		}

		protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);
			this.dataOrderings.Clear();
			this.dataValues.Clear();
			this.dataBindings.Update("");
			this.isActive = false;
		}

		long frameCount = 0;
		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (!this.isActive)
			{
				return;
			}

			if (this.frameCount++ % 60 == 0)
			{
				var clock = new Stopwatch();
				clock.Start();
				this.handleWorkers();

				clock.Stop();
				this.setData("Time (ms)", clock.ElapsedMilliseconds.ToString());
				this.updateBindings();
			}
		}

		private void handleWorkers()
		{
			NativeCounter totalCimsInCityLimits = new NativeCounter(Allocator.TempJob);
			NativeCounter totalCimsOutsideCity = new NativeCounter(Allocator.TempJob);
			NativeCounter totalExtraCimsOutsideCity = new NativeCounter(Allocator.TempJob);
			NativeCounter foreignCimsInCity = new NativeCounter(Allocator.TempJob);
			NativeCounter nativeCimsOutsideCity = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsGoingHome = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsGoingToSchool = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsShopping = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsAtSchool = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsAtSchoolInsideCity = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsAtSchoolOutsideCity = new NativeCounter(Allocator.TempJob);
			NativeCounter cimsActive = new NativeCounter(Allocator.TempJob);
			NativeCounter homelessCims = new NativeCounter(Allocator.TempJob);
			NativeCounter dummyTraffic = new NativeCounter(Allocator.TempJob);
			NativeCounter realTraffic = new NativeCounter(Allocator.TempJob);
			NativeCounter deliveryTraffic = new NativeCounter(Allocator.TempJob);
			NativeCounter parkedVehicles = new NativeCounter(Allocator.TempJob);
			NativeCounter personalVehicles = new NativeCounter(Allocator.TempJob);
			NativeCounter dispatchedAmbulances = new NativeCounter(Allocator.TempJob);
			NativeCounter totalStudents = new NativeCounter(Allocator.TempJob);

			CountWorkersJob countWorkersJob = new CountWorkersJob();
			countWorkersJob.init();

			countWorkersJob.totalCimsInCityLimits = totalCimsInCityLimits.ToConcurrent();
			countWorkersJob.totalCimsOutsideCity = totalCimsOutsideCity.ToConcurrent();
			countWorkersJob.foreignCimsInCity = foreignCimsInCity.ToConcurrent();
			countWorkersJob.nativeCimsOutsideCity = nativeCimsOutsideCity.ToConcurrent();
			countWorkersJob.cimsGoingHome = cimsGoingHome.ToConcurrent();
			countWorkersJob.cimsActive = cimsActive.ToConcurrent();
			countWorkersJob.cimsShopping = cimsShopping.ToConcurrent();
			countWorkersJob.homelessCims = homelessCims.ToConcurrent();

			countWorkersJob.workerHandle = EntityManager.GetComponentTypeHandle<Worker>(true);
			countWorkersJob.travelPurposeHandle = EntityManager.GetComponentTypeHandle<TravelPurpose>(true);
			countWorkersJob.citizenHandle = EntityManager.GetComponentTypeHandle<Citizen>(true);
			countWorkersJob.currentBuildingHandle = EntityManager.GetComponentTypeHandle<CurrentBuilding>(true);
			countWorkersJob.currentTransportHandle = GetComponentTypeHandle<CurrentTransport>(true);
			countWorkersJob.householdMemberHandle = GetComponentTypeHandle<HouseholdMember>(true);
			countWorkersJob.outsideConnectionLookup = GetComponentLookup<OutsideConnection>(true);
			countWorkersJob.unspawnedLookup = GetComponentLookup<Unspawned>(true);
			countWorkersJob.movingLookup = GetComponentLookup<Moving>(true);
			countWorkersJob.propertyRenterLookup = GetComponentLookup<PropertyRenter>(true);

			CountCimsJob countCimsJob = new CountCimsJob();

			countCimsJob.totalCimsInCityLimits = totalCimsInCityLimits.ToConcurrent();
			countCimsJob.totalCimsOutsideCity = totalCimsOutsideCity.ToConcurrent();
			countCimsJob.totalExtraCims = totalExtraCimsOutsideCity.ToConcurrent();
			countCimsJob.foreignCimsInCity = foreignCimsInCity.ToConcurrent();
			countCimsJob.nativeCimsOutsideCity = nativeCimsOutsideCity.ToConcurrent();
			countCimsJob.cimsGoingHome = cimsGoingHome.ToConcurrent();
			countCimsJob.cimsGoingToSchool = cimsGoingToSchool.ToConcurrent();
			countCimsJob.cimsAtSchool = cimsAtSchool.ToConcurrent();
			countCimsJob.cimsAtSchoolInsideCity = cimsAtSchoolInsideCity.ToConcurrent();
			countCimsJob.cimsAtSchoolOutsideCity = cimsAtSchoolOutsideCity.ToConcurrent();
			countCimsJob.cimsActive = cimsActive.ToConcurrent();
			countCimsJob.cimsShopping = cimsShopping.ToConcurrent();
			countCimsJob.homelessCims = homelessCims.ToConcurrent();
			countCimsJob.totalStudents = totalStudents.ToConcurrent();

			countCimsJob.travelPurposeHandle = EntityManager.GetComponentTypeHandle<TravelPurpose>(true);
			countCimsJob.citizenHandle = EntityManager.GetComponentTypeHandle<Citizen>(true);
			countCimsJob.currentBuildingHandle = EntityManager.GetComponentTypeHandle<CurrentBuilding>(true);
			countCimsJob.currentTransportHandle = GetComponentTypeHandle<CurrentTransport>(true);
			countCimsJob.householdMemberHandle = GetComponentTypeHandle<HouseholdMember>(true);
			countCimsJob.studentTypeHandle = GetComponentTypeHandle<Game.Citizens.Student>(true);
			countCimsJob.outsideConnectionLookup = GetComponentLookup<OutsideConnection>(true);
			countCimsJob.unspawnedLookup = GetComponentLookup<Unspawned>(true);
			countCimsJob.movingLookup = GetComponentLookup<Moving>(true);
			countCimsJob.movingAwayLookup = GetComponentLookup<MovingAway>(true);
			countCimsJob.commuterHouseholdLookup = GetComponentLookup<CommuterHousehold>(true);
			countCimsJob.propertySeekerLookup = GetComponentLookup<PropertySeeker>(true);
			countCimsJob.propertyRenterLookup = GetComponentLookup<PropertyRenter>(true);


			CountVehiclesJob countVehiclesJob = new CountVehiclesJob();
			countVehiclesJob.personalCarHandle = GetComponentTypeHandle<PersonalCar>(true);
			countVehiclesJob.deliveryTruckHandle = GetComponentTypeHandle<DeliveryTruck>(true);
			countVehiclesJob.ambualnceHandle = GetComponentTypeHandle<Ambulance>(true);
			countVehiclesJob.parkedCarHandle = GetComponentTypeHandle<ParkedCar>(true);
			countVehiclesJob.publicTransoprtHandle = GetComponentTypeHandle<PublicTransport>(true);
			countVehiclesJob.cargoTransportHandle = GetComponentTypeHandle<CargoTransport>(true);

			countVehiclesJob.outsideConnectionLookup = GetComponentLookup<Game.Net.OutsideConnection>(true);

			countVehiclesJob.realTraffic = realTraffic.ToConcurrent();
			countVehiclesJob.dummyTraffic = dummyTraffic.ToConcurrent();
			countVehiclesJob.personalVehicles = personalVehicles.ToConcurrent();
			countVehiclesJob.deliveryTraffic = deliveryTraffic.ToConcurrent();
			countVehiclesJob.dispatchedAmbulances = dispatchedAmbulances.ToConcurrent();
			countVehiclesJob.parkedVehicles = parkedVehicles.ToConcurrent();

			Unity.Jobs.JobHandle countWorkersHandle = countWorkersJob.ScheduleParallel(this.workerQuery, default);
			Unity.Jobs.JobHandle countCimsHandle = countCimsJob.ScheduleParallel(this.cimsQuery, default);
			Unity.Jobs.JobHandle countVehiclesHandle = countVehiclesJob.ScheduleParallel(this.getAllVehiclesQuery, default);

			countWorkersHandle.Complete();
			countCimsHandle.Complete();
			countVehiclesHandle.Complete();

			this.setData("Cims - In City Limits - Total", totalCimsInCityLimits);
			this.setData("Cims - In City Limits - Visitors", foreignCimsInCity);
			this.setData("Cims - In City Limits - Active", cimsActive);
			this.setData("Cims - In City Limits - Homeless", homelessCims);
			this.setData("Cims - Outside City - Relevant", totalCimsOutsideCity);
			this.setData("Cims - Outside City - Misc", totalExtraCimsOutsideCity);
			this.setData("Cims - Going Home", cimsGoingHome);
			this.setData("Cims - Shopping", cimsShopping);

			this.setData("Workers - Total", countWorkersJob.totalWorkersN);
			this.setData("Workers - Source (local/foreign)", countWorkersJob.localWorkersN, countWorkersJob.outsideWorkersN);
			this.setData("Workers - At Work", countWorkersJob.totalWorkersWorkingN);
			this.setData("Workers - Going to Work", countWorkersJob.cimsGoingToWorkN);
			this.setData("Workers - Shifts (d/e/n)", countWorkersJob.dayShiftWorkersN, countWorkersJob.eveningShiftWorkersN, countWorkersJob.nightShiftWorkersN);

			this.setData("Students - Total", totalStudents);
			this.setData("Students - At School - Total", cimsAtSchool);
			this.setData("Students - At School - In City Limits", cimsAtSchoolInsideCity);
			this.setData("Students - At School - Outside City Limits", cimsAtSchoolOutsideCity);
			this.setData("Students - Going to School", cimsGoingToSchool);

			this.setData("Vehicles - Active - Total (Real/Fake)", (realTraffic.Count + dummyTraffic.Count).ToString() 
				+ " (" + realTraffic.Count.ToString() + " / " + dummyTraffic.Count.ToString() + ")");
			this.setData("Vehicles - Active - Personal", personalVehicles);
			this.setData("Vehicles - Active - Commercial", deliveryTraffic);
			this.setData("Vehicles - Parked", parkedVehicles);
			this.setData("Ambulances - Active", dispatchedAmbulances);

			countWorkersJob.cleanup();

			totalCimsInCityLimits.Dispose();
			totalCimsOutsideCity.Dispose();
			totalExtraCimsOutsideCity.Dispose();
			foreignCimsInCity.Dispose();
			nativeCimsOutsideCity.Dispose();
			cimsGoingHome.Dispose();
			cimsGoingToSchool.Dispose();
			cimsShopping.Dispose();
			cimsAtSchool.Dispose();
			cimsAtSchoolInsideCity.Dispose();
			cimsAtSchoolOutsideCity.Dispose();
			cimsActive.Dispose();
			homelessCims.Dispose();
			dummyTraffic.Dispose();
			realTraffic.Dispose();
			deliveryTraffic.Dispose();
			parkedVehicles.Dispose();
			personalVehicles.Dispose();
			dispatchedAmbulances.Dispose();
			totalStudents.Dispose();
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
