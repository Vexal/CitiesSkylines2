using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Pandemic
{
	public static class Utils
	{
		public static string keyString(this Entity entity)
		{
			return entity.Index.ToString() + ":" + entity.Version.ToString();
		}
	}
}
