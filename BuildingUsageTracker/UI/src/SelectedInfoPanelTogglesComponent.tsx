import { getModule } from "cs2/modding";
import { Component } from "react";
import { Theme, FocusKey, UniqueFocusKey } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";
import CustomBindings, { MOD_NAME } from "./CustomBindings";
import styles from "./buildingusagetracker.module.scss"


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
	const getInfoRow = (data: any, field: string, text: string, small: boolean=false): any => {
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
				className={InfoRowTheme.infoRow + (small ? (" " + styles.smallInfoRow) : "")}
			></InfoRow>
		} else {
			return null;
		}
    }

	componentList["BuildingUsageTracker.SelectedBuildingOccupancyView"] = (e: InfoSectionComponent) => {
        const data = JSON.parse(e.group);
        console.log("occupancy data", data);

        const infs = <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
            <SectionContainer data={data}
                expandDetails={CustomBindings.expandEnrouteView.value}
                triggerDetailsName={CustomBindings.toggleShowDetails("occupancyView")}
                sectionName="Current Occupants"
                showEntityList={false}
                infoRows={[
                    getInfoRow(data, "workers", "Workers"),
                    getInfoRow(data, "patients", "Patients"),
                    getInfoRow(data, "students", "Students"),
                    getInfoRow(data, "sleepers", "Sleeping"),
                    getInfoRow(data, "other", "Other")
                ]}
            />
		</InfoSection>
			;
		return infs;
	}
	componentList["BuildingUsageTracker.SelectedBuildingEnRouteView"] = (e: InfoSectionComponent) => {
        const data = JSON.parse(e.group);
		console.log("enroute cims", data);
        return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
            <SectionContainer data={data}
                expandDetails={CustomBindings.expandEnrouteView.value}
                triggerDetailsName={CustomBindings.toggleShowDetails("enrouteView")}
                sectionName="En-route Cims"
                triggerEntitiesName="toggleShowEnrouteEntityList"
                showEntityList={true}
                infoRows={[
                getInfoRow(data, "passingThroughCount", "(Passing Through Station / Parking)", true)
                ,getInfoRow(data, "inVehicleCount", "(In Vehicles)", true)
                ,getInfoRow(data, "inPublicTransportCount", "(In Public Transport)", true)
                ,getInfoRow(data, "waitingTransportCount", "(Awaiting Public Transport)", true)
                ,getInfoRow(data, "workerCount", "Going to Work")
                ,getInfoRow(data, "healthcareCount", "Seeking Healthcare")
                ,getInfoRow(data, "studentCount", "Going to School")
                ,getInfoRow(data, "goingHomeCount", "Going Home")
                ,getInfoRow(data, "movingInCount", "Moving In")
                ,getInfoRow(data, "shoppingCount", "Shopping")
                ,getInfoRow(data, "liesureCount", "Liesure")
                ,getInfoRow(data, "touristCount", "Toruists")
                ,getInfoRow(data, "movingAwayCount", "Moving Away")
                ,getInfoRow(data, "other", "Other")
                ]}
            />
        </InfoSection>
			;
	}
	componentList["BuildingUsageTracker.SelectedBuildingVehicleEnRouteView"] = (e: InfoSectionComponent) => {
		const data = JSON.parse(e.group);
		console.log("the data", data);
        return <InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
            <SectionContainer data={data}
                expandDetails={CustomBindings.expandEnrouteView.value}
                triggerDetailsName={CustomBindings.toggleShowDetails("enrouteVehicleView")}
                sectionName="En-route Vehicles"
                triggerEntitiesName="toggleShowEnrouteVehicleEntityList"
                showEntityList={true}
                infoRows={[
                    getInfoRow(data, "personalCarCount", "Personal Vehicles"),
                    getInfoRow(data, "serviceCount", "Service Vehicles"),
                    getInfoRow(data, "deliveryCount", "Delilvery Vehicles"),
                    getInfoRow(data, "taxiCount", "Taxis"),
                    getInfoRow(data, "otherCount", "Other")
                ]}
            />
		</InfoSection>
			;
	}

	return componentList as any;
}

class SectionContainer extends Component<{ data: any, infoRows: any[], expandDetails: boolean, sectionName: string, triggerEntitiesName?: string, triggerDetailsName: string, showEntityList?: boolean }, { expandDetails: boolean, isShowingEntities: boolean }> {
    state = {
        expandDetails: this.props.expandDetails,
        isShowingEntities: this.props.data.entities !== null && this.props.data.entities !== undefined
    }

    render() {
        const data = this.props.data;
        const sectionName = this.state.expandDetails ? `▼ ${this.props.sectionName}` : `► ${this.props.sectionName}`;
        return (
            <>
                <InfoRow
                    left={
                        <div className={styles.mainRowHeader} onClick={() => this.toggleExpand()}>
                            {sectionName}
                        </div>}
                    right={<span>{this.props.showEntityList && <button className={styles.showEntitiesButton} onClick={() => {
                        const shouldShowEntities = !this.state.isShowingEntities;
                        this.setState({ isShowingEntities: shouldShowEntities }, () => {
                            trigger(MOD_NAME, this.props.triggerEntitiesName ?? "", shouldShowEntities);
                        });
                    }}>{this.state.isShowingEntities ? "Hide Entities " : "Show Entities "}</button>} {data["totalCount"]}</span>}
                    uppercase={true}
                    disableFocus={true}
                    subRow={false}
                    className={InfoRowTheme.infoRow}
                />
                {this.state.expandDetails && <>
                    {this.props.infoRows}
                </>}
            </>
        );
    }

    toggleExpand = () => {
        const shouldExpand = !this.state.expandDetails;
        this.setState({ expandDetails: shouldExpand }, () => {
            trigger(MOD_NAME, this.props.triggerDetailsName, shouldExpand);
        })
    }
}
    