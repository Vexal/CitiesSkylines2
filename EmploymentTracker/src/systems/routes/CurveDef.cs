using Colossal.Mathematics;
using System;

namespace EmploymentTracker
{
	public struct CurveDef : IEquatable<CurveDef>
	{
		public Bezier4x3 curve;
		public byte type;
		public int hashCode;

		public CurveDef(Bezier4x3 curve, byte type)
		{
			this.curve = curve;
			this.type = type;

			int precomputedHashCode = 435695894;
			precomputedHashCode = precomputedHashCode * -1521134295 + curve.a.GetHashCode();
			precomputedHashCode = precomputedHashCode * -1521134295 + curve.b.GetHashCode();
			precomputedHashCode = precomputedHashCode * -1521134295 + curve.c.GetHashCode();
			precomputedHashCode = precomputedHashCode * -1521134295 + curve.d.GetHashCode();

			this.hashCode = precomputedHashCode;
		}

		public override bool Equals(object obj)
		{
			return obj is CurveDef def &&
					this.curve.Equals(def.curve) &&
					this.type == def.type;
		}

		public bool Equals(CurveDef other)
		{
			return this.type == other.type && this.curve.Equals(other.curve);
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}
	}   
}
