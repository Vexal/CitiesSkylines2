import { bindValue } from "cs2/api";

export const MOD_NAME = "Pandemic";

const CustomBindings = {
	currentInfectionCount: bindValue<string[]>(MOD_NAME, 'currentInfectionCount'),
	diseaseNames: bindValue<Map<string, string>>(MOD_NAME, "diseaseNames"),
	diseaseList: bindValue<any[]>(MOD_NAME, 'diseases'),
	showCitizenHealth: bindValue<boolean>(MOD_NAME, "showCitizenHealth")
}

export default CustomBindings;