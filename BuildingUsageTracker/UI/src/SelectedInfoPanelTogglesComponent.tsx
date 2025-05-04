import { getModule } from "cs2/modding";
import { Theme, FocusKey, UniqueFocusKey } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";


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

export const MOD_NAME = "BuildingUsageTracker";

function handleClick(eventName: string) {
	// This triggers an event on C# side and C# designates the method to implement.
}

let showList = false;

export const SelectedInfoPanelTogglesComponent = (componentList: any): any => {
	// I believe you should not put anything here.
	componentList["BuildingUsageTracker.SelectedBuildingOccupancyView"] = (e: InfoSectionComponent) => {
		const data = JSON.parse(e.group);
		console.log("the parts", data);


		const getInfoRow = (field:string, text:string): any =>  {
			if (data[field]) {
				return <InfoRow
					left={text}
					right=
					{
						data[field]
					}
					uppercase={false}
					disableFocus={true}
					subRow={true}
					className={InfoRowTheme.infoRow}
				></InfoRow>
			} else {
				return undefined;
			}
		}

		const infs = <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={"Current Occupants"}
				right={data["occupantCount"]}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			{getInfoRow("workers", "Workers") }
			{getInfoRow("patients", "Patients") }
			{getInfoRow("students", "Students") }
			{getInfoRow("sleepers", "Sleeping") }
			{getInfoRow("other", "Other") }
		</InfoSection>
			;
		console.log(infs);
		return infs;
	}
	componentList["BuildingUsageTracker.SelectedBuildingEnRouteView"] = (e: InfoSectionComponent) => {
		console.log("the unparsed json", e.group);
		const data = JSON.parse(e.group);
		console.log("the parts", data);


		const getInfoRow = (field: string, text: string): any => {
			if (data[field]) {
				return <InfoRow
					left={text}
					right=
					{
						data[field]
					}
					uppercase={false}
					disableFocus={true}
					subRow={true}
					className={InfoRowTheme.infoRow}
				></InfoRow>
			} else {
				return null;
			}
		}

		/*
		public NativeCounter totalCount;
			public NativeCounter workerCount;
			public NativeCounter studentCount;
			public NativeCounter touristCount;
			public NativeCounter healthcareCount;
			public NativeCounter emergencyCount;
			public NativeCounter jailCount;
			public NativeCounter goingHomeCount;
			public NativeCounter otherCount;
			public NativeCounter shoppingCount;
			public NativeCounter liesureCount;
			public NativeCounter movingInCount;
		*/
		const entityList = (entities: string[]): any => {
			return <div>
				{entities.map(entity => <div key={entity}>
					{entity}
				</div>)}
			</div>
		}
		return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={false} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={<div>
					<div>En-route Cims</div>
					{showList && data.entities && entityList(data.entities) }
				</div>}
				right={<span><button style={{ color: "lightblue" }} onClick={() => {
					trigger("BuildingUsageTracker", "toggleShowEnrouteEntityList", true);

					showList = true;
					console.log("testing")
				}}>First</button> {data["totalCount"]}</span>}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			{getInfoRow("workerCount", "Going to Work")}
			{getInfoRow("healthcareCount", "Seeking Healthcare")}
			{getInfoRow("studentCount", "Going to School")}
			{getInfoRow("goingHomeCount", "Going Home")}
			{getInfoRow("movingInCount", "Moving In")}
			{getInfoRow("shoppingCount", "Shopping")}
			{getInfoRow("liesureCount", "Liesure")}
			{getInfoRow("other", "Other")}
			{getInfoRow("touristCount", "Toruists")}
		</InfoSection>
			;
	}

	return componentList as any;
}