using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Creatures;
using Game.Net;
using Game.Objects;
using Game.Routes;
using Game.Tools;
using Game.UI;
using Colossal.Localization;
using Game.UI.InGame;
using System;
using Unity.Collections;
using Unity.Entities;

namespace BuildingUsageTracker
{
	internal abstract partial class SelectedBuildingInfoSection : InfoSectionBase
	{
		protected UIUpdateState uf;
		private Entity previousSelectedEntity = Entity.Null;
		protected ToolSystem toolSystem;
		protected NameSystem nameSystem;
		protected static string MOD_NAME = "BuildingUsageTracker";
		protected SelectedBuildingInfoSection otherView;
		protected bool showEntities = false;
        protected ValueBinding<bool> showDetails;
		protected ValueBinding<string> entityNames;
        protected TriggerBinding<bool> toggleShowDetails;
        protected string sectionName;

		protected void OnCreate(string sectionName, bool expandDetails)
		{
			base.OnCreate();
            this.sectionName = sectionName;
            this.uf = UIUpdateState.Create(World, 60);
			this.toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			this.nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
			m_InfoUISystem.AddMiddleSection(this);
            this.showDetails = new ValueBinding<bool>(MOD_NAME, "showDetails_" + sectionName, expandDetails);
            AddBinding(this.showDetails);
            this.toggleShowDetails = new TriggerBinding<bool>(MOD_NAME, "toggleShowDetails_" + sectionName, s => { this.showDetails.Update(s); this.updateExpandDetailsSetting(s); });
            AddBinding(this.toggleShowDetails);
			this.entityNames = new ValueBinding<string>(MOD_NAME, "entityNames_" + sectionName, "{}");
			AddBinding(this.entityNames);
        }

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (!Enabled)
			{
				this.Enabled = true;
			}

			Entity selectedEntity = this.toolSystem.selected;
			if (selectedEntity != this.previousSelectedEntity)
			{
				this.previousSelectedEntity = selectedEntity;
				this.uf.ForceUpdate();
				this.selectionChanged();
				if (!this.shouldBeVisible(selectedEntity))
				{
					this.visible = false;
					return;
				}
			}

			if (this.uf.Advance())
			{
				if (!this.shouldBeVisible(selectedEntity))
				{
					this.visible = false;
					return;
				}

				this.update(selectedEntity);
				this.visible = true;
			}
		}

		protected void addSubObjectsConnectedRoutes(ref NativeHashSet<Entity> results, Entity entity)
		{
			if (EntityManager.TryGetBuffer<SubObject>(entity, true, out var subObjects))
			{
				for (int i = 0; i < subObjects.Length; i++)
				{
					this.addConnectedRoutes(ref results, subObjects[i].m_SubObject);
					this.addSubObjectsConnectedRoutes(ref results, subObjects[i].m_SubObject);
				}
			}
			if (EntityManager.TryGetBuffer<SubLane>(entity, true, out var subLanes))
			{
				for (int i = 0; i < subLanes.Length; i++)
				{
					this.addParkingSpots(ref results, subLanes[i].m_SubLane);
				}
			}
		}

		protected void addConnectedRoutes(ref NativeHashSet<Entity> results, Entity entity)
		{
			if (EntityManager.TryGetBuffer<ConnectedRoute>(entity, true, out var routes))
			{
				for (int j = 0; j < routes.Length; ++j)
				{
					if (EntityManager.Exists(routes[j].m_Waypoint))
					{
						results.Add(routes[j].m_Waypoint);
					}
				}
			}
		}

		protected void addParkingSpots(ref NativeHashSet<Entity> results, Entity entity)
		{
			if (EntityManager.TryGetComponent<ParkingLane>(entity, out var parkingLane))
			{
				results.Add(entity);
			}
		}

		protected void toggleEntities(bool toggle)
		{
			this.showEntities = toggle;
			if (toggle)
			{
				this.otherView.toggleEntities(false);
			}

			this.uf.ForceUpdate();
		}

		protected virtual bool shouldBeVisible(Entity selectedEntity)
		{
			if (!EntityManager.Exists(selectedEntity) || !EntityManager.isBuilding(selectedEntity))
			{
				return false;
			}

			return true;
		}

		protected void populateEntityNames(ref NativeList<Entity> entities)
		{
			if (entities.IsEmpty)
			{
				return;
			}

			string json = "{";
			bool needsComma = false;
			LocalizationDictionary dict = null;
			try
			{
				dict = Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
			}
			catch
			{
				dict = null;
			}

			if (dict == null)
			{
				// no localization dictionary available, nothing to render
				return;
			}
			foreach (var entity in entities)
			{
				var name = this.nameSystem.GetName(entity);
				string nameJson = "null";
				string display = "";
				try
				{
					var writer = new BasicJsonWriter();
					if (name is IJsonWritable jw)
					{
						jw.Write(writer);
						nameJson = writer.Result ?? "null";

						// derive human-readable string from writer fields
						if (!string.IsNullOrEmpty(writer.TypeName) && writer.TypeName.Contains("CustomName"))
						{
							display = writer.CustomName ?? string.Empty;
						}
						else if (!string.IsNullOrEmpty(writer.TypeName) && writer.TypeName.Contains("LocalizedName"))
						{
							var id = writer.NameId;
							if (id != null && dict.TryGetValue(id, out var val)) display = val;
							else display = id ?? string.Empty;
						}
						else if (!string.IsNullOrEmpty(writer.TypeName) && writer.TypeName.Contains("FormattedName"))
						{
							var id = writer.NameId;
							string template = null;
							if (id != null && dict.TryGetValue(id, out var val)) template = val;

							if (template == null) template = id ?? string.Empty;
							foreach (var kv in writer.NameArgs)
							{
								string valText = kv.Value;
								if (kv.Value != null && dict.TryGetValue(kv.Value, out var v2)) valText = v2;

								template = template.Replace("{" + kv.Key + "}", valText).Replace(kv.Key, valText);
							}

							display = template;
						}
						else
						{
							display = writer.Result ?? string.Empty;
						}
					}
					else
					{
						nameJson = '"' + name.ToString().Replace("\"", "\\\"") + '"';
						display = name.ToString() ?? string.Empty;
					}
				}
				catch (Exception e)
				{
					nameJson = "null";
					display = "";
					Mod.log.Info("exception: " + e.ToString());
				}

				string displayEsc = display.Replace("\"", "\\\"");
				string combined = "{" + "\"display\":\"" + displayEsc + "\"}";
				json += (needsComma ? "," : "") + Utils.jsonEntity(entity) + ":" + combined;
				needsComma = true;
			}

			json += "}";
			this.entityNames.Update(json);
		}

		protected abstract void selectionChanged();

		protected abstract void update(Entity selectedEntity);

		public override GameMode gameMode => GameMode.Game;

        protected abstract void updateExpandDetailsSetting(bool expanded);


        public override void OnWriteProperties(IJsonWriter writer)
		{
			
		}

		protected override void OnProcess()
		{
			
		}

		protected override void Reset()
		{
			
		}
	}
}
