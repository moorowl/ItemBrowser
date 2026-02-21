using PugMod;

namespace ItemBrowser.Api.Entries.Requirements.Types {
	public class ContentBundleAbsent : ObjectEntryRequirement {
		public readonly DataBlockAddress Address;

		private readonly string _term;
			
		public ContentBundleAbsent(DataBlockAddress address) {
			Address = address;
			_term = ScriptableData.TryGetDataBlock<ContentBundleDataBlock>(address, out var dataBlock) ? dataBlock.name : null;
		}

		public override bool IsFulfilled() {
			return !ClientWorldStateSystem.ActivatedContentBundles.Contains(Address);
		}

		public override string GetLocalizedDescription() {
			return string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/ContentBundleAbsent"),
				_term != null ? (API.Localization.GetLocalizedTerm($"ContentBundles/{_term}") ?? _term) : Address.ToString()
			);
		}
	}
}