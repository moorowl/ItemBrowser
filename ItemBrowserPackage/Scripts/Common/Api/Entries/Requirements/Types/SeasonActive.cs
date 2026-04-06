using PugMod;

namespace ItemBrowser.Common.Api.Entries.Requirements.Types {
	public class SeasonActive : ObjectEntryRequirement {
		public readonly Season Season;
			
		public SeasonActive(Season season) {
			Season = season;
		}

		public override bool IsFulfilled() {
			return Manager.prefs.season == Season;
		}

		public override string GetLocalizedDescription() {
			return string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/SeasonActive"),
				API.Localization.GetLocalizedTerm($"Seasons/{Season}")
			);
		}
	}
}