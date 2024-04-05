namespace EmploymentTracker
{
	internal partial class HighlightEmployeesSystem
	{
		struct HighlightFeatures
		{
			public bool employeeResidences;
			public bool studentResidences;
			public bool employeeCommuters;
			public bool destinations;
			public bool workplaces;
			public bool routes;

			public bool dirty;

			public HighlightFeatures(bool employeeResidences, bool studentResidences, bool employeeCommuters, bool destinations, bool workplaces, bool routes)
			{
				this.employeeResidences = employeeResidences;
				this.studentResidences = studentResidences;
				this.employeeCommuters = employeeCommuters;
				this.destinations = destinations;
				this.workplaces = workplaces;
				this.routes = routes;
				this.dirty = false;
			}

			public HighlightFeatures(EmploymentTrackerSettings settings)
			{
				this.employeeResidences = settings.highlightEmployeeResidences;
				this.studentResidences = settings.highlightStudentResidences;
				this.employeeCommuters = settings.highlightEmployeeCommuters;
				this.destinations = settings.highlightDestinations;
				this.workplaces = settings.highlightWorkplaces;
				this.routes = settings.highlightRoutes;
				this.dirty = false;
			}

			public bool highlightAnything()
			{
				return this.employeeResidences ||
					this.studentResidences ||
					this.employeeCommuters ||
					this.destinations ||
					this.workplaces ||
					this.routes;
			}
		}
	}
}
