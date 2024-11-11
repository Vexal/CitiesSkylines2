import { ModRegistrar } from "cs2/modding";
import ParkingMonitorButton from "./ParkingMonitorButton";

const register: ModRegistrar = (moduleRegistry) => {

	moduleRegistry.append('GameTopLeft', ParkingMonitorButton);
}

export default register;