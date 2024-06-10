using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using System.Collections.Generic;

namespace DCMilestoneRewards
{
	[FileLocation(nameof(DCMilestoneRewards))]
	[SettingsUIGroupOrder(moneyGroup)]
	[SettingsUIShowGroupName(moneyGroup)]
	public class Setting : ModSetting
	{
		public const string kSection = "Main";

		public const string moneyGroup = "Money";

		public Setting(IMod mod) : base(mod)
		{
			this.disableMilestoneRewards = false;
		}

		public override void SetDefaults()
		{
			this.disableMilestoneRewards = false;
		}

		[SettingsUISection(kSection, moneyGroup)]
		public bool disableMilestoneRewards { get; set; }

	}

	public class LocaleEN : IDictionarySource
	{
		private readonly Setting settings;
		public LocaleEN(Setting setting)
		{
			settings = setting;
		}
		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ settings.GetSettingsLocaleID(), "Difficulty: Milestone Rewards" },
				{ settings.GetOptionTabLocaleID(Setting.kSection), "Main" },

				{ settings.GetOptionLabelLocaleID(nameof(Setting.disableMilestoneRewards)), "Disable Milestone Rewards" },
				{ settings.GetOptionDescLocaleID(nameof(Setting.disableMilestoneRewards)), $"Remove the money bonus when reaching a milestone" },

			};
		}


		public void Unload()
		{

		}
	}
}
