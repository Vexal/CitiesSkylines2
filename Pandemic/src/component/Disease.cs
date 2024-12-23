﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
	public struct Disease : IComponentData, IQueryTypeParameter
	{
		public uint minFrame;
	}
}
