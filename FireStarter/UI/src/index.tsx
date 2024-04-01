import { ModRegistrar } from "cs2/modding";
import { FloatingButton } from "cs2/ui";
import { HelloWorldComponent } from "mods/hello-world";
import TestButton from "./component";

const register: ModRegistrar = (moduleRegistry) => {

	

	moduleRegistry.append('Menu', HelloWorldComponent);
	moduleRegistry.append('GameTopLeft', TestButton);
}

export default register;