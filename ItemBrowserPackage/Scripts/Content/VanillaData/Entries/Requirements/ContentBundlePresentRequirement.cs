using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries.Requirements {
	public class ContentBundlePresentRequirement : ObjectEntryRequirement {
		public readonly DataBlockAddress Address;

		private readonly string _term;
			
		public ContentBundlePresentRequirement(DataBlockAddress address) {
			Address = address;
			_term = ScriptableData.TryGetDataBlock<ContentBundleDataBlock>(address, out var dataBlock) ? dataBlock.name : null;
		}

		public override bool IsFulfilled() {
			return ClientWorldStateSystem.ActivatedContentBundles.Contains(Address);
		}

		public override string GetLocalizedDescription() {
			if (_term == null)
				return string.Format(API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/ContentBundlePresent"), Address.ToString());

			var contentBundleDetails = API.Localization.GetLocalizedTerm($"ItemBrowser-ContentBundleDetails/{_term}");
			return contentBundleDetails ?? string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/ContentBundlePresent"),
				API.Localization.GetLocalizedTerm($"ContentBundles/{_term}") ?? _term
			);
		}
	}
}