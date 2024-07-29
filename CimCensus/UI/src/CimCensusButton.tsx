import { Button, FloatingButton, Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import tadaSrc from "./Citizen.svg";
import pop from "./Population.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";
import styles from "cimcensusbutton.module.scss"


/** @type {string} */
export const MOD_NAME = "CimCensus";

export const dataBindings = bindValue<string>(MOD_NAME, 'dataBindings');
export const autoRefreshActive = bindValue<boolean>(MOD_NAME, 'autoRefreshActive');

export default class CimCensusButton extends Component {
	state = {
		menuOpen: false,
		/** @type {[string[]] */
		dataBindings: CimCensusButton.parseBindings(dataBindings.value),
		hovering: false,
		autoRefresh: autoRefreshActive.value
	}

	render() {
		//TODO figure out why Panel component is too laggy; temporarily use div with manual styling
		return <>
			<FloatingButton src={tadaSrc} selected={this.state.menuOpen} onSelect={() => {
				const shouldOpen = !this.state.menuOpen;
				trigger(MOD_NAME, "update", shouldOpen);
				if (!shouldOpen) {
					trigger(MOD_NAME, "toggle", false);
				} else if (shouldOpen && this.state.autoRefresh) {
					trigger(MOD_NAME, "toggle", true);
				}
				this.setState({ menuOpen:  shouldOpen});
			}} onMouseEnter={() => {
				this.setState({ hovering: true })
			}} onMouseLeave={() => {
				this.setState({ hovering: false })
			}} />
			{this.state.hovering && <ToolTip text={"Cim Census"} />}

			{this.state.menuOpen && <div className={styles.cimCensusPanel}>
				<SectionHeader text="Cim Census" />
				<div className={styles.cimCensusPanelBody}>
					<PanelSection>
						<div style={{
							backgroundColor: "rgba(24, 33, 51, 0.599922)"
						}} />
						<div style={{ display: "flex" }}>
							<img src={pop} style={{verticalAlign:"center", alignSelf:"center", maxHeight:"45rem"}} />
							<div style={{ flex: "1" }} />
							<div style={{marginRight:"10rem"} }>
								<Button selected={false} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "update", true);
								}}>
									<div style={{ padding: "4rem" }}>{"Refresh"}</div>
								</Button>
							</div>
							<div>
								<Button selected={this.state.autoRefresh} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "toggle", this.state.autoRefresh ? false : true);
									this.setState({ autoRefresh: !this.state.autoRefresh });
								}}>
									<div style={{ padding: "4rem" }}>{"Auto-refresh"}</div>
								</Button>
							</div>
						</div>
						<PanelSectionRow />
						{this.state.dataBindings.map(data => <DataItem key={data[0]} name={data[0]} text={data[1]} />)}
						{false && <DataSection dataCategory="Cims">

							<DataSection dataCategory="Cims">

							</DataSection>
							<DataSection dataCategory="Cims">

							</DataSection>
						</DataSection>}
					</PanelSection>
				</div>
			</div>}
		</>
	}

	componentDidMount() {
		dataBindings.subscribe(val => {
			this.setState({ dataBindings: CimCensusButton.parseBindings(val) });
		})
		autoRefreshActive.subscribe(val => {
			this.setState({ autoRefreshActive: autoRefreshActive.value });
		})
	}

	getCimsData = () => {
		const results = [];
		for (let i = 0; i < this.state.dataBindings.length; ++i) {
			if (this.state.dataBindings[i][0].indexOf("Cims - ") === 0) {
				results.push({ name: this.state.dataBindings[i][0].substring("Cims - ".length), value: this.state.dataBindings[i][1] });
			}
		}

		return results;
	}

	static parseBindings(stringList: string) {
		const results = stringList.split(":");

		return results.map(kv => kv.split(","));
	}
}

type DSProps = {
	dataCategory: string,
	children:any[]
}

class DataSection extends Component<DSProps, any> {
	render() {
		return <div>
			{this.props.children && this.props.children.map(subCategory => <div>
				{subCategory }
			</div>)}
		</div>
	}
}

export function DataItem(props: { name: string, text: string | null }) {
	return <div style={{ display: "flex" }}>
		<div style={{ paddingLeft: "5rem" }}>
			<LocalizedText text={props.name} />
		</div>
		<div style={{ flex: "1" }} />
		<div style={{ paddingRight: "20rem" }}>
			{props.text}
		</div>
	</div>
}

function SectionHeader(props: { text: string }) {
	const { translate } = useLocalization();
	return <div className={styles.cimCensusPanelHeader} style={{ fontWeight: "bold" }}>{translate(MOD_NAME + props.text, props.text)}</div>
}

export function LocalizedText(props: { text: string }) {
	const { translate } = useLocalization();
	return <>{translate(MOD_NAME + props.text, props.text)}</>
}

export function ToolTip(props: { text: string }) {
	const { translate } = useLocalization();
	return <div style={{ fontWeight: "bold", position: "absolute", marginTop: "43rem", maxWidth: "200rem", color: "white", background: "black" }}>{translate(MOD_NAME + props.text, props.text)}</div>
}