using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class VendingMachineDisplay : ObjectEntryDisplay<VendingMachine> {
		public ItemBrowserSlot vendingMachineSlot;
		public ItemBrowserSlot resultSlot;
		public PugText costText;

		public override IEnumerable<VendingMachine> OnSort(IEnumerable<VendingMachine> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Vendor));
		}
		
		protected override void OnRender(VendingMachine entry) {
			var buyCost = ObjectUtility.GetValue(entry.Result, 0, true);
			
			vendingMachineSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Vendor
			});
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			}, entry.Stock);
			costText.Render(buyCost.ToString());
		}

		protected override void OnRenderDescription(VendingMachine entry, EntryDescriptionButton description) {
			var buyCost = ObjectUtility.GetValue(entry.Result, 0, true);
			
			// Purchased from
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/VendingMachine_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Vendor),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}