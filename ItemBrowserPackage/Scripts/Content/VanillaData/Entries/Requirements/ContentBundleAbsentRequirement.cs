using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries.Requirements {
	public class ContentBundleAbsentRequirement : ObjectEntryRequirement {
		public readonly DataBlockAddress Address;

		private readonly string _term;
			
		public ContentBundleAbsentRequirement(DataBlockAddress address) {
			Address = address;
			_term = ScriptableData.TryGetDataBlock<ContentBundleDataBlock>(address, out var dataBlock) ? dataBlock.name : null;
		}

		public override bool IsFulfilled() {
			return !ClientWorldStateSystem.ActivatedContentBundles.Contains(Address);
		}

		public override string GetLocalizedDescription() {
			return string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/ContentBundleAbsent"),
				_term != null ? (API.Localization.GetLocalizedTerm($"ContentBundles/{_term}") ?? _term) : Address.ToString()
			);
		}
	}
}