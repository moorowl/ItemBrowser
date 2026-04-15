using ItemBrowser.Common.Api.Entries;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries.Requirements {
	public class SeasonActiveRequirement : ObjectEntryRequirement {
		public readonly Season Season;
			
		public SeasonActiveRequirement(Season season) {
			Season = season;
		}

		public override bool IsFulfilled() {
			return Manager.prefs.season == Season;
		}

		public override string GetLocalizedDescription() {
			return string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/SeasonActive"),
				API.Localization.GetLocalizedTerm($"Seasons/{Season}") ?? Season.ToString()
			);
		}
	}
}