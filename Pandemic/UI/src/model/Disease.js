const TYPE_ABBRV = {
	1: "CC",
	2: "FL",
	3: "EX"
}

const TYPE_NAME = {
	1: "Common Cold",
	2: "Influenza",
	3: "Novel Virus"
}


export default class Disease {
	constructor(diseaseJson) {
		this._diseaseJson = diseaseJson;
		this._parentStrain = null;
		//console.log(this);
	}

	/** @returns {string} */
	get uniqueKey() {
		return this._diseaseJson.uniqueKey;
	}

	/** @returns {number}*/
	get totalInfectionCount() {
		return this._diseaseJson.infectionCount;
	}

	/** @returns {number}*/
	get baseHealthPenalty() {
		return this._diseaseJson.baseHealthPenalty;
	}

	/** @returns {number}*/
	get baseSpreadChance() {
		return (Math.round(this._diseaseJson.baseSpreadChance * 1000) / 1000).toFixed(3);
	}

	/** @returns {number}*/
	get baseSpreadRadius() {
		return (Math.round(this._diseaseJson.baseSpreadRadius * 1000) / 1000).toFixed(3);
	}

	/** @returns {number}*/
	get mutationChance() {
		return (Math.round(this._diseaseJson.mutationChance * 1000) / 1000).toFixed(3);
	}

	/** @returns {number}*/
	get mutationMagnitude() {
		return (Math.round(this._diseaseJson.mutationMagnitude * 1000) / 1000).toFixed(3);
	}

	/** @returns {number}*/
	get baseDeathChance() {
		return (Math.round(this._diseaseJson.baseDeathChance * 1000) / 1000).toFixed(3);
	}

	/** @returns {number}*/
	get progressionSpeed() {
		return (Math.round(this._diseaseJson.progressionSpeed * 1000 * 100) / 1000).toFixed(3);
	}

	/** @returns {number}*/
	get type() {
		return this._diseaseJson.type;
	}

	/** @returns {string} */
	get createTimeString() {
		return this._diseaseJson.createYear + "." + (this._diseaseJson.createWeek) + "." + (this._diseaseJson.createHour) + "." + (this._diseaseJson.createMinute);
	}

	/** @returns {string} */
	get typeString() {
		return TYPE_NAME[this.type]
	}

	/** @returns {string} */
	get strainName() {
		return TYPE_ABBRV[this.type] + this.createTimeString;
	}

	/** @returns {string} */
	get strainHeader() {
		return this.typeString + " " + this.strainName;
	}

	/** @returns {string|undefined} */
	get parent() {
		return this._diseaseJson.parent;
	}

	/** @returns {string|undefined} */
	get parentStrain() {
		return this._parentStrain;
	}

	set parentStrain(p) {
		this._parentStrain = p;
	}

	static diseaseStage(progression) {
		if (progression < .33) {
			return "Early";
		} else if (progression < .66) {
			return "Moderate";
		} else {
			return "Late";
		}
	}
}
