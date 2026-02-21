using PugMod;

namespace ItemBrowser.Api.Entries.Requirements.Types {
	public class ContentBundlePresent : ObjectEntryRequirement {
		public readonly DataBlockAddress Address;

		private readonly string _term;
			
		public ContentBundlePresent(DataBlockAddress address) {
			Address = address;
			_term = ScriptableData.TryGetDataBlock<ContentBundleDataBlock>(address, out var dataBlock) ? dataBlock.name : null;
		}

		public override bool IsFulfilled() {
			return ClientWorldStateSystem.ActivatedContentBundles.Contains(Address);
		}

		public override string GetLocalizedDescription() {
			if (_term == null)
				return string.Format(API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/ContentBundlePresent"), Address.ToString());

			var contentBundleDetails = API.Localization.GetLocalizedTerm($"ItemBrowser-ContentBundleDetails/{_term}");
			return contentBundleDetails ?? string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/ContentBundlePresent"),
				API.Localization.GetLocalizedTerm($"ContentBundles/{_term}") ?? _term
			);
		}
	}
}