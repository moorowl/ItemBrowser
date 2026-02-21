using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class VendingMachineDisplay : ObjectEntryDisplay<VendingMachine> {
		public ItemBrowserSlot vendingMachineSlot;
		public ItemBrowserSlot resultSlot;
		public PugText costText;

		public override IEnumerable<VendingMachine> OnSort(IEnumerable<VendingMachine> entries) {
			return entries
				.OrderBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Vendor));
		}
		
		protected override void OnRender(VendingMachine entry) {
			var buyCost = ObjectUtils.GetValue(entry.Result, 0, true);
			
			vendingMachineSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Vendor
			});
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			}, entry.Stock);
			costText.Render(buyCost.ToString());
		}

		protected override void OnRenderDescription(VendingMachine entry, EntryDescriptionButton description) {
			var buyCost = ObjectUtils.GetValue(entry.Result, 0, true);
			
			// Purchased from
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/VendingMachine_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Vendor),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}