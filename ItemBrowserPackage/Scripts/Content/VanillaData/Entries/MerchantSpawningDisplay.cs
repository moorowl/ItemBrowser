using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class MerchantSpawningDisplay : ObjectEntryDisplay<MerchantSpawning> {
		public ItemBrowserSlot merchantSlot;
		public ItemBrowserSlot idolSlot;
		
		protected override void OnRender(MerchantSpawning entry) {
			merchantSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Merchant
			});
			idolSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Idol
			});
		}

		protected override void OnRenderDescription(MerchantSpawning entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/MerchantSpawning_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Idol)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}