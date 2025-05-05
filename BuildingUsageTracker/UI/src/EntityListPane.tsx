// @ts-nocheck
import { Component } from "react";
import CustomBindings, { MOD_NAME, getArray } from "./CustomBindings"
import { trigger } from "cs2/api";

import styles from "./buildingusagetracker.module.scss"

export class EntityListPaneCim extends Component {
	render() {
		return  <EntityListPane binding={CustomBindings.enrouteCount} />
	}
}

export class EntityListPaneVehicle extends Component {
	render() {
		return <EntityListPane binding={CustomBindings.enrouteVehicleCount} />
	}
}

export default class EntityListPane extends Component {
	state = {
		entities: getArray(this.props.binding, "entities")
	}

	render() {
		if (!this.state.entities) {
			return null;
		}

		return <div className={styles.entityPanel}>
			<div className={styles.entityPanelHeader}>
				Enroute Entities
			</div>

			<div className={styles.entityPaneList }>
				{this.state.entities.map(entity => <div onClick={() => trigger(MOD_NAME, "selectEnrouteEntity", entity) }>
					{entity }
				</div>)}
			</div>
		</div>
	}

	componentDidMount() {
		this.props.binding.subscribe(val => {
			this.setState({ entities: getArray(this.props.binding, "entities") });
		})
	}
}

/*EntityListPane.propTypes = {
	binding: PropTypes.any
}*/