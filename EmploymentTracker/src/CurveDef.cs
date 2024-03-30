using Game.Net;
using UnityEngine;

namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem
	{
		struct CurveDef
		{
			public Curve curve;
			public Color color;
			public float width;

			public CurveDef(Curve curve, Color color, float width)
			{
				this.curve = curve;
				this.color = color;
				this.width = width;
			}
		}
    }
}
