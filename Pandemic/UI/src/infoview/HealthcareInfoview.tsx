import { bindValue } from "cs2/api";
import Disease from "../model/Disease";
import { Component } from "react";
import { VanillaComponentResolver } from "../mods/VanillaComponentResolver";
import styles from "../pandemic.module.scss"
import { InfoviewPanelLabel, InfoviewPanelSection, InfoviewPanelSectionTheme } from "../mods/VanillaComponents";
import CustomBindings from "../model/CustomBindings";

export const diseaseList = bindValue<any[]>("Pandemic", 'diseases');
export const mutationCooldown = bindValue<number>("Pandemic", 'mutationCooldown');



function DiseaseDetailsPanel(props: { disease: Disease, expanded: boolean, currentInfectionCount: number }) {
	return <>
		<InfoviewPanelLabel
			text={"Current / Total Sick"}
			tiny={1}
			rightText={props.currentInfectionCount + " / " + props.disease.totalInfectionCount}
		/>
		{props.expanded && <>
			<InfoviewPanelLabel
				text={"Parent Strain"}
				tiny={1}
				rightText={props.disease.parentStrain}
			/>
			<InfoviewPanelLabel
				text={"Spread Chance"}
				tiny={1}
				rightText={props.disease.baseSpreadChance + "% / s"}
			/>
			<InfoviewPanelLabel
				text={"Spread Radius"}
				tiny={1}
				rightText={props.disease.baseSpreadRadius}
			/>
			<InfoviewPanelLabel
				text={"Health Impact"}
				tiny={1}
				rightText={props.disease.baseHealthPenalty}
			/>
			<InfoviewPanelLabel
				text={"Lethality"}
				tiny={1}
				rightText={props.disease.baseDeathChance + "% / s"}
			/>
			<InfoviewPanelLabel
				text={"Progression Speed"}
				tiny={1}
				rightText={props.disease.progressionSpeed + "% / s"}
			/>
			<InfoviewPanelLabel
				text={"Mutation Chance"}
				tiny={1}
				rightText={props.disease.mutationChance + "%"}
			/>
			<InfoviewPanelLabel
				text={"Mutation Variance"}
				tiny={1}
				rightText={props.disease.mutationMagnitude + "%"}
			/>
		</>}
	</>
}

export class DiseaseInfoPanel extends Component {
	state = {
		/** @type {Disease[]} */
		diseaseList: null,// DiseaseInfoPanel.buildDiseaseList(diseaseList.value, patientCountMap(currentInfectionCount.value)),
		diseaseMap: null,
		currentInfectionCount: null,// patientCountMap(currentInfectionCount.value),
		mutationCooldown: 0,
		activeOnly: true,
		expandedDisease: {}
	}

	render() {
		console.log("the disease list", InfoviewPanelLabel, InfoviewPanelSectionTheme, this.state.diseaseList, this.state.currentInfectionCount);
		return <div>
			<InfoviewPanelSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoviewPanelSectionTheme.infoviewPanelSection}>
				<InfoviewPanelLabel uppercase={true} text={"Active Disease Strains"} rightText={this.state.mutationCooldown}></InfoviewPanelLabel>
				{this.state.diseaseList?.filter((disease: Disease) => !this.state.activeOnly || this.state.currentInfectionCount[disease.uniqueKey] > 0).map((disease: Disease) => <><InfoviewPanelLabel
					small={1}
					text={disease.strainHeader}
					rightText={<div onClick={e => this.toggleExpanded(disease)} className={styles.diseaseDetailsButton + " " + (this.isExpanded(disease) ? styles.expanded : "")}>{this.isExpanded(disease) ? "Hide Details" : "Show Details"}</div>}
				/>
					<DiseaseDetailsPanel disease={disease} expanded={this.isExpanded(disease)} currentInfectionCount={this.state.currentInfectionCount[disease.uniqueKey]} />
				</>)}
			</InfoviewPanelSection>
		</div>
	}

	componentDidMount() {
		const pcm = DiseaseInfoPanel.patientCountMap(CustomBindings.currentInfectionCount.value);
		const diseaseData = Disease.buildDiseaseList(diseaseList.value);
		this.setState({ diseaseList: diseaseData.diseaseList, currentInfectionCount: pcm, diseaseMap: diseaseData.diseaseMap }, () => {

			diseaseList.subscribe(val => {
				const newDiseaseData = Disease.buildDiseaseList(val);
				//console.log("new disease list", newDiseaseData);
				this.setState({ diseaseList: newDiseaseData.diseaseList, diseaseMap: newDiseaseData.diseaseMap });
			});
			CustomBindings.currentInfectionCount.subscribe(val => {
				const npcm = DiseaseInfoPanel.patientCountMap(val);
				this.setState({ currentInfectionCount: npcm });
			});
		});

		mutationCooldown.subscribe(val => {
			this.setState({ mutationCooldown: val });
		});
	}

	isExpanded(disease: Disease): boolean {
		return this.state.expandedDisease[disease.uniqueKey] === true;
	}

	toggleExpanded(disease: Disease) {
		const shouldExpand = !this.isExpanded(disease);
		this.setState({ expandedDisease: { ...this.state.expandedDisease, [disease.uniqueKey]: shouldExpand } });
	}

	static patientCountMap = (countList) => {
		const results = {};
		for (let i = 0; i < countList.length; ++i) {
			const c = countList[i].split("_");
			results[c[0]] = c[1]
		}

		return results;
	}
}