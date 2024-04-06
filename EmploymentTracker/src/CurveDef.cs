﻿using Colossal.Mathematics;
using System;

namespace EmploymentTracker
{
	public struct CurveDef : IEquatable<CurveDef>
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

		public bool Equals(CurveDef other)
		{
			return this.type == other.type && this.curve.Equals(other.curve);
		}

		public override int GetHashCode()
		{
			/*int hashCode = 1573490305;
			hashCode = hashCode * -1521134295 + this.curve.GetHashCode();
			hashCode = hashCode * -1521134295 + this.type.GetHashCode();
			return hashCode;*/

			return this.curve.GetHashCode();
		}
	}   
}
