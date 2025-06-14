import { getModule } from "cs2/modding";
import { Theme, FocusKey, UniqueFocusKey } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";
import CustomBindings, { MOD_NAME } from "./CustomBindings";


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

let showList = false;

export const SelectedInfoPanelTogglesComponent = (componentList: any): any => {
	const getInfoRow = (data: any, field: string, text: string): any => {
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
	// I believe you should not put anything here.
	componentList["BuildingUsageTracker.SelectedBuildingOccupancyView"] = (e: InfoSectionComponent) => {
		const data = JSON.parse(e.group);

		const infs = <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={"Current Occupants"}
				right={data["occupantCount"]}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			{getInfoRow(data, "workers", "Workers") }
			{getInfoRow(data, "patients", "Patients") }
			{getInfoRow(data, "students", "Students") }
			{getInfoRow(data, "sleepers", "Sleeping") }
			{getInfoRow(data, "other", "Other") }
		</InfoSection>
			;
		console.log(infs);
		return infs;
	}
	componentList["BuildingUsageTracker.SelectedBuildingEnRouteView"] = (e: InfoSectionComponent) => {
		const data = JSON.parse(e.group);
		console.log("enroute cims", data);
		const isShowingEntities = data.entities !== null && data.entities !== undefined;

		return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={<div>
					<div>En-route Cims</div>
				</div>}
				right={<span><button style={{ color: "lightblue" }} onClick={() => {
					trigger(MOD_NAME, "toggleShowEnrouteEntityList", !isShowingEntities);

				}}>{isShowingEntities ? "Hide  " : "Show  "}</button> {data["totalCount"]}</span>}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			{getInfoRow(data, "passingThroughCount", "(Passing Through Station / Parking)")}
			{getInfoRow(data, "inVehicleCount", "(In Vehicles)")}
			{getInfoRow(data, "workerCount", "Going to Work")}
			{getInfoRow(data, "healthcareCount", "Seeking Healthcare")}
			{getInfoRow(data, "studentCount", "Going to School")}
			{getInfoRow(data, "goingHomeCount", "Going Home")}
			{getInfoRow(data, "movingInCount", "Moving In")}
			{getInfoRow(data, "shoppingCount", "Shopping")}
			{getInfoRow(data, "liesureCount", "Liesure")}
			{getInfoRow(data, "other", "Other")}
			{getInfoRow(data, "touristCount", "Toruists")}
		</InfoSection>
			;
	}
	componentList["BuildingUsageTracker.SelectedBuildingVehicleEnRouteView"] = (e: InfoSectionComponent) => {
		const data = JSON.parse(e.group);
		console.log("the data", data);

		const isShowingEntities = data.entities !== null && data.entities !== undefined;

		return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
			<InfoRow
				left={<div>
					<div>En-route Vehicles</div>
				</div>}
				right={<span><button style={{ color: "lightblue" }} onClick={() => {
					trigger(MOD_NAME, "toggleShowEnrouteVehicleEntityList", !isShowingEntities);

				}}>{isShowingEntities ? "Hide   " : "Show   "}</button> {data["totalCount"]}</span>}
				uppercase={true}
				disableFocus={true}
				subRow={false}
				className={InfoRowTheme.infoRow}
			></InfoRow>
			{getInfoRow(data, "personalCarCount", "Personal Vehicles")}
			{getInfoRow(data, "serviceCount", "Service Vehicles")}
			{getInfoRow(data, "deliveryCount", "Delilvery Vehicles")}
			{getInfoRow(data, "taxiCount", "Taxis")}
			{getInfoRow(data, "otherCount", "Other")}
		</InfoSection>
			;
	}

	return componentList as any;
}