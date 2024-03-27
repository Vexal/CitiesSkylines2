using Colossal.Logging;
using EmploymentTracker;
using Game;
using Game.Buildings;
using Game.Modding;
using Game.SceneFlow;
using Unity.Entities;

namespace EmploymentTracker
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentTracker)}.{nameof(Mod)}").SetShowsErrorsInUI(true);

        public void OnLoad(UpdateSystem updateSystem)
        {

            log.Info(nameof(OnLoad) + " employment test");
            updateSystem.UpdateBefore<TestSystem>(SystemUpdatePhase.Rendering);
            throw new System.Exception("crash");
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

        }



        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
