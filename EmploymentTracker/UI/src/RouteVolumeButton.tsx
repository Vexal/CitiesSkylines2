import { FloatingButton, PanelSection } from "cs2/ui";
import tadaSrc from "./Roads.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";

export const routeVolumeToolActive = bindValue<boolean>("EmploymentTracker", 'routeVolumeToolActive');
export const laneIdList = bindValue<string>("EmploymentTracker", 'laneIdList');

export default class RouteVolumeButton extends Component {
	state = {
		routeVolumeToolActive: routeVolumeToolActive.value,
		laneIdList: laneIdList.value,
		activeLanes: laneIdList.value.split(",").map(v => v === "true" ? true : false),
		hovering: false
	}

	render() {
		return <> <FloatingButton src={tadaSrc} selected={this.state.routeVolumeToolActive} onSelect={() => {
			//this.setState({ menuOpen: !this.state.menuOpen });
			trigger("EmploymentTracker", "toggleRouteVolumeToolActive", !this.state.routeVolumeToolActive);
		}} onMouseEnter={() => {
			this.setState({ hovering: true })
		}} onMouseLeave={() => {
			this.setState({ hovering: false })
			}} />
			{this.state.hovering && <ToolTip text={"Route Highlighter Road Segment Tool (shift+r)"} />}

			{this.state.routeVolumeToolActive && this.state.laneIdList.length > 0 && <div style={{ backgroundColor: "#183e69AA", borderStyle: "solid", borderWidth: "1rem", borderColor: "lightblue", borderRadius: "5rem" }}>
				<PanelSection>
					
				</PanelSection>
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
		laneIdList.subscribe(val => {
			console.log("lane ids", val);
			this.setState({ laneIdList: val });
		});
	}
}

export function ToolTip(props: { text: string}) {
	const { translate } = useLocalization();
	return <div style={{ fontWeight: "bold", position: "absolute", marginTop: "43rem", maxWidth:"200rem", color:"white", background:"black" }}>{translate("EmploymentTracker_" + props.text, props.text)}</div>
}