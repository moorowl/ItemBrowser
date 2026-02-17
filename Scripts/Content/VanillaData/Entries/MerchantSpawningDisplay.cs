using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class MerchantSpawningDisplay : ObjectEntryDisplay<MerchantSpawning> {
		public ItemBrowserSlot merchantSlot;
		public ItemBrowserSlot idolSlot;
		
		protected override void OnRender(MerchantSpawning entry) {
			merchantSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Merchant
			});
			idolSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Idol
			});
		}

		protected override void OnRenderDescription(MerchantSpawning entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/MerchantSpawning_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Idol)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}