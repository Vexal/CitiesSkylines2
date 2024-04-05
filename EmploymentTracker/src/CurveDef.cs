using Colossal.Mathematics;
using Game.Net;
using UnityEngine;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem
	{
		struct CurveDef
		{
			public Bezier4x3 curve;
			public byte type;

			public CurveDef(Bezier4x3 curve, byte type)
			{
				this.curve = curve;
				this.type = type;
			}

			public override bool Equals(object obj)
			{
				return obj is CurveDef def &&
					   this.curve.Equals(def.curve) &&
					   this.type == def.type;
			}

			public override int GetHashCode()
			{
				int hashCode = 1573490305;
				hashCode = hashCode * -1521134295 + this.curve.GetHashCode();
				hashCode = hashCode * -1521134295 + this.type.GetHashCode();
				return hashCode;
			}
		}
    }
}
