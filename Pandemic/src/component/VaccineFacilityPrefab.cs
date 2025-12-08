using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
	[ComponentMenu("Buildings/", new Type[]{typeof(BuildingPrefab)})]
	public class VaccineFacilityPrefab : ComponentBase
	{
		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{
			components.Add(ComponentType.ReadWrite<VaccineFacility>());
		}

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{

		}
	}
}
