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

const InfoSectionTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.module.scss",
	"classes"
);

const InfoRowTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.module.scss",
	"classes"
)

const InfoSection: any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.tsx",
	"InfoSection"
)

const InfoRow: any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx",
	"InfoRow"
)

function handleClick(eventName: string) {
	// This triggers an event on C# side and C# designates the method to implement.
}

export const SelectedInfoPanelTogglesComponent = (componentList: any): any => {
	// I believe you should not put anything here.
	componentList["EmploymentTracker.HighlightRoutesSystem"] = (e: InfoSectionComponent) => {
		// These get the value of the bindings.
		return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={"test"}
				right=
				{
					"Hello"
				}
				tooltip={"ok?"}
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