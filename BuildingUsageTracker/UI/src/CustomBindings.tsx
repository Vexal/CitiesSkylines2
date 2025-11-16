import { ValueBinding, bindValue } from "cs2/api";

export const MOD_NAME = "BuildingUsageTracker";

const CustomBindings = {
	enrouteCount: bindValue<string>(MOD_NAME, 'enrouteCountBinding'),
	enrouteVehicleCount: bindValue<string>(MOD_NAME, 'enrouteVehicleCountBinding'),
    expandEnrouteView: bindValue<boolean>(MOD_NAME, 'showDetails_enrouteView'),
    expandVehicleEnrouteView: bindValue<boolean>(MOD_NAME, 'showDetails_enrouteVehicleView'),
    expandOccupantView: bindValue<boolean>(MOD_NAME, 'showDetails_occupancyView'),
    toggleShowDetails: (name:string) => "toggleShowDetails_" + name
}

export const getArray = (binding: ValueBinding<string>, name: string): string[] => {
	return JSON.parse(binding.value)[name];
}

export default CustomBindings;