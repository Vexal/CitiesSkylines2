import { getModule } from "cs2/modding";
import { Theme, FocusKey, UniqueFocusKey } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { VanillaComponentResolver } from "./VanillaComponentResolver";


interface InfoSectionComponent {
	group: string;
	tooltipKeys: Array<string>;
	tooltipTags: Array<string>;
}

export const InfoSectionTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.module.scss",
	"classes"
);

export const InfoRowTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.module.scss",
	"classes"
)

export const InfoSection: any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.tsx",
	"InfoSection"
)

export const InfoRow: any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx",
	"InfoRow"
)


const InfomodeItemTheme: Theme | any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/infomode-item/infomode-item.module.scss",
	"classes"
)

/*
const InfoviewPanelSpaceTheme: Theme | any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/infoview-panel-space.module.scss",
	"classes"
)

*/
export const InfoviewLabelsTheme: Theme | any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/labels/labels.module.scss",
	"classes"
)


export const InfoviewPanelSectionTheme: Theme | any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/sections/infoview-panel-section.module.scss",
	"classes"
)


export const InfomodeItem: any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/infomode-item/infomode-item.tsx",
	"InfomodeItem"
)

/*
const InfoViewPanelSpace: any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/infoview-panel-space.tsx",
	"InfoViewPanelSpace"
)

*/
export const InfoviewPanelLabel: any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/labels/labels.tsx",
	"InfoviewPanelLabel"
)
/*
const InfoviewMenu: any = getModule(
	"game-ui/game/components/infoviews/infoview-menu.module.scss",
	"InfoviewMenu"
)
*/
export const InfoviewPanelSection: any = getModule(
	"game-ui/game/components/infoviews/active-infoview-panel/components/sections/infoview-panel-section.tsx",
	"InfoviewPanelSection"
)

function handleClick(eventName: string) {
	// This triggers an event on C# side and C# designates the method to implement.
}

export const SelectedInfoPanelTogglesComponent = (componentList: any): any => {
	console.log("component list", componentList);
	//return <div>"test again here</div>
	componentList["Pandemic.HealthInfoUISystem"] = (e: any) => {
		//return <div>"test</div>
		console.log("the e", e);
		//return <InfoviewPanelSection>test</InfoviewPanelSection>
		console.log("the e2", e);
		const parts = [e.group, e.group]; //  e.group.split(",");
		return <InfoviewPanelSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoviewPanelSectionTheme.infoviewPanelSection}>
			<InfomodeItem
				left={parts[0]}
				right=
				{
					parts[1]
				}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfomodeItemTheme.infomodeItem}
			></InfomodeItem>
		</InfoviewPanelSection>
		// These get the value of the bindings.
		/*return <InfoviewPanelSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoviewPanelSectionTheme.infoviewPanelSection}>
			<InfomodeItem
				left={parts[0]}
				right=
				{
					parts[1]
				}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfomodeItemTheme.infomodeItem}
			></InfomodeItem>
		</InfoviewPanelSection>
			;*/
			return <div>test test test?</div>
	}

	return componentList as any;
}