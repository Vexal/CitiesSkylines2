import { ModRegistrar, ModuleRegistryExtend } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world";
import { InfomodeItem, InfoviewPanelLabel, InfoviewLabelsTheme, InfoviewPanelSection, InfoviewPanelSectionTheme, SelectedInfoPanelTogglesComponent, InfoRowTheme } from "./SelectedInfoPanelTogglesComponent";
import { VanillaComponentResolver } from "./VanillaComponentResolver";
import { bindValue } from "cs2/api";
import { Component } from "react";
import { InfoRow } from "cs2/ui";

export const diseaseList = bindValue<any[]>("Pandemic", 'diseases');
export const currentInfectionCount = bindValue<string[]>("Pandemic", 'currentInfectionCount');
class DiseaseInfoPanel extends Component {
	state = {
		diseaseList: diseaseList.value,
		currentInfectionCount: patientCountMap(currentInfectionCount.value)
	}
	render() {
		console.log("the disease list", InfoviewPanelLabel, InfoviewPanelSectionTheme, this.state.diseaseList, this.state.currentInfectionCount);
		return <div>
			<InfoviewPanelSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoviewPanelSectionTheme.infoviewPanelSection}>
				<InfoviewPanelLabel uppercase={true} text={"Active Disease Strains"}></InfoviewPanelLabel>
				{this.state.diseaseList?.map(disease => <><InfoviewPanelLabel
					small={1}
					text={DiseaseInfoPanel.strainHeader(disease)}
				/>
					<InfoviewPanelLabel
						text={"Current Sick"}
						tiny={1}
						rightText={this.state.currentInfectionCount[disease.uniqueKey]}
					/>
					<InfoviewPanelLabel
						text={"Total Sick"}
						tiny={1}
						rightText={disease.infectionCount}
					/>
				</>)}
			</InfoviewPanelSection>
		</div>
	}

	componentDidMount() {
		diseaseList.subscribe(val => {
			this.setState({ diseaseList: val });
		});
		currentInfectionCount.subscribe(val => {
			this.setState({ currentInfectionCount: patientCountMap(val) });
		});
	}

	static patientCountMap = (countList) => {
		const results = {};
		for (let i = 0; i < countList.length; ++i) {
			const c = countList[i].split("_");
			results[c[0]] = c[1]
		} 

		return results;
	}

	static strainHeader(disease) {
		return (disease.type == 1 ? "Common Cold" : "Flu") + " " + strainName(disease);
	}
}

function patientCountMap(countList) {
	const results = {};
	for (let i = 0; i < countList.length; ++i) {
		const c = countList[i].split("_");
		results[c[0]] = c[1]
	}

	return results;
}

function strainName(disease: any) {
	return (disease.type == 1 ? "CC" : "FL") + disease.createYear + "." + (disease.createMonth + 2) + "." + (disease.createHour) + "." + (disease.createMinute)
}
const TestComponent: ModuleRegistryExtend = (Component) => {
	return (props) => {
		const { children, ...otherProps } = props || {};
		console.log("the children", { children });
		return (
			<>
				<Component {...otherProps} >{children}</Component>
				<DiseaseInfoPanel/>
			</>
		);
	}
}

const register: ModRegistrar = (moduleRegistry) => {
	console.log("test!");
	VanillaComponentResolver.setRegistry(moduleRegistry);
	moduleRegistry.append('Menu', HelloWorldComponent);
	console.log("test!2");
	//moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/active-infoview-panel.tsx", 'activeInfoviewPanel', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/components/sections/infoview-panel-section.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.extend("game-ui/game/components/infoviews/infoview-menu.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.override("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'HealthcareInfoviewPanel', SelectedInfoPanelTogglesComponent);
	
	//moduleRegistry.append("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'HealthcareInfoviewPanel', SelectedInfoPanelTogglesComponent);
	moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'HealthcareInfoviewPanel', TestComponent);

	console.log("Pandemic UI module registrations completed.");
}

export default register;