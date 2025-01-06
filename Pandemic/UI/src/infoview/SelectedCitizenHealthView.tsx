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
		const parts = e.group.split(",");
		// These get the value of the bindings.
		return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={parts[0]}
				right=
				{
					parts[1]
				}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
		</InfoSection>
			;
	}

	return componentList as any;
}