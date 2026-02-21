using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class MerchantDisplay : ObjectEntryDisplay<Merchant> {
		public ItemBrowserSlot merchantSlot;
		public ItemBrowserSlot resultSlot;
		public PugText costText;
		
		public override IEnumerable<Merchant> OnSort(IEnumerable<Merchant> entries) {
			return entries
				.OrderBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.MerchantType));
		}

		protected override void OnRender(Merchant entry) {
			merchantSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.MerchantType
			});
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			}, entry.Stock);

			var buyCost = ObjectUtils.GetValue(entry.Result, 0, true);
			costText.Render(buyCost.ToString());
		}

		protected override void OnRenderDescription(Merchant entry, EntryDescriptionButton description) {
			var buyCost = ObjectUtils.GetValue(entry.Result, 0, true);
			
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Merchant_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.MerchantType),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (entry.Requirement != MerchantItemRequirement.None) {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/Merchant_2_{entry.Requirement}",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Merchant_1",
				formatFields = new[] {
					entry.Stock.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}