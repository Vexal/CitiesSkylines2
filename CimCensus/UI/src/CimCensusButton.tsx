import { Button, FloatingButton, Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import tadaSrc from "./Citizen.svg";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";
import styles from "cimcensusbutton.module.scss"


/** @type {string} */
export const MOD_NAME = "CimCensus";

export const dataBindings = bindValue<string>(MOD_NAME, 'dataBindings');

export default class CimCensusButton extends Component {
	state = {
		menuOpen: false,
		/** @type {[string[]] */
		dataBindings: CimCensusButton.parseBindings(dataBindings.value),
		hovering: false
	}

	render() {
		//TODO figure out why Panel component is too laggy; temporarily use div with manual styling
		return <>
			<FloatingButton src={tadaSrc} selected={this.state.menuOpen} onSelect={() => {
				this.setState({ menuOpen: !this.state.menuOpen }); console.log("census menu open");
				
			}} onMouseEnter={() => {
				this.setState({ hovering: true })
			}} onMouseLeave={() => {
				this.setState({ hovering: false })
			}} />
			{this.state.hovering && <ToolTip text={"Route Highlighter Settings"} />}

			{this.state.menuOpen && <div className={styles.cimCensusPanel}>
				<SectionHeader text="Cim Census" />
				<div className={styles.cimCensusPanelBody}>
					<PanelSection>
						<PanelSectionRow />
						{this.state.dataBindings.map(data => <DataItem key={data[0]} name={data[0]} text={data[1]} />)}
					
					</PanelSection>
				</div>
			</div>}
		</>
	}

	/*
	{this.state.routeTimeMs.map(kv => <div style={{ display: "flex" }}>
						<div style={{ padding: "5rem" }}>
							{kv[0]}
						</div>
						<div style={{ flex: "1", padding: "5rem" }} />
						<div style={{ paddingRight: "20rem" }}>
							{kv[1]}
						</div>
					</div>)}
	*/
	componentDidMount() {
		dataBindings.subscribe(val => {
			this.setState({ dataBindings: CimCensusButton.parseBindings(val) });
		})
	}

	static parseBindings(stringList: string) {
		const results = stringList.split(":");

		return results.map(kv => kv.split(","));
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

export function OptionToggle(props: { value: boolean, name: string, text: string | null }) {
	const { translate } = useLocalization();

	return <div style={{ display: "flex" }}>
		<div style={{ padding: "5rem" }}>
			{props.text && translate(MOD_NAME + props.text, props.text)}
		</div>
		<div style={{ flex: "1", padding: "5rem" }} />
		<div style={{ paddingRight: "20rem" }}>
			<Button selected={props.value} variant="flat" onSelect={() => {
				trigger(MOD_NAME, props.name, props.value ? false : true);
			}}>
				<div style={{ padding: "5rem" }}>{props.value ? translate(MOD_NAME + "On", "On") : translate(MOD_NAME + "Off", "Off")}</div>
			</Button>
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