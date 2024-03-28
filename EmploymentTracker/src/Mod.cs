using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Unity.Entities;

namespace EmploymentTracker
{
	public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);
		public static EmploymentTrackerSettings Settings { get; set; }

		public void OnLoad(UpdateSystem updateSystem)
        {

            log.Info(nameof(OnLoad) + " employment test");
            Settings = new EmploymentTrackerSettings();
			AssetDatabase.global.LoadSettings("Mod Settings Section", Settings, new EmploymentTrackerSettings());
			updateSystem.UpdateBefore<HighlightEmployeesSystem>(SystemUpdatePhase.MainLoop);
            
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
        }



        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
