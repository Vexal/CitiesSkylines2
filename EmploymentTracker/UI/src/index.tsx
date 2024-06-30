import { ModRegistrar } from "cs2/modding";
import HighlightOptionsMenuButton from "./HighlightOptionsMenuButton";
import RouteVolumeButton from "./RouteVolumeButton";
import RouteLaneInfoPanel from "./RouteLaneInfoPanel";
import { SelectedInfoPanelTogglesComponent } from "./SelectedInfoPanelTogglesComponent";
import { VanillaComponentResolver } from "./VanillaComponentResolver";

const register: ModRegistrar = (moduleRegistry) => {
	VanillaComponentResolver.setRegistry(moduleRegistry);

	moduleRegistry.append('GameTopRight', HighlightOptionsMenuButton);
	moduleRegistry.append('GameTopRight', RouteVolumeButton);
	moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', SelectedInfoPanelTogglesComponent);

	console.log("Route highlighter UI module registrations completed.");
}

export default register;