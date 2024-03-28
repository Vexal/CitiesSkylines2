using Colossal.IO.AssetDatabase;
using Game.Settings;

namespace EmploymentTracker
{
	[FileLocation(nameof(EmploymentTracker))]
	public class EmploymentTrackerSettings : Setting
	{
		public bool enabled { get; set; }

		public override void SetDefaults()
		{
			this.enabled = true;
		}
	}
}
