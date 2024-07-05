using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Net;
using Game.Settings;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace EmploymentTracker
{
	partial class HighlightRoutesSystem : UISystemBase
	{
		private InputAction toggleSystemAction;
		private InputAction togglePathDisplayAction;
		private InputAction togglePathVolumeDisplayAction;
		//private InputAction toggleRenderTypeAction;

		private ValueBinding<bool> debugActiveBinding;
		private ValueBinding<bool> refreshTransitingEntitiesBinding;

		private ValueBinding<int> trackedEntityCount;
		private ValueBinding<int> uniqueSegmentCount;
		private ValueBinding<int> totalSegmentCount;
		private ValueBinding<string> routeTimeMs;
		private ValueBinding<string> selectionTypeBinding;

		private ValueBinding<bool> incomingRoutes;
		private ValueBinding<bool> incomingRoutesTransit;
		private ValueBinding<bool> highlightSelected;
		private ValueBinding<bool> highlightPassengerRoutes;
		private ValueBinding<bool> routeHighlightingToggled;
		private ValueBinding<bool> routeVolumeToolActive;
		private ValueBinding<bool> laneSelectionActive;
		private ValueBinding<string> laneIdListBinding;

		private Dictionary<string, string> bindings = new Dictionary<string, string>();
		private bool useNewRenderer = true;

		private void initBindings()
		{
			this.toggleSystemAction = new InputAction("shiftEmployment", InputActionType.Button);
			this.toggleSystemAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/e").With("Modifier", "<keyboard>/shift");
			this.togglePathDisplayAction = new InputAction("shiftPathing", InputActionType.Button);
			this.togglePathDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/v").With("Modifier", "<keyboard>/shift");
			this.togglePathVolumeDisplayAction = new InputAction("shiftPathingVolume", InputActionType.Button);
			this.togglePathVolumeDisplayAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/r").With("Modifier", "<keyboard>/shift");
			//this.toggleRenderTypeAction = new InputAction("renderType", InputActionType.Button);
			//this.toggleRenderTypeAction.AddCompositeBinding("OneModifier").With("Binding", "<keyboard>/x").With("Modifier", "<keyboard>/shift");

			//route toggles
			this.incomingRoutes = new ValueBinding<bool>("EmploymentTracker", "highlightEnroute", this.settings.incomingRoutes);
			this.highlightSelected = new ValueBinding<bool>("EmploymentTracker", "highlightSelectedRoute", this.settings.highlightSelected);
			this.incomingRoutesTransit = new ValueBinding<bool>("EmploymentTracker", "highlightEnrouteTransit", this.settings.incomingRoutesTransit);
			this.highlightPassengerRoutes = new ValueBinding<bool>("EmploymentTracker", "highlightPassengerRoutes", this.settings.highlightSelectedTransitVehiclePassengerRoutes);
			this.routeHighlightingToggled = new ValueBinding<bool>("EmploymentTracker", "routeHighlightingToggled", true);
			this.routeVolumeToolActive = new ValueBinding<bool>("EmploymentTracker", "routeVolumeToolActive", false);
			this.laneSelectionActive = new ValueBinding<bool>("EmploymentTracker", "laneSelectionActive", true);
			this.laneIdListBinding = new ValueBinding<string>("EmploymentTracker", "laneIdList", "");

			AddBinding(this.incomingRoutes);
			AddBinding(this.highlightSelected);
			AddBinding(this.incomingRoutesTransit);
			AddBinding(this.highlightPassengerRoutes);
			AddBinding(this.routeHighlightingToggled);
			AddBinding(this.routeVolumeToolActive);
			AddBinding(this.laneSelectionActive);
			AddBinding(this.laneIdListBinding);

			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightEnroute", s => { this.incomingRoutes.Update(s); this.settings.incomingRoutes = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightSelectedRoute", s => { this.highlightSelected.Update(s); this.settings.highlightSelected = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightEnrouteTransit", s => { this.incomingRoutesTransit.Update(s); this.settings.incomingRoutesTransit = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleHighlightPassengerRoutes", s => { this.highlightPassengerRoutes.Update(s); this.settings.highlightSelectedTransitVehiclePassengerRoutes = s; this.saveSettings(); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "quickToggleRouteHighlighting", s => { this.togglePathing(s); }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleLaneSelection", s => { this.laneSelectionActive.Update(s); this.resetActiveLanes(); this.laneSetDirty = true; }));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleRouteVolumeToolActive", s => { this.toggleRouteVolumeToolActive(s); }));
			AddBinding(new TriggerBinding<string>("EmploymentTracker", "toggleActiveLanes", s => { this.setActiveLanes(s); }));
			AddBinding(new TriggerBinding<int>("EmploymentTracker", "toggleHoverLane", s => {
				this.hoverLane = s;

				if (this.hoverLane >= 0 && EntityManager.TryGetBuffer<SubLane>(this.selectedEntity, true, out var laneBuffer) &&
				this.hoverLane < laneBuffer.Length)
				{
					int pathableIndex = -1;
					for (int i = 0; i < laneBuffer.Length; i++)
					{
						if (this.isLanePathable(laneBuffer[i]))
						{
							++pathableIndex;
						}

						if (pathableIndex == this.hoverLane)
						{
							if (EntityManager.TryGetComponent(laneBuffer[i].m_SubLane, out Curve curve))
							{
								this.hoverCurve = curve.m_Bezier;
							}

							break;
						}
					}
					
				}
			}));


			//options
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleAutoRefresh", this.toggleAutoRefresh));
			AddBinding(new TriggerBinding<bool>("EmploymentTracker", "toggleDebug", this.toggleDebug));

			this.debugActiveBinding = new ValueBinding<bool>("EmploymentTracker", "DebugActive", false);
			this.refreshTransitingEntitiesBinding = new ValueBinding<bool>("EmploymentTracker", "AutoRefreshTransitingEntitiesActive", true);

			AddBinding(this.debugActiveBinding);
			AddBinding(this.refreshTransitingEntitiesBinding);

			//stats
			this.trackedEntityCount = new ValueBinding<int>("EmploymentTracker", "TrackedEntityCount", 0);
			AddBinding(this.trackedEntityCount);
			this.uniqueSegmentCount = new ValueBinding<int>("EmploymentTracker", "UniqueSegmentCount", 0);
			AddBinding(this.uniqueSegmentCount);
			this.totalSegmentCount = new ValueBinding<int>("EmploymentTracker", "TotalSegmentCount", 0);
			AddBinding(this.totalSegmentCount);
			this.routeTimeMs = new ValueBinding<string>("EmploymentTracker", "RouteTimeMs", "");
			AddBinding(this.routeTimeMs);
			this.selectionTypeBinding = new ValueBinding<string>("EmploymentTracker", "selectionType", "");
			AddBinding(this.selectionTypeBinding);
		}

		private void enableBindings()
		{
			this.toggleSystemAction.Enable();
			this.togglePathDisplayAction.Enable();
			this.togglePathVolumeDisplayAction.Enable();
			//this.toggleRenderTypeAction.Enable();
		}

		private void disableBindings()
		{
			this.toggleSystemAction.Disable();
			this.togglePathDisplayAction.Disable();
			this.togglePathVolumeDisplayAction.Disable();
			//this.toggleRenderTypeAction.Disable();
		}

		private void updateBindings()
		{
			List<string> bindingList = new List<string>(this.bindings.Count);
			foreach (var b in this.bindings)
			{
				bindingList.Add(b.Key + "," + b.Value);
			}

			this.routeTimeMs.Update(string.Join(":", bindingList));
		}

		private void applySettings(Setting gameSettings)
		{
			if (gameSettings.GetType() == typeof(EmploymentTrackerSettings))
			{
				EmploymentTrackerSettings changedSettings = (EmploymentTrackerSettings)gameSettings;
				this.highlightFeatures = new HighlightFeatures(settings);
				this.routeHighlightOptions = new RouteOptions(settings);
				this.threadBatchSize = changedSettings.threadBatchSize;
			}
		}

		private bool checkFrameToggles()
		{
			if (this.toggleSystemAction.WasPressedThisFrame())
			{
				this.toggle(!this.toggled);
			}

			if (!this.toggled && !this.pathVolumeToggled)
			{
				return false;
			}

			if (this.togglePathDisplayAction.WasPressedThisFrame())
			{
				this.togglePathing(!this.pathingToggled);
			}

			if (!this.pathingToggled && !this.pathVolumeToggled)
			{
				return false;
			}

			if (this.selectedEntity == null || this.selectedEntity == default(Entity) || (this.selectionType == SelectionType.UNKNOWN && !this.pathVolumeToggled))
			{
				return false;
			}

			return true;
		}

		private void toggleAutoRefresh(bool active)
		{
			this.refreshTransitingEntitiesBinding.Update(active);
		}

		private void toggleDebug(bool active)
		{
			this.debugActiveBinding.Update(active);
		}

		private void saveSettings()
		{
			this.settings.ApplyAndSave();
		}

		public void toggle(bool active)
		{
			if (active != this.toggled)
			{
				this.reset();
				this.toggled = active;
			}
		}

		private void togglePathing(bool active)
		{
			if (active != this.pathingToggled)
			{
				this.reset();
				this.pathingToggled = active;
				if (this.routeHighlightingToggled.value != this.pathingToggled)
				{
					this.routeHighlightingToggled.Update(this.pathingToggled);
				}
			}
		}

		public void toggleRouteVolumeToolActive(bool active)
		{
			if (active)
			{
				this.defaultHasDebugSelect = this.defaultToolSystem.debugSelect;
				this.pathVolumeToggled = true;
				this.defaultToolSystem.debugSelect = true;
			}
			else
			{
				this.pathVolumeToggled = false;
				this.defaultToolSystem.debugSelect = this.defaultHasDebugSelect;
			}

			this.resetActiveLanes();

			this.routeVolumeToolActive.Update(active);
		}
	}
}
