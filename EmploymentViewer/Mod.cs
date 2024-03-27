using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using System;

namespace EmploymentViewer
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(EmploymentViewer)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));
            throw new Exception("crash");

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
