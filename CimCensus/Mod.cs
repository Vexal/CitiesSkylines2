using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace CimCensus
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(CimCensus)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		public static Mod INSTANCE;

		public void OnLoad(UpdateSystem updateSystem)
		{
			INSTANCE = this;
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");


			updateSystem.UpdateAfter<StatisticsCalculationSystem>(SystemUpdatePhase.GameSimulation);
		}

		public void OnDispose()
		{
			log.Info(nameof(OnDispose));
		}
	}
}
