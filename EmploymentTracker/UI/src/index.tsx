import { ModRegistrar } from "cs2/modding";
import HighlightOptionsMenuButton from "./HighlightOptionsMenuButton";
import RouteVolumeButton from "./RouteVolumeButton";
import RouteLaneInfoPanel from "./RouteLaneInfoPanel";

const register: ModRegistrar = (moduleRegistry) => {
	moduleRegistry.append('GameTopRight', HighlightOptionsMenuButton);
	moduleRegistry.append('GameTopRight', RouteVolumeButton);
}

export default register;