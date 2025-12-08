import { bindValue } from "cs2/api";

export const MOD_NAME = "Pandemic";

export const CustomBindings = {
	currentInfectionCount: bindValue<string[]>(MOD_NAME, 'currentInfectionCount'),
	diseaseNames: bindValue<Map<string, string>>(MOD_NAME, "diseaseNames"),
	diseaseBaseNames: bindValue<Map<string, string>>(MOD_NAME, "diseaseBaseNames"),
	diseaseList: bindValue<any[]>(MOD_NAME, 'diseases'),
	showCitizenHealth: bindValue<boolean>(MOD_NAME, "showCitizenHealth"),
	showActiveDiseaseDetails: bindValue<boolean>(MOD_NAME, 'showDetails_activeDiseases'),
	toggleShowDetails: (name: string) => "toggleShowDetails_" + name,
}

export default CustomBindings;