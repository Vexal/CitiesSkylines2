import { FloatingButton } from "cs2/ui";
import tadaSrc from "./Roads.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";


export const laneIdList = bindValue<string>("EmploymentTracker", 'laneIdList');

export default class RouteLaneInfoPanel extends Component {
	state = {
		laneIdList: laneIdList.value,
		hovering: false
	}

	render() {
		return this.state.laneIdList && <div>tst</div>

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
		laneIdList.subscribe(val => {
			console.log("lane ids", val);
			this.setState({ laneIdList: val });
		})
	}
}