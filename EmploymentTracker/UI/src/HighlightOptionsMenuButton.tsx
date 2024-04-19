import { Button, FloatingButton, Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import tadaSrc from "./Traffic.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";

export const autoRefreshEntitiesBinding = bindValue<boolean>("EmploymentTracker", 'AutoRefreshTransitingEntitiesActive');
export const debugStatsBinding = bindValue<boolean>("EmploymentTracker", 'DebugActive');
export const trackedEntityCountBinding = bindValue<number>("EmploymentTracker", 'TrackedEntityCount');
export const totalSegmentCountBinding = bindValue<number>("EmploymentTracker", 'TotalSegmentCount');
export const uniqueSegmentCountBinding = bindValue<number>("EmploymentTracker", 'UniqueSegmentCount');
export const routeTimeBinding = bindValue<string>("EmploymentTracker", 'RouteTimeMs');
export const selectionTypeBinding = bindValue<string>("EmploymentTracker", 'selectionType');


export const highlightEnroute = bindValue<boolean>("EmploymentTracker", 'highlightEnroute');
export const highlightSelectedRoute = bindValue<boolean>("EmploymentTracker", 'highlightSelectedRoute');
export const highlightEnrouteTransit = bindValue<boolean>("EmploymentTracker", 'highlightEnrouteTransit');
export const highlightPassengerRoutes = bindValue<boolean>("EmploymentTracker", 'highlightPassengerRoutes');

export const highlightPassengerDestinations = bindValue<boolean>("EmploymentTracker", 'highlightPassengerDestinations');
export const highlightEmployeeResidences = bindValue<boolean>("EmploymentTracker", 'highlightEmployeeResidences');
export const highlightResidentWorkplaces = bindValue<boolean>("EmploymentTracker", 'highlightResidentWorkplaces');
export const highlightStudentResidences = bindValue<boolean>("EmploymentTracker", 'highlightStudentResidences');

export default class HighlightOptionsMenuButton extends Component {
	state = {
		menuOpen: false,
		autoRefreshTransitingEntities: autoRefreshEntitiesBinding.value,
		showStats: debugStatsBinding.value,
		trackedEntityCount: trackedEntityCountBinding.value,
		uniqueSegmentCount: uniqueSegmentCountBinding.value,
		totalSegmentCount: totalSegmentCountBinding.value,
		selectionType: selectionTypeBinding.value,

		highlightEnroute: highlightEnroute.value,
		highlightSelectedRoute: highlightSelectedRoute.value,
		highlightEnrouteTransit: highlightEnrouteTransit.value,
		highlightPassengerRoutes: highlightPassengerRoutes.value,

		highlightPassengerDestinations: highlightPassengerDestinations.value,
		highlightEmployeeResidences: highlightEmployeeResidences.value,
		highlightResidentWorkplaces: highlightResidentWorkplaces.value,
		highlightStudentResidences: highlightStudentResidences.value,

		

		/** @type {[string[]] */
		routeTimeMs: HighlightOptionsMenuButton.parseBindings(routeTimeBinding.value),
	}

	render() {
		//const { translate } = useLocalization();

		return <div>

			<FloatingButton src={tadaSrc} selected={this.state.menuOpen} onSelect={() => {
				this.setState({ menuOpen: !this.state.menuOpen }); console.log("route menu open");
				
			}} />

			{this.state.menuOpen && <div>
				<Panel>
					<PanelSection>
						<PanelSectionRow />
						<div style={{fontWeight: "bold"}}>Highlight Routes</div>
						<OptionToggle text="Selected Object Route" value={this.state.highlightSelectedRoute} name={"toggleHighlightSelectedRoute"} />
						<OptionToggle text="Incoming Routes" value={this.state.highlightEnroute} name={"toggleHighlightEnroute"} />
						<OptionToggle text="Incoming Routes (Transit)" value={this.state.highlightEnrouteTransit} name={"toggleHighlightEnrouteTransit"} />
						<OptionToggle text="Transit Passenger Routes" value={this.state.highlightPassengerRoutes} name={"toggleHighlightPassengerRoutes"} />

						<PanelSectionRow />
						<div style={{ fontWeight: "bold" }}>Highlight Buildings</div>
						<OptionToggle text="Passenger Destinations" value={this.state.highlightPassengerDestinations} name={"toggleHighlightPassengerDestinations"} />
						<OptionToggle text="Employee Residences" value={this.state.highlightEmployeeResidences} name={"toggleHighlightEmployeeResidences"} />
						<OptionToggle text="Residents' Workplaces" value={this.state.highlightResidentWorkplaces} name={"toggleHighlightResidentWorkplaces"} />
						<OptionToggle text="Students' Residences" value={this.state.highlightStudentResidences} name={"toggleHighlightStudentResidences"} />

						<PanelSectionRow />
						<div style={{ fontWeight: "bold" }}>Other Options</div>

						<PanelSectionRow />
						<OptionToggle text="Auto-refresh Selected" value={this.state.autoRefreshTransitingEntities} name={"toggleAutoRefresh"}/>
						<OptionToggle text="Show Stats" value={this.state.showStats} name={"toggleDebug"}/>
						

						<PanelSectionRow />
						{this.state.showStats && <div>
							<div style={{ display: "flex" }}>
								<div style={{ padding: "5rem" }}>
									En-route count
								</div>
								<div style={{ flex: "1", padding: "5rem" }} />
								<div style={{ paddingRight: "20rem" }}>
									{this.state.trackedEntityCount}
								</div>
							</div>
							<div style={{ display: "flex" }}>
								<div style={{ padding: "5rem" }}>
									Type
								</div>
								<div style={{ flex: "1", padding: "5rem" }} />
								<div style={{ paddingRight: "20rem" }}>
									{this.state.selectionType}
								</div>
							</div>
							<div style={{ display: "flex" }}>
								<div style={{ padding: "5rem" }}>
									Unique Segments
								</div>
								<div style={{ flex: "1", padding: "5rem" }} />
								<div style={{ paddingRight: "20rem" }}>
									{this.state.uniqueSegmentCount}
								</div>
							</div>
							<div style={{ display: "flex" }}>
								<div style={{ padding: "5rem" }}>
									Total Segments
								</div>
								<div style={{ flex: "1", padding: "5rem" }} />
								<div style={{ paddingRight: "20rem" }}>
									{this.state.totalSegmentCount}
								</div>
							</div>
							{this.state.routeTimeMs.map(kv => <div style={{ display: "flex" }}>
								<div style={{ padding: "5rem" }}>
									{kv[0]}
								</div>
								<div style={{ flex: "1", padding: "5rem" }} />
								<div style={{ paddingRight: "20rem" }}>
									{kv[1]}
								</div>
							</div>)}
						</div>}
					</PanelSection>
				</Panel>
			</div>}
		</div>
	}

	componentDidMount() {
		autoRefreshEntitiesBinding.subscribe(val => {
			this.setState({ autoRefreshTransitingEntities: val });
		});

		debugStatsBinding.subscribe(val => {
			this.setState({ showStats: val });
		});

		trackedEntityCountBinding.subscribe(val => {
			this.setState({ trackedEntityCount: val });
		})

		totalSegmentCountBinding.subscribe(val => {
			this.setState({ totalSegmentCount: val });
		})

		uniqueSegmentCountBinding.subscribe(val => {
			this.setState({ uniqueSegmentCount: val });
		})

		highlightSelectedRoute.subscribe(val => {
			this.setState({ highlightSelectedRoute: val });
		})

		highlightEnroute.subscribe(val => {
			this.setState({ highlightEnroute: val });
		})

		highlightEnrouteTransit.subscribe(val => {
			this.setState({ highlightEnrouteTransit: val });
		})

		highlightPassengerRoutes.subscribe(val => {
			this.setState({ highlightPassengerRoutes: val });
		})

		highlightPassengerDestinations.subscribe(val => {
			this.setState({ highlightPassengerDestinations: val });
		})

		highlightEmployeeResidences.subscribe(val => {
			this.setState({ highlightEmployeeResidences: val });
		})

		highlightResidentWorkplaces.subscribe(val => {
			this.setState({ highlightResidentWorkplaces: val });
		})

		highlightStudentResidences.subscribe(val => {
			this.setState({ highlightStudentResidences: val });
		})

		selectionTypeBinding.subscribe(val => {
			this.setState({ selectionType: val });
		})

		routeTimeBinding.subscribe(val => {
			this.setState({ routeTimeMs: HighlightOptionsMenuButton.parseBindings(val) });
		})
	}

	static parseBindings(stringList: string) {
		const results = stringList.split(":");

		return results.map(kv => kv.split(","));
	}
}

interface OptionsProps {
	value: boolean,
	name: string,
	text: string|null
}

class OptionToggle extends Component<OptionsProps> {
	render() {
		return <div style={{ display: "flex" }}>
			<div style={{ padding: "5rem" }}>
				{this.props.text}
			</div>
			<div style={{ flex: "1", padding: "5rem" }} />
			<div style={{ paddingRight: "20rem" }}>
				<Button selected={this.props.value} variant="flat" onSelect={() => {
					trigger("EmploymentTracker", this.props.name, this.props.value ? false : true);
				}}>
					<div style={{ padding: "5rem" }}>{this.props.value ? "On" : "Off"}</div>
				</Button>
			</div>
		</div>
	}
}