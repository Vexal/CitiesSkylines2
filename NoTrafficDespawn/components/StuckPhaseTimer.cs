using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace NoTrafficDespawn
{
	public struct StuckPhaseTimer : IComponentData, IQueryTypeParameter
	{
		public int frameCount;

		public StuckPhaseTimer(int frameCount)
		{
			this.frameCount = frameCount;
		}
	}
}
