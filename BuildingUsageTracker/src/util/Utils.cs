using Colossal;
using Game.Buildings;
using Game.Routes;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	internal static class Utils
	{
		public static string jsonFieldC(string name, int value, bool comma = true)
		{
			return (comma ? ",\"" : "\"") + name + "\":" + value;
		}

		public static string jsonFieldC(string name, NativeCounter value, bool comma = true)
		{
			return (comma ? ",\"" : "\"") + name + "\":" + value.Count;
		}

		public static string jsonEntity(Entity entity)
		{
			return "\"" + entity.Index + ":" + entity.Version + "\"";
		}

		public static string jsonArray(string name, NativeList<Entity> entities, bool comma = true)
		{
			string result = (comma ? ",\"" : "\"") + name + "\":[";
			for (int i = 0; i < entities.Length; ++i)
			{
				result += jsonEntity(entities[i]);
				if ( i < entities.Length - 1 )
				{
					result += ",";
				}
			}

			return result + "]";
		}

		public static bool isBuilding(this EntityManager EntityManager, Entity entity)
		{
			return EntityManager.Exists(entity) && EntityManager.HasComponent<Building>(entity);
		}

		public static bool isTransitStation(this EntityManager EntityManager, Entity entity)
		{
			return EntityManager.Exists(entity) && (EntityManager.HasComponent<PublicTransportStation>(entity) || EntityManager.HasComponent<BusStop>(entity) 
				|| EntityManager.HasComponent<TramStop>(entity));
		}

		public static bool isParkingStructure(this EntityManager EntityManager, Entity entity)
		{
			return EntityManager.Exists(entity) && (EntityManager.HasComponent<ParkingFacility>(entity));
		}

		public static Entity entity(string str)
		{
			string[] e = str.Split(':');
			if (e.Length != 2 )
			{
				return Entity.Null;
			}
			return new Entity { Index = int.Parse(e[0]), Version = int.Parse(e[1]) };
		}
	}
}
