using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Creatures;
using Game.Events;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace Pandemic
{
	internal partial class ForceSicknessSystem : UISystemBase
	{
		private InputAction forceSickAction;
		private InputAction forceDangerAction;
		private InputAction forceAllSickAction;
		ToolSystem toolSystem;
		SimulationSystem simulationSystem;
		Entity selectedEntity; 
		private SicknessStarter sicknessStarter;
		private EntityQuery sickEventQuery;
		private EntityQuery allHealthyCitizenQuery;

		protected override void OnCreate()   
		{
			base.OnCreate();
			this.sicknessStarter = new SicknessStarter(World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>(), EntityManager);
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			this.simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

			this.forceSickAction = new InputAction("forceSick", InputActionType.Button);
			this.forceSickAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/s").With("Modifier", "<keyboard>/shift");

			this.forceDangerAction = new InputAction("forceDangerAction", InputActionType.Button);
			this.forceDangerAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/d").With("Modifier", "<keyboard>/shift");

			this.forceAllSickAction = new InputAction("forceDangerAction", InputActionType.Button);
			this.forceAllSickAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/a").With("Modifier", "<keyboard>/shift");

			this.sickEventQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<SicknessEventData>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.allHealthyCitizenQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Citizen>()
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				ComponentType.ReadOnly<HealthProblem>()
				}
			});
		}

		protected override void OnStartRunning() 
		{
			base.OnStartRunning();
			this.forceSickAction.Enable();
			this.forceDangerAction.Enable();
			this.forceAllSickAction.Enable();
		}

		protected override void OnStopRunning() 
		{
			base.OnStopRunning();
			this.forceSickAction.Disable();
			this.forceDangerAction.Disable();
			this.forceAllSickAction.Disable();
		}

		protected override void OnUpdate()
		{
			NativeArray<SicknessEventData> sickEvents = this.sickEventQuery.ToComponentDataArray<SicknessEventData>(Allocator.Temp);
			NativeArray<Entity> sickEventEntities = this.sickEventQuery.ToEntityArray(Allocator.Temp);
			for (int i = 0; i < sickEvents.Length; i++)
			{
				if (sickEvents[i].duration.m_EndFrame < this.simulationSystem.frameIndex)
				{
					EntityManager.AddComponent<Deleted>(sickEventEntities[i]);
				}
			}

			Entity selected = this.getSelected();

			bool updatedSelection = selected != this.selectedEntity;
			if (updatedSelection)
			{				
				this.selectedEntity = selected;
			}

			if (!EntityManager.Exists(selected))
			{
				return;
			}

			if (this.forceSickAction.WasPressedThisFrame())
			{
				if (!EntityManager.TryGetComponent<Game.Creatures.Resident>(this.selectedEntity, out var resident) && EntityManager.Exists(resident.m_Citizen))
				{
					return;
				}
				if (!EntityManager.HasComponent<HealthProblem>(resident.m_Citizen))
				{
					this.sicknessStarter.makeSick(resident.m_Citizen);
				}
			}

			if (this.forceAllSickAction.WasPressedThisFrame())
			{
				NativeArray<Entity> citizens = this.allHealthyCitizenQuery.ToEntityArray(Allocator.Temp);
                foreach (var citizen in citizens)
                {
					this.sicknessStarter.makeSick(citizen);
                }
            }

			if (this.forceDangerAction.WasPressedThisFrame())
			{

				if (!EntityManager.TryGetComponent<Building>(this.selectedEntity, out var building))
				{
					return;
				}
				if (!EntityManager.HasComponent<InDanger>(this.selectedEntity))
				{
					this.sicknessStarter.makeDanger(this.selectedEntity, this.simulationSystem.frameIndex);
				}
			}   
		}

		private Entity getSelected()
		{
			return toolSystem.selected;
		}
	}
}
