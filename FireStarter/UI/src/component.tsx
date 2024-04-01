import { FloatingButton } from "cs2/ui";
//import workerSrc from "./construction-worker.svg";
import tadaSrc from "./FireSafetySprinklerSystem.svg";
import s2 from "./AdvancedFurnace.svg";
import s22 from "./AirPollution.svg";
import iconStyles from "./icon.module.scss";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";

export const fireToolActiveBinding = bindValue<boolean>("FireStarter", 'FireToolActive');

export default class TestButton extends Component {
	state = {
		active: false
	}

	render() {
			/*<FloatingButton src={tadaSrc} />
			<FloatingButton src={s22} selected={ this.state.active} onSelect={() => {
				//trigger("FireStarter", "test", "testarg");
			}} />*/
		return <div>

			<img style={{cursor: "pointer"} } src={tadaSrc} className={iconStyles.icon} onClick={() => {

				trigger("FireStarter", "test", "test other button " + fireToolActiveBinding.value);
				this.setState({ active: fireToolActiveBinding.value });
			}}>
			</img>
			<div style={{color: this.state.active ? "green" : "red"} }>{this.state.active ? "Fire On" : "Fire Off"}</div>
		</div>
	}
}

/*export const TestButton = () => (


	<div>
		<FloatingButton src={tadaSrc}/>
		<FloatingButton src={s22} onSelect={() => {
			trigger("FireStarter", "test", "testarg");
		}} />

		<img src={tadaSrc} className={iconStyles.icon} onClick={() => {

			trigger("FireStarter", "test", "test other button");
		}}>
		</img>
		<div >test</div>
	</div>
)*/