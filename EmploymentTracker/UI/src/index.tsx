import { ModRegistrar } from "cs2/modding";
import HighlightOptionsMenuButton from "./HighlightOptionsMenuButton";
import RouteVolumeButton from "./RouteVolumeButton";

const register: ModRegistrar = (moduleRegistry) => {
	moduleRegistry.append('GameTopRight', HighlightOptionsMenuButton);
	moduleRegistry.append('GameTopRight', RouteVolumeButton);
}

export default register;