import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";
import { DiseaseInfoPanel } from "./infoview/HealthcareInfoview";
import { extendPanel } from "./mods/PanelExtension";
import { SelectedCitizenHealthView } from "./infoview/SelectedCitizenHealthView";


const register: ModRegistrar = (moduleRegistry) => {
	VanillaComponentResolver.setRegistry(moduleRegistry);
	//moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/active-infoview-panel.tsx", 'activeInfoviewPanel', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/components/sections/infoview-panel-section.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.extend("game-ui/game/components/infoviews/infoview-menu.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
	//moduleRegistry.override("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'HealthcareInfoviewPanel', SelectedInfoPanelTogglesComponent);
	
	//moduleRegistry.append("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'HealthcareInfoviewPanel', SelectedInfoPanelTogglesComponent);
	moduleRegistry.extend("game-ui/game/components/infoviews/active-infoview-panel/panels/healthcare-infoview-panel.tsx", 'HealthcareInfoviewPanel', extendPanel(<DiseaseInfoPanel />));
	moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', SelectedCitizenHealthView);

	console.log("Pandemic UI module registrations completed.");
}

export default register;