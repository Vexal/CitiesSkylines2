import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";

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