using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class MerchantDisplay : ObjectEntryDisplay<Merchant> {
		public ItemBrowserSlot merchantSlot;
		public ItemBrowserSlot resultSlot;
		public PugText costText;
		
		public override IEnumerable<Merchant> OnSort(IEnumerable<Merchant> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.MerchantType));
		}

		protected override void OnRender(Merchant entry) {
			merchantSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.MerchantType
			});
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			}, entry.Stock);

			var buyCost = ObjectUtility.GetValue(entry.Result, 0, true);
			costText.Render(buyCost.ToString());
		}

		protected override void OnRenderDescription(Merchant entry, EntryDescriptionButton description) {
			var buyCost = ObjectUtility.GetValue(entry.Result, 0, true);
			
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Merchant_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.MerchantType),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Merchant_1",
				formatFields = new[] {
					entry.Stock.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}