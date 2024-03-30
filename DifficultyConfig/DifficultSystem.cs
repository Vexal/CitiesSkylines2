using Game;
using Game.Common;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace DifficultyConfig
{
	internal partial class DifficultSystem : GameSystemBase
	{
		private EntityQuery milestoneQuery;

		private int[] originalMilestoneRewards;

		protected override void OnCreate()
		{
			base.OnCreate();		
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			var settings = Mod.INSTANCE.settings();

			this.milestoneQuery = GetEntityQuery(ComponentType.ReadOnly<MilestoneData>());
			RequireForUpdate(this.milestoneQuery);

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

		private void updateGlobal(DifficultySettings settings)
		{
			this.updateMilestoneRewards(!settings.disableMilestoneRewards);
		}

		protected override void OnUpdate()
		{
			
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
					Mod.log.Info("Cached milestone reward " + i + ": " + rewards[i]);
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

					Mod.log.Info("Native milestone " + i + ": " + milestone.m_Reward);
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
