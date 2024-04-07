import { Button, FloatingButton, MenuButton, Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import tadaSrc from "./Traffic.svg";
import iconStyles from "./icon.module.scss";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";

export const autoRefreshEntitiesBinding = bindValue<boolean>("EmploymentTracker", 'AutoRefreshTransitingEntitiesActive');
export const debugStatsBinding = bindValue<boolean>("EmploymentTracker", 'DebugActive');
export const trackedEntityCountBinding = bindValue<number>("EmploymentTracker", 'TrackedEntityCount');
export const undupedEntityCountBinding = bindValue<number>("EmploymentTracker", 'UndupedEntityCount');
export const totalSegmentCountBinding = bindValue<number>("EmploymentTracker", 'TotalSegmentCount');
export const uniqueSegmentCountBinding = bindValue<number>("EmploymentTracker", 'UniqueSegmentCount');
export const routeTimeBinding = bindValue<string>("EmploymentTracker", 'RouteTimeMs');


export const highlightEnroute = bindValue<boolean>("EmploymentTracker", 'highlightEnroute');
export const highlightSelectedRoute = bindValue<boolean>("EmploymentTracker", 'highlightSelectedRoute');

export default class HighlightOptionsMenuButton extends Component {
	state = {
		menuOpen: false,
		autoRefreshTransitingEntities: autoRefreshEntitiesBinding.value,
		showStats: debugStatsBinding.value,
		trackedEntityCount: trackedEntityCountBinding.value,
		undupedEntityCount: undupedEntityCountBinding.value,
		uniqueSegmentCount: uniqueSegmentCountBinding.value,
		totalSegmentCount: totalSegmentCountBinding.value,

		highlightEnroute: highlightEnroute.value,
		highlightSelectedRoute: highlightSelectedRoute.value,

		/** @type {[string[]] */
		routeTimeMs: HighlightOptionsMenuButton.parseBindings(routeTimeBinding.value),
	}

	render() {
		return <div>

			<FloatingButton src={tadaSrc} selected={this.state.menuOpen} onSelect={() => {
				this.setState({ menuOpen: !this.state.menuOpen }); console.log("route menu open");
				
			}} />

			{this.state.menuOpen && <div>
				<Panel>
					<PanelSection>
						<PanelSectionRow />
						<div>Route Highlight Options</div>
						<OptionToggle value={this.state.highlightEnroute} name={"highlightEnroute"} />
						<OptionToggle value={this.state.highlightSelectedRoute} name={"highlightSelectedRoute"} />

						<PanelSectionRow />
						<OptionToggle value={this.state.autoRefreshTransitingEntities} name={"toggleAutoRefresh"} />

						<PanelSectionRow />
						<OptionToggle value={this.state.autoRefreshTransitingEntities} name={"toggleAutoRefresh"}/>
						<OptionToggle value={this.state.showStats} name={"toggleDebug"}/>
						

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
									Unduped entities
								</div>
								<div style={{ flex: "1", padding: "5rem" }} />
								<div style={{ paddingRight: "20rem" }}>
									{this.state.undupedEntityCount}
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

		undupedEntityCountBinding.subscribe(val => {
			this.setState({ undupedEntityCount: val });
		})

		highlightSelectedRoute.subscribe(val => {
			this.setState({ highlightSelectedRoute: val });
		})

		highlightEnroute.subscribe(val => {
			this.setState({ highlightEnroute: val });
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

interface IProps {
	value: boolean,
	name: string
}

class OptionToggle extends Component<IProps> {
	/*static propTypes = {
		value: PropTypes.bool.isRequired,
		name: PropTypes.string.isRequired
	}*/

	render() {
		return <div style={{ display: "flex" }}>
			<div style={{ padding: "5rem" }}>
				Auto-refresh entity selection
			</div>
			<div style={{ flex: "1", padding: "5rem" }} />
			<div style={{ paddingRight: "20rem" }}>
				<Button selected={this.props.value} variant="flat" onSelect={() => {
					trigger("EmploymentTracker", this.props.name, this.props.value ? "false" : "true");
				}}>
					<div style={{ padding: "5rem" }}>{this.props.value ? "On" : "Off"}</div>
				</Button>
			</div>
		</div>
	}
}