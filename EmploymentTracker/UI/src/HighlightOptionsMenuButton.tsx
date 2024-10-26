import { Button, FloatingButton, Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import tadaSrc from "./Traffic.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";
import { ToolTip, routeVolumeToolActive } from "./RouteVolumeButton";

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
export const toggleAll = bindValue<boolean>("EmploymentTracker", 'allToggled');
export const routeHighlightingToggled = bindValue<boolean>("EmploymentTracker", 'routeHighlightingToggled');
export const buildingsToggled = bindValue<boolean>("EmploymentTracker", 'buildingsToggled');

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

		toggleAll: toggleAll.value,
		buildingsToggled: routeHighlightingToggled.value,
		routeHighlightingToggled: buildingsToggled.value,
		routeVolumeToolActive: routeVolumeToolActive.value,

		/** @type {[string[]] */
		routeTimeMs: HighlightOptionsMenuButton.parseBindings(routeTimeBinding.value),
		hovering: false
	}

	render() {
		//TODO figure out why Panel component is too laggy; temporarily use div with manual styling
		return <>
			<FloatingButton selected={this.state.menuOpen} onSelect={() => {
				this.setState({ menuOpen: !this.state.menuOpen }); console.log("route menu open");
				
			}} onMouseEnter={() => {
				this.setState({ hovering: true })
			}} onMouseLeave={() => {
				this.setState({ hovering: false })
			}} />
			{this.state.hovering && <ToolTip text={"Route Highlighter Settings"} />}

			{this.state.menuOpen && <div style={{backgroundColor:"#183e69AA", position:"absolute", marginTop:"50rem", borderStyle:"solid", borderWidth:"1rem", borderColor:"lightblue", borderRadius:"5rem"} }>
				<PanelSection>
					<SectionHeader text="Route Highlighter" />
					<PanelSectionRow />
					<SectionHeader text="Quick-toggle"/>
					<OptionToggle text="All (shift+e)" value={this.state.toggleAll} name={"toggleAll"} />
					<OptionToggle text="Routes (shift+v)" value={this.state.routeHighlightingToggled} name={"quickToggleRouteHighlighting"} />
					<OptionToggle text="Buildings (shift+b)" value={this.state.buildingsToggled} name={"toggleBuildings"} />
					<OptionToggle text="Road Segment Tool (shift+r)" value={this.state.routeVolumeToolActive} name={"toggleRouteVolumeToolActive"} />

					<PanelSectionRow />
					<SectionHeader text="Highlight Routes"/>
					<OptionToggle text="Selected Object Route" value={this.state.highlightSelectedRoute} name={"toggleHighlightSelectedRoute"} />
					<OptionToggle text="Incoming Routes" value={this.state.highlightEnroute} name={"toggleHighlightEnroute"} />
					<OptionToggle text="Incoming Routes (Transit)" value={this.state.highlightEnrouteTransit} name={"toggleHighlightEnrouteTransit"} />
					<OptionToggle text="Transit Passenger Routes" value={this.state.highlightPassengerRoutes} name={"toggleHighlightPassengerRoutes"} />

					<PanelSectionRow />
					<SectionHeader text="Highlight Buildings" />
					<OptionToggle text="Passenger Destinations" value={this.state.highlightPassengerDestinations} name={"toggleHighlightPassengerDestinations"} />
					<OptionToggle text="Employee Residences" value={this.state.highlightEmployeeResidences} name={"toggleHighlightEmployeeResidences"} />
					<OptionToggle text="Residents' Workplaces" value={this.state.highlightResidentWorkplaces} name={"toggleHighlightResidentWorkplaces"} />
					<OptionToggle text="Students' Residences" value={this.state.highlightStudentResidences} name={"toggleHighlightStudentResidences"} />

					<PanelSectionRow />
					<div style={{ fontWeight: "bold" }}>Other Options</div>

					<PanelSectionRow />
					<OptionToggle text="Auto-refresh Selected" value={this.state.autoRefreshTransitingEntities} name={"toggleAutoRefresh"}/>
					<OptionToggle text="Show Stats" value={this.state.showStats} name={"toggleDebug"}/>
						

					{this.state.showStats && <div style={{fontSize:"12rem"} }>
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
			</div>}
		</>
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

		toggleAll.subscribe(val => {
			this.setState({ toggleAll: val });
		})

		routeHighlightingToggled.subscribe(val => {
			this.setState({ routeHighlightingToggled: val });
		})

		buildingsToggled.subscribe(val => {
			this.setState({ buildingsToggled: val });
		})

		selectionTypeBinding.subscribe(val => {
			this.setState({ selectionType: val });
		})

		routeTimeBinding.subscribe(val => {
			this.setState({ routeTimeMs: HighlightOptionsMenuButton.parseBindings(val) });
		})

		routeVolumeToolActive.subscribe(val => {
			this.setState({ routeVolumeToolActive: val });
		})
	}

	static parseBindings(stringList: string) {
		const results = stringList.split(":");

		return results.map(kv => kv.split(","));
	}
}

export function OptionToggle(props: { value: boolean, name: string, text: string | null }) {
	const { translate } = useLocalization();

	return <div style={{ display: "flex" }}>
		<div style={{ padding: "5rem" }}>
			{props.text && translate("EmploymentTracker_" + props.text, props.text)}
		</div>
		<div style={{ flex: "1", padding: "5rem" }} />
		<div style={{ paddingRight: "20rem" }}>
			<Button selected={props.value} variant="flat" onSelect={() => {
				trigger("EmploymentTracker", props.name, props.value ? false : true);
			}}>
				<div style={{ padding: "5rem" }}>{props.value ? translate("EmploymentTracker_" + "On", "On") : translate("EmploymentTracker_" + "Off", "Off")}</div>
			</Button>
		</div>
	</div>
}

function SectionHeader(props: { text: string }) {
	const { translate } = useLocalization();
	return <div style={{ fontWeight: "bold" }}>{translate("EmploymentTracker_" + props.text, props.text)}</div>
}