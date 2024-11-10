import { bindValue } from "cs2/api";

const CustomBindings = {
	showToolIconsInUI: bindValue<boolean>("EmploymentTracker", 'showToolIconsInUI')
}

export default CustomBindings;