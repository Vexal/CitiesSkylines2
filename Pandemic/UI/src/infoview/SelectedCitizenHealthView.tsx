import Disease from "../model/Disease";
import { VanillaComponentResolver } from "../mods/VanillaComponentResolver";
import { InfoRow, InfoRowTheme, InfoSection, InfoSectionTheme } from "../mods/VanillaComponents";

interface InfoSectionComponent {
	group: string;
	tooltipKeys: Array<string>;
	tooltipTags: Array<string>;
}

export const SelectedCitizenHealthView = (componentList: any): any => {
	// I believe you should not put anything here.
	componentList["Pandemic.HealthInfoUISystem"] = (e: InfoSectionComponent) => {
		const disease = JSON.parse(e.group);
		if (!disease.strainName) {
			return null;
		}
		console.log("this text?", e);
		// These get the value of the bindings.
		return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={"Health"}
				right={disease.health }
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			<InfoRow
				left={"Disease"}
				right=
				{
					disease.diseaseName
				}
				uppercase={false}
				disableFocus={true}
				subRow={true}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			<InfoRow
				left={"Strain"}
				right=
				{
					disease.strainName
				}
				uppercase={false}
				disableFocus={true}
				subRow={true}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			<InfoRow
				left={"Stage"}
				right=
				{
					Disease.diseaseStage(disease.diseaseProgression)
				}
				uppercase={false}
				disableFocus={true}
				subRow={true}
				className={InfoRowTheme.infoRow}
			></InfoRow>
		</InfoSection>
			;
	}

	return componentList as any;
}