// @ts-nocheck
import { Button, FloatingButton, PanelSection } from "cs2/ui";
import tadaSrc from "./Roads.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";

export const routeVolumeToolActive = bindValue<boolean>("EmploymentTracker", 'routeVolumeToolActive');
export const laneSelectionActive = bindValue<boolean>("EmploymentTracker", 'laneSelectionActive');
export const laneIdList = bindValue<string>("EmploymentTracker", 'laneIdList');

const panelStyle = { backgroundColor: "#183e69AA", borderStyle: "solid", borderWidth: "1rem", borderColor: "lightblue", borderRadius: "5rem" };

export default class RouteVolumeButton extends Component {
	state = {
		routeVolumeToolActive: routeVolumeToolActive.value,
		laneIdList: laneIdList.value,
		activeLanes: laneIdList.value.split(",").map(v => v === "1" ? true : false),
		hovering: false,
		hoverLane: -1,
		laneSelectionActive: laneSelectionActive.value
	}

	render() {
		return <>
			<FloatingButton
				
				selected={this.state.routeVolumeToolActive}
				onSelect={() => { trigger("EmploymentTracker", "toggleRouteVolumeToolActive", !this.state.routeVolumeToolActive); }}
				onMouseEnter={() => { this.setState({ hovering: true }) }}
				onMouseLeave={() => { this.setState({ hovering: false }) }}
			/>

			{this.state.hovering && <ToolTip text={"Route Highlighter Road Segment Tool (shift+r)"} />}

			{this.state.routeVolumeToolActive && this.state.laneIdList.length > 0 &&	
				<div style={panelStyle}>
					<PanelSection>
						<Button variant="flat" selected={this.state.laneSelectionActive} onSelect={() => {
							trigger("EmploymentTracker", "toggleLaneSelection", !this.state.laneSelectionActive);
						}}>
							<div style={{padding:"2rem"} }><LocalizedText text="Lane Selection" /></div>
						</Button>
					</PanelSection>
					{this.state.laneSelectionActive && <PanelSection>
						<div style={{ display: "flex", flexDirection: "row", flexWrap: "wrap", width: "90rem" }}>
							{this.state.activeLanes.map((active, index) => {
								const rowDisplay = { display: "flex", fontSize: "12rem", width: "25%" };
								if (this.state.hoverLane === index) {
									rowDisplay["backgroundColor"] = "lightgreen";
								}

								return (
									<div style={rowDisplay} onMouseEnter={() => {
										this.setState({ hoverLane: index });
										trigger("EmploymentTracker", "toggleHoverLane", index);
									}} onMouseLeave={() => {
										this.setState({ hoverLane: -1 });
										trigger("EmploymentTracker", "toggleHoverLane", -1);
									}}>
										<div>
											<Button selected={active} variant="flat" onSelect={() => {
												trigger("EmploymentTracker", "toggleActiveLanes", this.updateSelection(index, !active));
											}}>
												<div style={{ padding: "2rem", width: "20rem" }}>{index}</div>
											</Button>
										</div>
									</div>
								);
							})}
						</div>
					</PanelSection>}
				</div>}
		</>

		/*return <div className={iconStyles.fireButton + " " + (this.state.routeVolumeToolActive ? iconStyles.fireActive : "")}>

			<img src={tadaSrc} className={iconStyles.fireButton.fireIcon} onClick={() => {
				const newVal = !this.state.routeVolumeToolActive;
				trigger("EmploymentTracker", "toggleRouteVolumeToolActive", newVal);
				//this.setState({ routeVolumeToolActive: newVal });
			}}>
			</img>
		</div>*/
	}

	componentDidMount() {
		routeVolumeToolActive.subscribe(val => {
			this.setState({ routeVolumeToolActive: val });
		});
		laneSelectionActive.subscribe(val => {
			console.log("lsa", val);
			this.setState({ laneSelectionActive: val });
		});
		laneIdList.subscribe(val => {
			console.log("lane ids", val);
			this.setState({ laneIdList: val, activeLanes: val.split(",").map(v => v === "1" ? true : false) });
		});
	}

	/** @returns {string} */
	updateSelection(laneId: number, toggled: boolean) {
		/** @type {string[]} */
		const arr = [];
		for (let i = 0; i < this.state.activeLanes.length; ++i) {
			if (i == laneId) {
				arr.push(toggled ? "1" : "0");
			} else {
				arr.push(this.state.activeLanes[i] ? "1" : "0");
			}
		}

		return arr.join(",");
	}
}

export function LocalizedText(props: { text: string }) {
	const { translate } = useLocalization();
	return <>{translate("EmploymentTracker_" + props.text, props.text)}</>
}

export function ToolTip(props: { text: string}) {
	const { translate } = useLocalization();
	return <div style={{ fontWeight: "bold", position: "absolute", marginTop: "43rem", maxWidth:"200rem", color:"white", background:"black" }}>{translate("EmploymentTracker_" + props.text, props.text)}</div>
}