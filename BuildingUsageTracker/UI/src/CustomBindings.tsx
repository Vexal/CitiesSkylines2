import { ValueBinding, bindValue } from "cs2/api";

export const MOD_NAME = "BuildingUsageTracker";

const CustomBindings = {
	enrouteCount: bindValue<string>(MOD_NAME, 'enrouteCountBinding'),
	enrouteVehicleCount: bindValue<string>(MOD_NAME, 'enrouteVehicleCountBinding'),
}

export const getArray = (binding: ValueBinding<string>, name: string): string[] => {
	return JSON.parse(binding.value)[name];
}

export default CustomBindings;