import { ModRegistrar } from "cs2/modding";
import CimCensusButton from "./CimCensusButton";

const register: ModRegistrar = (moduleRegistry) => {

	moduleRegistry.append('GameTopLeft', CimCensusButton);
	console.log("loaded cim census");
}

export default register;