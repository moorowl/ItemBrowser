using System.Collections.Generic;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries.Requirements {
	public class CraftingPrerequisitesRequirement : ObjectEntryRequirement {
		public readonly CraftingPrerequisites Prerequisites;
		
		public CraftingPrerequisitesRequirement(CraftingPrerequisites prerequisites) {
			Prerequisites = prerequisites;
		}

		public override bool IsFulfilled() {
			var currentBundles = ClientWorldStateSystem.ActivatedContentBundles;
			return Prerequisites.IsSatisfied(currentBundles, ClientWorldStateSystem.WorldInfo);
		}

		public override string GetLocalizedDescription() {
			var or = API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/CraftingPrerequisites");
			var prerequisites = new List<string>();

			if (Prerequisites.ContentBundlePresent.hasValue) {
				var contentBundleName = GetContentBundleName(Prerequisites.ContentBundlePresent.value);
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/ContentBundlePresent"),
					API.Localization.GetLocalizedTerm($"ContentBundles/{contentBundleName}") ?? contentBundleName
				));
			}
			
			if (Prerequisites.ContentBundleAbsent.hasValue) {
				var contentBundleName = GetContentBundleName(Prerequisites.ContentBundleAbsent.value);
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/ContentBundleAbsent"),
					API.Localization.GetLocalizedTerm($"ContentBundles/{contentBundleName}") ?? contentBundleName
				));
			}

			var bossDefeatedPrerequisites = new HashSet<ObjectID>();
			if (Prerequisites.BirdBossKilled)
				bossDefeatedPrerequisites.Add(ObjectID.BirdBoss);
			if (Prerequisites.OctopusBossKilled)
				bossDefeatedPrerequisites.Add(ObjectID.OctopusBoss);
			if (Prerequisites.ScarabBossKilled)
				bossDefeatedPrerequisites.Add(ObjectID.ScarabBoss);
			if (Prerequisites.HydraBossNatureKilled)
				bossDefeatedPrerequisites.Add(ObjectID.HydraBossNature);
			if (Prerequisites.HydraBossSeaKilled)
				bossDefeatedPrerequisites.Add(ObjectID.HydraBossSea);
			if (Prerequisites.HydraBossDesertKilled)
				bossDefeatedPrerequisites.Add(ObjectID.HydraBossDesert);
			
			foreach (var boss in bossDefeatedPrerequisites) {
				prerequisites.Add(string.Format(
					API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/BossDefeated"),
					ObjectUtility.GetLocalizedDisplayNameOrDefault(boss)
				));
			}

			return string.Join(or, prerequisites);
		}

		private static string GetContentBundleName(DataBlockAddress address) {
			return ScriptableData.TryGetDataBlock<ContentBundleDataBlock>(address, out var dataBlock) ? dataBlock.name : address.ToString();
		}
	}
}