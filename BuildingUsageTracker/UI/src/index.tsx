import { ModRegistrar } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world";
import { SelectedInfoPanelTogglesComponent } from "./SelectedInfoPanelTogglesComponent";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";
import { EntityListPaneCim, EntityListPaneVehicle } from "./EntityListPane";

const register: ModRegistrar = (moduleRegistry) => {
	console.log("the test");
	VanillaComponentResolver.setRegistry(moduleRegistry);
	moduleRegistry.append('GameTopLeft', EntityListPaneCim);
	moduleRegistry.append('GameTopLeft', EntityListPaneVehicle);

    moduleRegistry.append('Menu', HelloWorldComponent);
	moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);
}

export default register;