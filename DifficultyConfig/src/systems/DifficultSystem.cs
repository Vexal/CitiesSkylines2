using Colossal.Entities;
using Game;
using Game.City;
using Game.Common;
using Game.Prefabs;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;

namespace DifficultyConfig
{
	internal partial class DifficultSystem : GameSystemBase
	{
		private DifficultySettings settings;
		private EntityQuery milestoneQuery;

		private int[] originalMilestoneRewards;
		private CitySystem citySystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			this.citySystem = World.GetExistingSystemManaged<CitySystem>();
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			this.settings = Mod.INSTANCE.settings();

			this.milestoneQuery = GetEntityQuery(ComponentType.ReadOnly<MilestoneData>());

			this.originalMilestoneRewards = this.cacheMilestoneRewards();

			settings.onSettingsApplied += setting =>
			{
				if (setting.GetType() == typeof(DifficultySettings))
				{
					this.updateGlobal((DifficultySettings)setting);
				}
			};

			this.updateGlobal(settings);
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
		}

		private void updateGlobal(DifficultySettings settings)
		{
			this.updateMilestoneRewards(!settings.disableMilestoneRewards);
		}

		bool gameIsLost = false;
		protected override void OnUpdate()
		{
			if (this.settings.allowGameLoss)
			{
				if (EntityManager.TryGetComponent<PlayerMoney>(this.citySystem.City, out PlayerMoney currentMoney) && currentMoney.money < this.settings.minimumMoneyLoss)
				{
					if (!EntityManager.HasComponent<BurningCity>(this.citySystem.City))
					{
						EntityManager.AddComponentData(this.citySystem.City, new BurningCity(20));
					}
					if (!EntityManager.HasComponent<CollapsingCity>(this.citySystem.City))
					{
						EntityManager.AddComponentData(this.citySystem.City, new CollapsingCity(this.settings.lossSpeed));
					}
				} 
				else
				{
					if (EntityManager.HasComponent<BurningCity>(this.citySystem.City))
					{
						this.EntityManager.RemoveComponent<BurningCity>(this.citySystem.City);
					}
					if (EntityManager.HasComponent<CollapsingCity>(this.citySystem.City))
					{
						this.EntityManager.RemoveComponent<CollapsingCity>(this.citySystem.City);
					}
				}

				if (gameIsLost)
				{
					return;
				}

			}
		}

		private int[] cacheMilestoneRewards()
		{
			NativeArray<MilestoneData> nativeArray2 = this.milestoneQuery.ToComponentDataArray<MilestoneData>(Allocator.Temp);
			int[] rewards = new int[nativeArray2.Length];
			try
			{
				for (int i = 0; i < nativeArray2.Length; i++)
				{
					rewards[i] = nativeArray2[i].m_Reward;
				}
			}
			finally
			{
				nativeArray2.Dispose();
			}

			return rewards;
		}

		private void updateMilestoneRewards(bool toggle)
		{
			NativeArray<Entity> nativeArray = this.milestoneQuery.ToEntityArray(Allocator.Temp);
			NativeArray<MilestoneData> nativeArray2 = this.milestoneQuery.ToComponentDataArray<MilestoneData>(Allocator.Temp);

			try
			{
				for (int i = 0; i < nativeArray2.Length; i++)
				{
					var milestone = nativeArray2[i];

					if (toggle)
					{
						milestone.m_Reward = this.originalMilestoneRewards[i];
					}
					else
					{
						milestone.m_Reward = 0;
					}

					nativeArray2[i] = milestone;

					var entity = nativeArray[i];
					EntityManager.SetComponentData<MilestoneData>(entity, milestone);
					EntityManager.AddComponent<BatchesUpdated>(entity);
				}
			}
			finally
			{
				nativeArray.Dispose();
				nativeArray2.Dispose();
			}
		}
	}
}
