using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

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

			this.settings = new EmploymentTrackerSettings(this);
			this.settings.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(this.settings));
			GameManager.instance.localizationManager.AddSource("zh-HANS", new LocaleSC(this.settings));

			AssetDatabase.global.LoadSettings(nameof(EmploymentTracker), this.settings, new EmploymentTrackerSettings(this));

			updateSystem.UpdateBefore<HighlightEmployeesSystem>(SystemUpdatePhase.MainLoop);
			updateSystem.UpdateBefore<SimpleOverlayRendererSystem>(SystemUpdatePhase.Rendering);
			updateSystem.UpdateBefore<HighlightRoutesSystem>(SystemUpdatePhase.Rendering);

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
