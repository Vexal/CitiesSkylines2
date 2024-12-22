using Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
	public struct SicknessEventData : IComponentData, IQueryTypeParameter
	{
		public Duration duration;
	}
}
