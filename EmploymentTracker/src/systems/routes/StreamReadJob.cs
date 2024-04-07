using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EmploymentTracker
{
	[BurstCompile]
	public struct StreamReadJob : IJob
	{
		private NativeStream.Reader resultReader;

		public void Execute()
		{

		}
	}
}
