using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace NoTrafficDespawn
{
	public class Mod : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(NoTrafficDespawn)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
		public static Mod INSTANCE;

		public TrafficDespawnSettings settings;

		public void OnLoad(UpdateSystem updateSystem)
		{
			INSTANCE = this;
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			settings = new TrafficDespawnSettings(this);
			settings.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(settings));

			AssetDatabase.global.LoadSettings(nameof(NoTrafficDespawn), settings, new TrafficDespawnSettings(this));
			updateSystem.UpdateBefore<NewStuckMovingObjectSystem>(SystemUpdatePhase.Modification1);
			updateSystem.UpdateAfter<DisableTrafficDespawnSystem>(SystemUpdatePhase.Modification1);
		}

		public void OnDispose()
		{
			log.Info(nameof(OnDispose));
			if (settings != null)
			{
				settings.UnregisterInOptionsUI();
				settings = null;
			}
		}
	}
}
