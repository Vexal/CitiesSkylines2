import tadaSrc from "./FireSafetySprinklerSystem.svg";
import iconStyles from "./icon.module.scss";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";

export const fireToolActiveBinding = bindValue<boolean>("FireStarter", 'FireToolActive');
export const fireToolSettingActiveBinding = bindValue<boolean>("FireStarter", 'FireToolSettingActive');

export default class TestButton extends Component {
	state = {
		active: fireToolActiveBinding.value,
		toolAvailable: fireToolSettingActiveBinding.value
	}

	render() {
		if (!this.state.toolAvailable) {
			return null;
		}

		return <div className={iconStyles.fireButton + " " +  (this.state.active ? iconStyles.fireActive : "")}>

			<img src={tadaSrc} className={iconStyles.fireButton.fireIcon} onClick={() => {

				trigger("FireStarter", "test", "test other button " + fireToolActiveBinding.value);
				this.setState({ active: fireToolActiveBinding.value });
			}}>
			</img>
			<div className={iconStyles.fireText}>{this.state.active ? "Fire On" : "Fire"}</div>
		</div>
	}

	componentDidMount() {
		fireToolSettingActiveBinding.subscribe(val => {
			this.setState({ toolAvailable: val });
		})
	}
}