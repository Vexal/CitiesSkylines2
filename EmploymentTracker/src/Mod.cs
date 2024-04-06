using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Unity.Entities;

namespace EmploymentTracker
{
	public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);
		private EmploymentTrackerSettings settings;
		public static Mod INSTANCE;

		public void OnLoad(UpdateSystem updateSystem)
        {
			INSTANCE = this;
            log.Info(nameof(OnLoad) + " employment test");
			updateSystem.UpdateBefore<HighlightEmployeesSystem>(SystemUpdatePhase.MainLoop);
			updateSystem.UpdateBefore<RenderRoutesSystem>(SystemUpdatePhase.Rendering);

			this.settings = new EmploymentTrackerSettings(this);
			this.settings.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(this.settings));

			AssetDatabase.global.LoadSettings(nameof(EmploymentTracker), this.settings, new EmploymentTrackerSettings(this));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
        }

		public EmploymentTrackerSettings getSettings()
		{
			return this.settings;
		}

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
			if (this.settings != null)
			{
				this.settings.UnregisterInOptionsUI();
				this.settings = null;
			}
		}
    }
}
