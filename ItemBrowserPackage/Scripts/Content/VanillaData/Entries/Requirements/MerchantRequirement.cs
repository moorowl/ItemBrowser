using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries.Requirements {
	public class MerchantRequirement : ObjectEntryRequirement {
		public readonly MerchantItemRequirement Requirement;
			
		public MerchantRequirement(MerchantItemRequirement requirement) {
			Requirement = requirement;
		}

		public override bool IsFulfilled() {
			return Requirement switch {
				MerchantItemRequirement.HiveBossStatueActivated => true, // not possible to get these on the client unfortunately
				MerchantItemRequirement.LarvaBossStatueActivated => true,
				MerchantItemRequirement.CoreActivated => ClientWorldStateSystem.WorldInfo.coreIsActivated,
				MerchantItemRequirement.CoreBossDefeated => ClientWorldStateSystem.WorldInfo.coreBossHasBeenKilled,
				_ => true
			};
		}

		public override string GetLocalizedDescription() {
			return API.Localization.GetLocalizedTerm($"ItemBrowser-ObjectEntryRequirements/Merchant_{Requirement}") ?? Requirement.ToString();
		}
	}
}