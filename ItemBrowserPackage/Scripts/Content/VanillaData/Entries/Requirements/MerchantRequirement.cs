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
			var worldInfo = ClientWorldStateSystem.WorldInfo;

			return Requirement switch {
				MerchantItemRequirement.HiveBossStatueActivated => worldInfo.hiveBossStatueIsActivated,
				MerchantItemRequirement.LarvaBossStatueActivated => worldInfo.larvaBossStatueIsActivated,
				MerchantItemRequirement.CoreActivated => worldInfo.coreIsActivated,
				MerchantItemRequirement.CoreBossDefeated => worldInfo.coreBossHasBeenKilled,
				_ => true
			};
		}

		public override string GetLocalizedDescription() {
			return API.Localization.GetLocalizedTerm($"ItemBrowser-ObjectEntryRequirements/Merchant_{Requirement}") ?? Requirement.ToString();
		}
	}
}