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

export default class HighlightOptionsMenuButton extends Component {
	state = {
		menuOpen: false,
		autoRefreshTransitingEntities: autoRefreshEntitiesBinding.value,
		showStats: debugStatsBinding.value,
		trackedEntityCount: trackedEntityCountBinding.value,
		undupedEntityCount: undupedEntityCountBinding.value,
		uniqueSegmentCount: uniqueSegmentCountBinding.value,
		totalSegmentCount: totalSegmentCountBinding.value,
	}

	render() {
		return <div>

			<FloatingButton src={tadaSrc} selected={this.state.menuOpen} onSelect={() => {
				this.setState({ menuOpen: !this.state.menuOpen }); console.log("route menu open");
				
			}} />

			{this.state.menuOpen && <div>
				<Panel>
					<PanelSection>
						<PanelSectionRow  />
						<div style={{ display: "flex" }}>
							<div style={{ padding: "5rem" }}>
								Auto-refresh entity selection
							</div>
							<div style={{ flex: "1", padding:"5rem" }} />
							<div style={{paddingRight:"20rem"} }>
								<Button selected={this.state.autoRefreshTransitingEntities} variant="flat" onSelect={() => {
									trigger("EmploymentTracker", "toggleAutoRefresh", this.state.autoRefreshTransitingEntities ? "false" : "true");
								}}>
									<div style={{padding:"5rem"} }>{this.state.autoRefreshTransitingEntities ? "On" : "Off"}</div>
								</Button>
							</div>
						</div>

						<div style={{ display: "flex" }}>
							<div style={{ padding: "5rem" }}>
								Show Stats
							</div>
							<div style={{ flex: "1", padding:"5rem" }} />
							<div style={{paddingRight:"20rem"} }>
								<Button selected={this.state.showStats} variant="flat" onSelect={() => {
									trigger("EmploymentTracker", "toggleDebug", this.state.showStats ? "false" : "true");
								}}>
									<div style={{ padding: "5rem" }}>{this.state.showStats ? "On" : "Off"}</div>
								</Button>
							</div>
						</div>
					
						<PanelSectionRow />
						{this.state.showStats && <div>
							<div style={{ display: "flex" }}>
								<div style={{ padding: "5rem" }}>
									In-transit count
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
	}
}