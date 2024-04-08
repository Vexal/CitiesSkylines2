import { ModRegistrar } from "cs2/modding";
import HighlightOptionsMenuButton from "./HighlightOptionsMenuButton";
import { HelloWorldComponent } from "./mods/hello-world";

const register: ModRegistrar = (moduleRegistry) => {
	moduleRegistry.append('GameTopRight', HighlightOptionsMenuButton);
	moduleRegistry.append('Menu', HelloWorldComponent);
}

export default register;