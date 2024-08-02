using Colossal.Entities;
using Game;
using Game.Common;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;

namespace NoVehicleTrailers
{
	public partial class NoVehicleTrailersSystem : GameSystemBase
	{
		private EntityQuery personalTrailerPrefabQuery;
		private EntityQuery deleteTrailerQuery;
		private bool disableCarTrailers;

		protected override void OnCreate()
		{
			base.OnCreate();

			this.personalTrailerPrefabQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Game.Prefabs.CarTrailerData>(), ComponentType.ReadWrite<Game.Prefabs.PersonalCarData>()
			},
				Any = new ComponentType[]
			{
			},
				None = new ComponentType[]
			{
				
			},
				Options = EntityQueryOptions.IncludeDisabledEntities
			});

			this.deleteTrailerQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new ComponentType[]
			{
				ComponentType.ReadOnly<Game.Vehicles.CarTrailer>(),
				ComponentType.ReadOnly<Game.Vehicles.PersonalCar>()
			},
				Any = new ComponentType[]
			{
			},
				None = new ComponentType[]
			{
				ComponentType.ReadOnly<Deleted>(),
				ComponentType.ReadOnly<Temp>(),
				}
			});

			this.disableCarTrailers = Mod.INSTANCE.m_Setting.disableCarTrailers;		

			Mod.INSTANCE.m_Setting.noVehicleTrailersSystem = this;

			Mod.INSTANCE.m_Setting.onSettingsApplied += (setting) =>
			{
				if (((Setting)setting).disableCarTrailers != this.disableCarTrailers) {
					this.togglePersonalTrailers(((Setting)setting).disableCarTrailers);
					this.disableCarTrailers = ((Setting)setting).disableCarTrailers;
				}
			};
		}

		protected override void OnUpdate()
		{
			
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			if (this.disableCarTrailers)
			{
				this.togglePersonalTrailers(true);
			}
		}

		public void deletePersonalTrailers()
		{
			NativeArray<Entity> existingTrailers = this.deleteTrailerQuery.ToEntityArray(Allocator.Temp);
			foreach (Entity entity in existingTrailers)
			{
				EntityManager.AddComponent<Deleted>(entity);
				EntityManager.AddComponent<Updated>(entity);
			}
		}

		private void togglePersonalTrailers(bool disable)
		{
			NativeArray<Entity> trailerPrefabs = this.personalTrailerPrefabQuery.ToEntityArray(Allocator.Temp);
			for (int i = 0; i < trailerPrefabs.Length; i++)
			{
				if (EntityManager.TryGetComponent<Game.Prefabs.PersonalCarData>(trailerPrefabs[i], out var component))
				{
					if (disable)
					{
						EntityManager.AddComponent<Disabled>(trailerPrefabs[i]);
					}
					else if (EntityManager.HasComponent<Disabled>(trailerPrefabs[i]))
					{
						EntityManager.RemoveComponent<Disabled>(trailerPrefabs[i]);
					}

					EntityManager.AddComponent<Updated>(trailerPrefabs[i]);
				}
			}
		}
	}
}
