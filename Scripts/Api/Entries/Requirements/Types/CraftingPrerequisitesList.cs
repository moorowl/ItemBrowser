using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Api.Entries.Requirements.Types {
	public class CraftingPrerequisitesList : ObjectEntryRequirement {
		public readonly CraftingPrerequisites Prerequisites;
		
		public CraftingPrerequisitesList(CraftingPrerequisites prerequisites) {
			Prerequisites = prerequisites;
		}

		public override bool IsFulfilled() {
			var currentBundles = ClientWorldStateSystem.ActivatedContentBundles;
			return Prerequisites.IsSatisfied(currentBundles, ClientWorldStateSystem.WorldInfo);
		}

		public override string GetLocalizedDescription() {
			var or = API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/CraftingPrerequisitesList");
			var prerequisites = new List<string>();

			if (Prerequisites.ContentBundlePresent.hasValue) {
				var contentBundleName = GetContentBundleName(Prerequisites.ContentBundlePresent.value);
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/ContentBundlePresent"),
					API.Localization.GetLocalizedTerm($"ContentBundles/{contentBundleName}") ?? contentBundleName
				));
			}
			
			if (Prerequisites.ContentBundleAbsent.hasValue) {
				var contentBundleName = GetContentBundleName(Prerequisites.ContentBundleAbsent.value);
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/ContentBundleAbsent"),
					API.Localization.GetLocalizedTerm($"ContentBundles/{contentBundleName}") ?? contentBundleName
				));
			}
			
			if (Prerequisites.BirdBossKilled) {
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/BossDefeated"),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(ObjectID.BirdBoss)
				));
			}
			
			if (Prerequisites.OctopusBossKilled) {
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/BossDefeated"),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(ObjectID.OctopusBoss)
				));
			}
			
			if (Prerequisites.ScarabBossKilled) {
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/BossDefeated"),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(ObjectID.ScarabBoss)
				));
			}

			return string.Join(or, prerequisites);
		}

		private static string GetContentBundleName(DataBlockAddress address) {
			return ScriptableData.TryGetDataBlock<ContentBundleDataBlock>(address, out var dataBlock) ? dataBlock.name : address.ToString();
		}
	}
}