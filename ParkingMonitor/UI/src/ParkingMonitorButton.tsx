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
export const defaultRowsPerDistrict = bindValue<number>(MOD_NAME, 'defaultRowsPerDistrict');
export const districtSortOrder = bindValue<string>(MOD_NAME, 'districtSortOrder');

export default class ParkingMonitorButton extends Component {
	state = {
		menuOpen: false,
		/** @type {{key: string, value: string|number}} */
		dataBindings: ParkingMonitorButton.parseBindings(dataBindings.value),
		parkingBindings: ParkingMonitorButton.parseParkingBindings(parkingBindings.value, districtSortOrder.value),
		hovering: false,
		parkingType: parkingType.value,
		enabledBinding: enabledBinding.value,
		autoRefreshActiveBinding: autoRefreshActiveBinding.value,
		defaultRowsPerDistrict: defaultRowsPerDistrict.value,
		districtSortOrder: districtSortOrder.value,
		hideDistricts: {}

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
				}}>
				{ic}
				</div>
			{this.state.hovering && <ToolTip text={"Parking Monitor"} />}

			{this.state.menuOpen && <div className={styles.parkingMonitorPanel}>
				<SectionHeader text="Parking Monitor" />
				<div className={styles.parkingMonitorPanelBody}>
					<PanelSection>
						<div style={{ display: "flex" }}>
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
									<div className={styles.activeButton}>{playIcon}</div>
								</Button>
							</div>
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.autoRefreshActiveBinding === false} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "pause", false);
								}}>
									<div className={styles.activeButton}>{pause}</div>
								</Button>
							</div>
							<div style={{ marginRight: "10rem" }}>
								<Button selected={this.state.enabledBinding === false} variant="flat" onSelect={() => {
									trigger(MOD_NAME, "enable", false);
								}}>
									<div className={styles.activeButton}>{stop}</div>
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
						<div>
							{this.state.dataBindings.map(data => <DataItem key={data.key} name={data.key} text={data.value.toString()} cb={(k: String) => {}} />)}
						</div>
						<div style={{marginTop:"10rem"} }>
							<span className={styles.parkingMonitorSectionText }>
								<LocalizedText text={this.state.parkingType === "failedParking" ? "Failed Parking Attempts for Active Trips" : "Vehicles Planning to Park per Location"} />
							</span>
							
							<div className={styles.parkingMonitorLotList}>
								<div style={{ display: "flex", width:"400rem" }}>
									<div style={{width:"20rem"} }>
									#
									</div>
									<div style={{width:"200rem"} }>
									Name
									</div>
									<div style={{width:"130rem"} }>
									Lot Count
									</div>
									<div style={{width:"50rem"} }>
									Total
									</div>
								</div>
								{this.state.parkingBindings?.districtOrder.map((district, i) => {
									const districtLots: any[] = this.state.parkingBindings?.districtMap[district] as any[];
									const hideDistrict: boolean | undefined = (this.state.hideDistricts as Record<string, boolean>)[district] as boolean | undefined;

									return (
										<div key={district}>
											<div className={styles.districtHeader} style={{ display: "flex", width: "400rem", paddingTop: "10rem" }} onClick={() => this.setState({ hideDistricts: { ...this.state.hideDistricts, [district]: !hideDistrict } })}>
												<div style={{ color: "rgba(55,195,241,1)", width:"290rem" }}>
													<span>{district}</span>
												</div>
												<div style={{width:"110rem"} }>
													{districtLots.length}
												</div>
												<div style={{width:"30rem"} }>
													{this.state.parkingBindings?.districtCounts[district]}
												</div>
											</div>

											{!hideDistrict && <div>
												{districtLots.slice(0, Math.min(this.state.defaultRowsPerDistrict, districtLots.length)).map((lot, lotInd) => (
													<div onClick={() => { trigger(MOD_NAME, "selectLot", lot.key) }}
														className={styles.lotItem}
														key={lot.key}
													>
														<span style={{ width: "20rem" }} className={styles.tbl}>
															{lotInd + 1}
														</span>
														<span style={{ width: "20rem", height: "20rem" }} className={styles.tbl}>
															{lot.type === 1 && ic}
															{lot.type === 2 && roadIcon}
															{lot.type === 3 && houseIcon}
															{lot.type === 4 && officeIcon}
															{lot.type === 5 && industryIcon}
															{lot.type === 6 && commercialIcon}
														</span>
														<span style={{ width: "250rem", fontWeight: "bold" }} className={styles.tbl}>
															{lot.name}
														</span>

														<span style={{ width: "20rem" }} className={styles.tbl}>
															{magnifyingGlass}
														</span>
														<span style={{ width: "90rem", fontSize: "15rem" }} className={styles.tbl}>
															{lot.key}
														</span>
														<span style={{ width: "20rem", paddingRight: "10rem" }} className={styles.tbl}>
															{lot.count}
														</span>
													</div>))}
											</div>}
										</div>)
									
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
			this.setState({ parkingBindings: ParkingMonitorButton.parseParkingBindings(val, this.state.districtSortOrder) });
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
		defaultRowsPerDistrict.subscribe(val => {
			this.setState({ defaultRowsPerDistrict: defaultRowsPerDistrict.value });
		})
		districtSortOrder.subscribe(val => {
			this.setState({ districtSortOrder: districtSortOrder.value });
		})
	}

	static parseBindings(stringList: string) {
		const results = stringList.split("|");

		return results.map(kv => kv.split(",")).map(kv => ({ key: kv[0], value: parseInt(kv[1]) })).sort((a,b) => (a.value > b.value ? -1 : (b.value > a.value ? 1 : 0)));
	}

	static parseParkingBindings(stringList: string, districtSortOrder: string) : ParkingDistrict|null {
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
		const districtCounts: Record<string, number> = {};
		const districtOrder: string[] = [];

		for (let i = 0; i < lotArr.length; ++i) {
			const row = lotArr[i];
			let district: string = row.district ? row.district as string : (no_district ? no_district : "");
			if (!districtMap[district]) {
				districtMap[district] = [];
				districtCounts[district] = 0;
				districtOrder.push(district);
			}

			districtCounts[district] += row.count;
			districtMap[district].push(row);
		}

		if (districtSortOrder === "NAME") {
			districtOrder.sort();
		}

		//console.log("bindings", districtMap);
		return { districtMap: districtMap, districtOrder: districtOrder, districtCounts: districtCounts};
	}
}
type ParkingDistrict = {
	districtMap: Record<string, any>,
	districtOrder: string[],
	districtCounts: Record<string, number>
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
	return <div className={styles.parkingMonitorPanelHeaderr} style={{ fontWeight: "bold" }}>{translate(MOD_NAME + props.text, props.text)}</div>
}

export function LocalizedText(props: { text: string }) {
	const { translate } = useLocalization();
	return <>{translate(MOD_NAME + props.text, props.text)}</>
}

export function ToolTip(props: { text: string }) {
	const { translate } = useLocalization();
	return <div style={{ fontWeight: "bold", position: "absolute", marginTop: "43rem", maxWidth: "200rem", color: "white", background: "black" }}>{translate(MOD_NAME + props.text, props.text)}</div>
}

const ic = <svg width="100%" height="100%"  viewBox="0 0 34 34"><defs><clipPath><rect width="40" height="40" /></clipPath></defs><g><path d="M733.5-302.422l18.073,15.486,14.1-9.392v-1.62L733.5-304.027Z" transform="translate(-732.621 317.048)" fill="#434343" /><path d="M1223.5-1133.647l18.118,15.388,14.114-9.271-18.261-15.21Z" transform="translate(-1222.621 1146.628)" fill="gray" /><path d="M5.966-5.683H7.338a4.831,4.831,0,0,0,2.878-.706,2.4,2.4,0,0,0,.955-2.055,2.438,2.438,0,0,0-.8-2.011,3.956,3.956,0,0,0-2.508-.65h-1.9Zm9.415-2.9A5.4,5.4,0,0,1,13.4-4.071,8.939,8.939,0,0,1,7.755-2.51H5.966v6.5H1.8V-14.277H8.078a8.815,8.815,0,0,1,5.441,1.43A5.043,5.043,0,0,1,15.382-8.581Z" transform="matrix(0.67, -0.435, 0.714, 0.598, 15.317, 22.863)" fill="#e5e5e5" /></g></svg>

const playIcon = <svg fill="#FFFFFF" height="100%" width="100%" version="1.1" viewBox="0 0 60 60" ><g stroke-width="0"></g><g stroke-linecap="round" stroke-linejoin="round"></g><g> <g> <path d="M45.563,29.174l-22-15c-0.307-0.208-0.703-0.231-1.031-0.058C22.205,14.289,22,14.629,22,15v30 c0,0.371,0.205,0.711,0.533,0.884C22.679,45.962,22.84,46,23,46c0.197,0,0.394-0.059,0.563-0.174l22-15 C45.836,30.64,46,30.331,46,30S45.836,29.36,45.563,29.174z M24,43.107V16.893L43.225,30L24,43.107z"></path> <path d="M30,0C13.458,0,0,13.458,0,30s13.458,30,30,30s30-13.458,30-30S46.542,0,30,0z M30,58C14.561,58,2,45.439,2,30 S14.561,2,30,2s28,12.561,28,28S45.439,58,30,58z"></path> </g> </g></svg>
const pause = <svg fill="#FFFFFF" height="100%" width="100%" version="1.1" viewBox="0 0 512 512"><g stroke-width="0"></g><g stroke-linecap="round" stroke-linejoin="round"></g><g> <path d="M256,0C114.617,0,0,114.615,0,256s114.617,256,256,256s256-114.615,256-256S397.383,0,256,0z M224,320 c0,8.836-7.164,16-16,16h-32c-8.836,0-16-7.164-16-16V192c0-8.836,7.164-16,16-16h32c8.836,0,16,7.164,16,16V320z M352,320 c0,8.836-7.164,16-16,16h-32c-8.836,0-16-7.164-16-16V192c0-8.836,7.164-16,16-16h32c8.836,0,16,7.164,16,16V320z"></path> </g></svg>
const stop = <svg fill="#FFFFFF" height="100%" width="100%" version="1.1" viewBox="0 0 297 297" ><g stroke-width="0"></g><g stroke-linecap="round" stroke-linejoin="round"></g><g> <path d="M148.5,0C66.486,0,0,66.486,0,148.5S66.486,297,148.5,297S297,230.514,297,148.5S230.514,0,148.5,0z M213.292,190.121 c0,12.912-10.467,23.379-23.378,23.379H106.67c-12.911,0-23.378-10.467-23.378-23.379v-83.242c0-12.912,10.467-23.379,23.378-23.379 h83.244c12.911,0,23.378,10.467,23.378,23.379V190.121z"></path> </g></svg>
const magnifyingGlass = <svg fill="#FFFFFF" height="50%" width="50%" version="1.1" viewBox="0 0 490.4 490.4"><g stroke-width="0"></g><g stroke-linecap="round" stroke-linejoin="round"></g><g> <g> <path d="M484.1,454.796l-110.5-110.6c29.8-36.3,47.6-82.8,47.6-133.4c0-116.3-94.3-210.6-210.6-210.6S0,94.496,0,210.796 s94.3,210.6,210.6,210.6c50.8,0,97.4-18,133.8-48l110.5,110.5c12.9,11.8,25,4.2,29.2,0C492.5,475.596,492.5,463.096,484.1,454.796z M41.1,210.796c0-93.6,75.9-169.5,169.5-169.5s169.6,75.9,169.6,169.5s-75.9,169.5-169.5,169.5S41.1,304.396,41.1,210.796z"></path> </g> </g></svg>
const roadIcon = <svg width="90%" height="90%" viewBox="0 0 32 32"><defs><clipPath><rect width="32" height="32" /></clipPath></defs><g><path d="M733.5-302.422l18.073,15.486,14.1-9.392v-1.62L733.5-304.027Z" transform="translate(-730 320.306)" fill="#434343" /><path d="M1223.5-1133.647l18.118,15.388,14.114-9.271-18.261-15.21Z" transform="translate(-1220 1149.886)" fill="gray" /><path d="M740.478-308.616l3.13,2.636,1.385-.915-3.12-2.605Z" transform="translate(-729.414 321.622)" fill="#d4d4d4" /><path d="M740.386-308.926l3.13,2.636,1.385-.915-3.12-2.605Z" transform="translate(-716.136 332.797)" fill="#d4d4d4" /><path d="M741.151-309.033l3.13,2.636,1.385-.915-3.12-2.605Z" transform="translate(-723.76 327.228)" fill="#d4d4d4" /></g></svg>
const houseIcon = <svg width="90%" height="90%" viewBox="0 0 32 32"><defs><clipPath><rect width="32" height="32" /></clipPath></defs><g><g transform="translate(1.488 6.767)"><path d="M-10008-10482.843l16.188,10.105,13.515-10.105-16.512-8.157Z" transform="translate(10008 10492.43)" fill="#023b0a" /></g><g transform="translate(0.07 5.378)"><path d="M-9991.4-10491.047l9.246,5.405v-12.851l-9.246-4.459Z" transform="translate(9999.999 10500.713)" fill="#00b41a" /><path d="M-9989.369-10489.7l9.393,4.865v-.825l-9.393-4.784Z" transform="translate(9997.979 10494.326)" fill="#00580d" /><path d="M-9989.369-10486.715l9.393,4.873v-3.817l-9.393-4.784Z" transform="translate(9997.979 10489.326)" fill="#00580d" /><path d="M-9989.369-10489.7l9.393,4.865v-.825l-9.393-4.784Z" transform="translate(9997.979 10495.716)" fill="#00580d" /><path d="M-9989.369-10489.7l9.393,5.007.067-.864-9.46-4.887Z" transform="translate(9997.979 10497.104)" fill="#00580d" /><path d="M-9977.839-10492.07l5.7-3.98.236-12.183-5.937,3.422Z" transform="translate(9995.686 10507.154)" fill="#004b12" /><path d="M-9991.34-10490.063l9.324,4.668,5.832-3.4-9.379-4.062Z" transform="translate(9999.961 10487.854)" fill="#02ff26" /><path d="M34.45,18.577l5.734-3.356-.077.828L34.45,19.42Z" transform="translate(-16.499 -9.941)" fill="#002612" /><path d="M34.45,18.577l5.734-3.356v3.356L34.45,22.112Z" transform="translate(-16.499 -14.941)" fill="#002612" /><path d="M34.45,18.577l5.734-3.356-.077.828L34.45,19.42Z" transform="translate(-16.499 -8.552)" fill="#002612" /><path d="M34.45,18.577l5.734-3.356-.077.828L34.45,19.42Z" transform="translate(-16.499 -7.163)" fill="#002612" /><path d="M34.45,18.577l5.734-3.356-.077.828L34.45,19.42Z" transform="translate(-16.499 -5.775)" fill="#002612" /><path d="M10.044,10.117V8.992L13.1,10.7l.1,1.23Z" transform="translate(1 1)" fill="#00580d" /><path d="M-9991.4-10501.528l1.584.736v-1.512l-1.584-.647Z" transform="translate(10002.998 10499.874)" fill="#00b41a" /><path d="M-9991.4-10501.528l1.584.736v-1.512l-1.584-.647Z" transform="translate(10006.998 10501.874)" fill="#00b41a" /><path d="M-9977.839-10506.218l.977-.551v-1.5l-.977.563Z" transform="translate(9991.022 10505.302)" fill="#004b12" /><path d="M-9977.839-10506.218l.977-.551v-1.5l-.977.563Z" transform="translate(9995.022 10507.302)" fill="#004b12" /><path d="M-9991.34-10492.512l1.594.643.969-.511-1.584-.568Z" transform="translate(10002.942 10489.439)" fill="#9dffab" /><path d="M-9991.34-10492.512l1.594.643.969-.511-1.584-.568Z" transform="translate(10006.942 10491.439)" fill="#9dffab" /></g></g></svg>
const officeIcon = <svg width="90%" height="90%" viewBox="0 0 32 32"><defs><clipPath><rect width="32" height="32" /></clipPath></defs><g><g transform="translate(0.795 5.408)"><path d="M-10008-10482.914l16.191,10,13.512-10-16.52-8.08Z" transform="translate(10008.002 10493.75)" fill="#660090" /><path d="M-9906.381-10484.037l8.623,4.932.007-11.116-8.751-4.479Z" transform="translate(9914.063 10495.162)" fill="#9a0ad6" /><path d="M-9888.85-10478.244l5.781-4.1v-12.535l-4.4,2.859-1.393,2.635Z" transform="translate(9905.157 10494.277)" fill="#7902ab" /><path d="M-9905.352-10489.791l9.012,4.674,4.421-2.923-8.732-4.233Z" transform="translate(9914.057 10487.453)" fill="#d078f5" /><path d="M-9750.371-10484.5l8.645,4.572v4.243l-8.645-4.645Z" transform="translate(9758.059 10489.511)" fill="#64028d" /><path d="M-9750.558-10483.292l8.831,4.556v3.052l-8.645-4.645Z" transform="translate(9758.059 10484.511)" fill="#64028d" /><path d="M-9732.8-10478.926l5.771-3.8v3.8l-5.771,4.074Z" transform="translate(9749.131 10488.658)" fill="#310146" /><path d="M-9732.8-10478.926l5.771-3.8v2.972l-5.771,3.865Z" transform="translate(9749.131 10484.658)" fill="#310146" /><path d="M7.546.5l1.178-2.82,9.055,4.653L16.294,4.94Z" fill="#c65af3" /></g></g></svg>
const industryIcon = <svg width="90%" height="90%" viewBox="0 0 32 32"><defs><linearGradient x1="0.5" x2="0.5" y2="1" gradientUnits="objectBoundingBox"><stop offset="0" stop-color="#8a6d00" /><stop offset="1" stop-color="#f1bf00" /></linearGradient><clipPath><rect width="32" height="32" /></clipPath></defs><g><g transform="translate(0.796 4.408)"><path d="M-10008-10482.843l16.188,10.105,13.515-10.105-16.512-8.157Z" transform="translate(10008 10493.885)" fill="#ffdc00" /><path d="M-9906.379-10481.035l8.623,4.981.145-4.981-8.771-4.782Z" transform="translate(9914.058 10492.352)" fill="#d6a900" /><path d="M-9888.85-10475.07l5.781-4.144v-4.669l-5.781,3.848Z" transform="translate(9905.154 10491.461)" fill="#a48700" /><path d="M-9906.28-10489l8.7,4.788,5.612-3.837-8.653-4.337Z" transform="translate(9914.016 10495.641)" fill="#fff69f" /><path d="M-9902.63-10487.994l5.228,2.607,3.594-2.426-4.818-2.471Z" transform="translate(9912.163 10494.567)" fill="#f29d00" /><path d="M1.23-14.7a1.76,1.76,0,0,1,.957.284c.117.054.272,7.749.272,7.968,0,.421-.55.764-1.23.764S0-6.028,0-6.451c0-.122.046-7.727.24-7.942A1.415,1.415,0,0,1,1.23-14.7Z" transform="translate(17.216 14.703)" fill="url(#a)" /><path d="M.722-.412c.407,0,.734.114.734.254S1.129.1.722.1-.017-.016-.017-.158.313-.412.722-.412Z" transform="translate(17.717 0.56)" fill="#504200" /></g></g></svg>
const commercialIcon = <svg width="90%" height="90%" viewBox="0 0 32 32"><defs><clipPath><rect width="32" height="32" /></clipPath></defs><g><g transform="translate(1.488 6.767)"><path d="M-10008-10482.843l16.188,10.105,13.515-10.105-16.512-8.157Z" transform="translate(10008 10492.43)" fill="#005f95" /></g><g transform="translate(-2 2.599)"><path d="M-9991.4-10488.85l11.725,7.117.133-10.134-11.857-6.12Z" transform="translate(9999.999 10501.578)" fill="#0089cf" /><path d="M-9989.369-10489.464l11.91,6.406v-1.086l-11.91-6.3Z" transform="translate(9997.982 10495.559)" fill="#03243e" /><path d="M-9989.369-10489.464l11.91,6.406v-1.086l-11.91-6.3Z" transform="translate(9997.982 10497.388)" fill="#03243e" /><path d="M-9989.369-10489.464l11.91,6.593.086-1.138-12-6.435Z" transform="translate(9997.982 10499.217)" fill="#03243e" /><path d="M-9988.707-10487.747l9.987,5.894v-2.545l-9.987-5.58Z" transform="translate(9997.982 10501.044)" fill="#03243e" /><path d="M-9977.839-10488.589l7.228-5.24.233-9.236-7.329,4.376Z" transform="translate(9998.165 10508.451)" fill="#0071a9" /><path d="M-9991.34-10489.177l11.824,6.146,7.4-4.48-11.895-5.35Z" transform="translate(9999.967 10492.854)" fill="#73e0fc" /><path d="M34.45,19.64l7.271-4.419-.1,1.09L34.45,20.749Z" transform="translate(-13.991 -8.267)" fill="#001209" /><path d="M34.45,19.64l7.271-4.419-.1,1.09L34.45,20.749Z" transform="translate(-13.991 -6.438)" fill="#001209" /><path d="M34.45,19.64l7.271-4.419-.1,1.09L34.45,20.749Z" transform="translate(-13.991 -4.609)" fill="#001209" /><path d="M35.1,19.2,41,15.593v2.288l-5.9,4.206Z" transform="translate(-13.991 -2.781)" fill="#001209" /><path d="M5.688,8.846l1.041-.583V3.825L.549,1.052-.4,1.493Z" transform="translate(11 2)" fill="#0071a9" /><path d="M-9991.4-10493.844l6.02,3.223.068-4.59-6.088-2.773Z" transform="translate(10001.999 10501.467)" fill="#73d0ff" /></g></g></svg>
