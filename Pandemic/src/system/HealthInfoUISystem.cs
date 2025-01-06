using Colossal.Entities;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Citizens;
using Game.Common;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Pandemic
{
	public partial class HealthInfoUISystem : InfoSectionBase
	{
		private EntityQuery diseaseQuery;
		private EntityQuery currentDiseaseQuery;
		private ValueBinding<Disease[]> diseaseBinding;
		private ValueBinding<string[]> currentInfectionCountBinding;
		private UIUpdateState uf;
		private ToolSystem toolSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			m_InfoUISystem.AddTopSection(this);
			this.uf = UIUpdateState.Create(base.World, 60);

			this.diseaseBinding = new ValueBinding<Disease[]>("Pandemic", "diseases", new Disease[] { }, new ArrayWriter<Disease>(new ValueWriter<Disease>()));
			AddBinding(this.diseaseBinding);

			this.currentInfectionCountBinding = new ValueBinding<string[]>("Pandemic", "currentInfectionCount", new string[] { }, new ArrayWriter<string>());
			AddBinding(this.currentInfectionCountBinding);

			this.diseaseQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Disease>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.currentDiseaseQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<CurrentDisease>(),
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>()
				}
			});

			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.Enabled)
			{
				this.Enabled = true;
			}

			if (!this.uf.Advance())
			{
				return;
			}

			NativeArray<Disease> diseases = this.diseaseQuery.ToComponentDataArray<Disease>(Allocator.Temp);
			this.diseaseBinding.Update(diseases.ToArray());

			NativeArray<CurrentDisease> currentSick = this.currentDiseaseQuery.ToComponentDataArray<CurrentDisease>(Allocator.Temp);

			NativeHashMap<Entity, int> currentSickCounnts = new NativeHashMap<Entity, int>(diseases.Length, Allocator.Temp);

			foreach (CurrentDisease c in currentSick)
			{
				if (currentSickCounnts.TryGetValue(c.disease, out int v)) {
					currentSickCounnts[c.disease] += 1;
				}
				else
				{
					currentSickCounnts[c.disease] = 1;
				}
			}

			string[] r = new string[currentSickCounnts.Count];
			int i = 0;
			foreach (var v in currentSickCounnts)
			{
				r[i++] = v.Key.Index.ToString() + ":" + v.Key.Version.ToString() + "_" + v.Value.ToString();
			}

			this.currentInfectionCountBinding.Update(r);
		}

		protected override void Reset()
		{

		}

		protected override void OnProcess()
		{

		}

		public override void OnWriteProperties(IJsonWriter writer)
		{
			if (!EntityManager.Exists(this.toolSystem.selected))
			{
				return;
			}

			Entity citizen = this.toolSystem.selected;
			if (EntityManager.HasComponent<Citizen>(citizen) || this.tryGetCitizenEntity(this.toolSystem.selected, out citizen) &&
				EntityManager.TryGetComponent(citizen, out CurrentDisease currentDisease) && EntityManager.TryGetComponent(currentDisease.disease, out Disease disease))
			{
				writer.PropertyName("diseaseStrain");
				writer.Write(patientCount);
				writer.PropertyName("patientCapacity");
				writer.Write(patientCapacity);
			}
		}

		/*protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			
		}*/

		public override GameMode gameMode => GameMode.Game;

		protected override string group => "test";
		private bool tryGetCitizenEntity(Entity target, out Entity citizen)
		{
			if (EntityManager.TryGetComponent<Game.Creatures.Resident>(target, out var resident) && EntityManager.Exists(resident.m_Citizen))
			{
				citizen = resident.m_Citizen;
				return true;
			}
			else
			{
				citizen = Entity.Null;
				return false;
			}
		}
		
		private bool tryGetCitizen(Entity target, out Citizen citizen)
		{
			if (EntityManager.TryGetComponent<Game.Creatures.Resident>(target, out var resident) && EntityManager.Exists(resident.m_Citizen) && EntityManager.TryGetComponent(resident.m_Citizen, out citizen))
			{
				return true;
			}
			else
			{
				citizen = default;
				return false;
			}
		}

		/*protected override void OnCreate()
		{
			base.OnCreate();
			m_InfoUISystem.AddMiddleSection(this);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			visible = true;
		}

		protected override string group
		{
			get
			{
				return "HealthcareSection";
			}
		}

		public override void OnWriteProperties(IJsonWriter writer)
		{
			
		}

		protected override void OnProcess()
		{
			
		}

		protected override void Reset()
		{
			
		}*/
	}
}
