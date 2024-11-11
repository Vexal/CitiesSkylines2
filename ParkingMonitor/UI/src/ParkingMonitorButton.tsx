import { Button, FloatingButton, Icon, Panel, PanelSection, PanelSectionRow } from "cs2/ui";
import { bindValue, trigger } from "cs2/api";
import { Component } from "react";
import { useLocalization } from "cs2/l10n";
import styles from "./parkingmonitorbutton.module.scss"


/** @type {string} */
export const MOD_NAME = "ParkingMonitor";

export const dataBindings = bindValue<string>(MOD_NAME, 'dataBindings');
export const parkingBindings = bindValue<string>(MOD_NAME, 'parkingBindings');
export const parkingType = bindValue<string>(MOD_NAME, 'parkingType');
export const autoRefreshActiveBinding = bindValue<boolean>(MOD_NAME, 'autoRefreshActive');
export const enabledBinding = bindValue<boolean>(MOD_NAME, 'enabled');

export default class ParkingMonitorButton extends Component {
	state = {
		menuOpen: false,
		/** @type {{key: string, value: string|number}} */
		dataBindings: ParkingMonitorButton.parseBindings(dataBindings.value),
		parkingBindings: ParkingMonitorButton.parseParkingBindings(parkingBindings.value),
		hovering: false,
		parkingType: parkingType.value,
		enabledBinding: enabledBinding.value,
		autoRefreshActiveBinding: autoRefreshActiveBinding.value,
		districtMaxCount: {}

	}

	render() {
		//TODO figure out why Panel component is too laggy; temporarily use div with manual styling
		return <> 
			<div className={styles.parkingMonitorButton + " " + (this.state.menuOpen ? styles.parkingMonitorMenuOpen : "")} onClick={() => {
				const shouldOpen = !this.state.menuOpen;
				this.setState({ menuOpen:  shouldOpen});
			}} onMouseEnter={() => {
				this.setState({ hovering: true })
			}} onMouseLeave={() => {
				this.setState({ hovering: false })
				}}>{ic}</div>
			{this.state.hovering && <ToolTip text={"Parking Monitor"} />}

			{this.state.menuOpen && <div className={styles.cimCensusPanel}>
				<SectionHeader text="Parking Monitor" />
				<div className={styles.cimCensusPanelBody}>
					<PanelSection>
						<div style={{ display: "flex" }}>
							{ic}
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.parkingType === "failedParking"} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "setParkingType", "failedParking");
								}}>
									<div style={{ padding: "4rem" }}><LocalizedText text="Failed Parking"/></div>
								</Button>
							</div>
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.parkingType === "activeParking"} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "setParkingType", "activeParking");
								}}>
									<div style={{ padding: "4rem" }}><LocalizedText text="Active Parking"/></div>
								</Button>
							</div>
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.autoRefreshActiveBinding === true && this.state.enabledBinding === true} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "pause", true);
								}}>
									<div style={{ padding: "4rem" }}>{playIcon}</div>
								</Button>
							</div>
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.autoRefreshActiveBinding === false} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "pause", false);
								}}>
									<div style={{ padding: "4rem" }}>{pause}</div>
								</Button>
							</div>
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.enabledBinding === false} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "enable", false);
								}}>
									<div style={{ padding: "4rem" }}>{stop}</div>
								</Button>
							</div>
							<div style={{ flex: "1" }} />
							<div style={{marginRight:"10rem"} }>
								<Button selected={false} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "clear", true);
								}}>
									<div style={{ padding: "4rem" }}><LocalizedText text="Clear" /></div>
								</Button>
							</div>
						</div>
						<PanelSectionRow />
						<span className={styles.parkingMonitorSectionText}>
							<LocalizedText text={"Parking Monitor"} />
						</span>
						<div style={{maxHeight:"400rem", overflowY:"scroll"} }>
							{this.state.dataBindings.map(data => <DataItem key={data.key} name={data.key} text={data.value.toString()} cb={(k: String) => {
								
							}} />)}
						</div>
						<div style={{marginTop:"10rem"} }>
							<span className={styles.parkingMonitorSectionText }>
								<LocalizedText text={this.state.parkingType === "failedParking" ? "Failed Parking Attempts for Active Trips" : "Vehicles Planning to Park per Location"} />
							</span>
							<div className={styles.parkingMonitorLotList}>
								{this.state.parkingBindings?.districtOrder.map((district, i) => {
									const districtLots: any[] = this.state.parkingBindings?.districtMap[district] as any[];
									console.log("it's rendering now");
									return (
										<div key={district}>
											<div style={{ color:"rgba(55,195,241,1)"} }>
												<span>{district} {districtLots.length} lots</span>
											</div>
											<div style={{fontSize:"15rem"} }>
												{districtLots.slice(0, Math.min(10, districtLots.length)).map((lot, lotInd) => {
													return (
														<div style={{ display: "flex" }} className={styles.dataItem} onClick={() => {
															trigger(MOD_NAME, "selectLot", lot.key);
														}}>
															<span>
																<span style={{ width: "20rem" }}>{lotInd + 1}</span> <b>{lot.name}</b> {lot.key}
															</span>
															<div style={{ flex: "1" }} />
															<div>
																{lot.count}
															</div>
														</div>
													)
                                                    /*
													<span>
																<span style={{width:"20rem"} }>{lotInd + 1}</span> <b>{lot.name}</b> {lot.key}
															</span>
															<div style={{ flex: "1" }} />
															<div>
																{lot.count}
															</div>
													return (
                                                        <DataItem key={lotInd}
                                                            index={lotInd + 1}
                                                            rawText={lot.name}
                                                            name={lot.key}
                                                            text={lot.count.toString()}
                                                            cb={(k: String) => {
                                                                trigger(MOD_NAME, "selectLot", k);
                                                            } } />);*/
                                                })}
											</div>
										</div>
									)
                                })}
							</div>
						</div>
					</PanelSection>
				</div>
			</div>}
		</>
	}

	componentDidMount() {
		dataBindings.subscribe(val => {
			this.setState({ dataBindings: ParkingMonitorButton.parseBindings(val) });
		})
		parkingBindings.subscribe(val => {
			this.setState({ parkingBindings: ParkingMonitorButton.parseParkingBindings(val) });
		})
		parkingType.subscribe(val => {
			this.setState({ parkingType: parkingType.value });
		})
		enabledBinding.subscribe(val => {
			this.setState({ enabledBinding: enabledBinding.value });
		})
		autoRefreshActiveBinding.subscribe(val => {
			this.setState({ autoRefreshActiveBinding: autoRefreshActiveBinding.value });
		})
	}

	static parseBindings(stringList: string) {
		const results = stringList.split("|");

		return results.map(kv => kv.split(",")).map(kv => ({ key: kv[0], value: parseInt(kv[1]) })).sort((a,b) => (a.value > b.value ? -1 : (b.value > a.value ? 1 : 0)));
	}

	static parseParkingBindings(stringList: string) : ParkingDistrict|null {
		if (stringList.trim().length === 0) {
			return null;
;
		}

		const results = stringList.split("|");
		
		const lotArr = results.map(r => JSON.parse(r)).sort((a, b) => {
			return (a.count > b.count ? -1 : (b.count > a.count ? 1 : 0));
		});

		const no_district = "No District";

		/** @type {object<string, [*]}*/
		const districtMap: Record<string, any> = {};
		const districtOrder: string[] = [];

		for (let i = 0; i < lotArr.length; ++i) {
			const row = lotArr[i];
			let district: string = row.district ? row.district as string : (no_district ? no_district : "");
			if (!districtMap[district]) {
				districtMap[district] = [];
				districtOrder.push(district);
			}

			districtMap[district].push(row);
		}
		//console.log("bindings", districtMap);
		return { districtMap: districtMap, districtOrder: districtOrder};
	}
}
type ParkingDistrict = {
	districtMap: Record<string, any>,
	districtOrder: string[]
}

type DSProps = {
	dataCategory: string,
	children:any[]
}

/*const no_district = function () {
	const { translate } = useLocalization();
	return translate(MOD_NAME + "No District", "No District");
}();*/

class DataSection extends Component<DSProps, any> {
	render() {
		return <div>
			{this.props.children && this.props.children.map(subCategory => <div>
				{subCategory }
			</div>)}
		</div>
	}
}

export function DataItem(props: { name: string, rawText?: string | null, text: string | null, cb: Function | null , index?: number}) {
	return (
		<div style={{ display: "flex" }} className={styles.dataItem} onClick={() => { if (props.cb) props.cb(props.name) }}>
			{props.index && <div style={{width:"20rem"} }>
				{props.index}
			</div>}
			<div style={{ paddingLeft: "5rem" }}>
				{props.rawText ? <span><b>{props.rawText}</b> - {props.name}</span> : <LocalizedText text={props.name} />}
			</div>
			<div style={{ flex: "1" }} />
			<div style={{ paddingRight: "20rem" }}>
				{props.text}
			</div>
		</div>
	);
}

function SectionHeader(props: { text: string }) {
	const { translate } = useLocalization();
	return <div className={styles.cimCensusPanelHeader} style={{ fontWeight: "bold" }}>{translate(MOD_NAME + props.text, props.text)}</div>
}

export function LocalizedText(props: { text: string }) {
	const { translate } = useLocalization();
	return <>{translate(MOD_NAME + props.text, props.text)}</>
}

export function ToolTip(props: { text: string }) {
	const { translate } = useLocalization();
	return <div style={{ fontWeight: "bold", position: "absolute", marginTop: "43rem", maxWidth: "200rem", color: "white", background: "black" }}>{translate(MOD_NAME + props.text, props.text)}</div>
}

const ic = <svg xmlns="http://www.w3.org/2000/svg"  width="34" height="34" viewBox="0 0 34 34"><defs><clipPath id="b"><rect width="34" height="34" /></clipPath></defs><g id="a"><path d="M733.5-302.422l18.073,15.486,14.1-9.392v-1.62L733.5-304.027Z" transform="translate(-732.621 317.048)" fill="#434343" /><path d="M1223.5-1133.647l18.118,15.388,14.114-9.271-18.261-15.21Z" transform="translate(-1222.621 1146.628)" fill="gray" /><path d="M5.966-5.683H7.338a4.831,4.831,0,0,0,2.878-.706,2.4,2.4,0,0,0,.955-2.055,2.438,2.438,0,0,0-.8-2.011,3.956,3.956,0,0,0-2.508-.65h-1.9Zm9.415-2.9A5.4,5.4,0,0,1,13.4-4.071,8.939,8.939,0,0,1,7.755-2.51H5.966v6.5H1.8V-14.277H8.078a8.815,8.815,0,0,1,5.441,1.43A5.043,5.043,0,0,1,15.382-8.581Z" transform="matrix(0.67, -0.435, 0.714, 0.598, 15.317, 22.863)" fill="#e5e5e5" /></g></svg>

const playIcon = <svg fill="#FFFFFF" height="25rem" width="20rem" version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 60" ><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <path d="M45.563,29.174l-22-15c-0.307-0.208-0.703-0.231-1.031-0.058C22.205,14.289,22,14.629,22,15v30 c0,0.371,0.205,0.711,0.533,0.884C22.679,45.962,22.84,46,23,46c0.197,0,0.394-0.059,0.563-0.174l22-15 C45.836,30.64,46,30.331,46,30S45.836,29.36,45.563,29.174z M24,43.107V16.893L43.225,30L24,43.107z"></path> <path d="M30,0C13.458,0,0,13.458,0,30s13.458,30,30,30s30-13.458,30-30S46.542,0,30,0z M30,58C14.561,58,2,45.439,2,30 S14.561,2,30,2s28,12.561,28,28S45.439,58,30,58z"></path> </g> </g></svg>
const pause = <svg fill="#FFFFFF" height="25rem" width="20rem" version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M256,0C114.617,0,0,114.615,0,256s114.617,256,256,256s256-114.615,256-256S397.383,0,256,0z M224,320 c0,8.836-7.164,16-16,16h-32c-8.836,0-16-7.164-16-16V192c0-8.836,7.164-16,16-16h32c8.836,0,16,7.164,16,16V320z M352,320 c0,8.836-7.164,16-16,16h-32c-8.836,0-16-7.164-16-16V192c0-8.836,7.164-16,16-16h32c8.836,0,16,7.164,16,16V320z"></path> </g></svg>
const stop = <svg fill="#FFFFFF" height="25rem" width="20rem" version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg"  viewBox="0 0 297 297" ><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M148.5,0C66.486,0,0,66.486,0,148.5S66.486,297,148.5,297S297,230.514,297,148.5S230.514,0,148.5,0z M213.292,190.121 c0,12.912-10.467,23.379-23.378,23.379H106.67c-12.911,0-23.378-10.467-23.378-23.379v-83.242c0-12.912,10.467-23.379,23.378-23.379 h83.244c12.911,0,23.378,10.467,23.378,23.379V190.121z"></path> </g></svg>