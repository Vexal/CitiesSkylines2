import CustomBindings from "./CustomBindings";

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
		return (Math.round(this._diseaseJson.progressionSpeed * 1000) / 1000).toFixed(3);
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
		const n = this.customName;
		return (n !== undefined && n !== null ? n : this.typeString) + " " + this.strainName;
	}

	/** @returns {string|undefined}*/
	get customName() {
		return CustomBindings.diseaseNames.value[this.uniqueKey];
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

	/**
	 * @param diseaseList {*[]}
	 * @return {{diseaseList: Disease[], diseaseMap: object.<string, Disease>}}
	 */
	static buildDiseaseList(diseaseList) {
		/** @type {Disease[]} */
		const l = diseaseList.map(d => new Disease(d._diseaseJson ? d._diseaseJson : d));
		const m = {};
		for (let i = 0; i < l.length; ++i) {
			m[l[i].uniqueKey] = l[i];
		}
		for (let i = 0; i < l.length; ++i) {
			if (l[i].parent) {
				l[i].parentStrain = m[l[i].parent]?.strainName;
			}
		}

		return { diseaseList: l, diseaseMap: m };
	}

	static patientCountMap = (countList) => {
		const results = {};
		for (let i = 0; i < countList.length; ++i) {
			const c = countList[i].split("_");
			results[c[0]] = c[1]
		}

		return results;
	}
}
