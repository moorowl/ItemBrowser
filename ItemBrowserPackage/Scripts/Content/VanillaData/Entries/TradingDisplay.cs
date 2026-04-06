using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class TradingDisplay : ObjectEntryDisplay<Trading> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot vendorSlot;
		public ItemBrowserSlot[] ingredientSlots;
		
		protected override void OnRender(Trading entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			}, entry.Amount);
			vendorSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Vendor.Id,
				variation = entry.Vendor.Variation
			}); 
			
			foreach (var slot in ingredientSlots)
				slot.gameObject.SetActive(false);

			var requiredObjectsToCraft = GetRequiredObjectsToCraft(entry.Result);
			for (var i = 0; i < requiredObjectsToCraft.Count; i++) {
				if (i >= ingredientSlots.Length)
					break;
					
				var craftingObject = requiredObjectsToCraft[i];
				var slot = ingredientSlots[i];
				slot.gameObject.SetActive(true);

				slot.Icon = new BasicSlotIcon(new ObjectDataCD {
					objectID = craftingObject.objectID,
					amount = craftingObject.amount
				});
			}
		}

		protected override void OnRenderDescription(Trading entry, EntryDescriptionButton description) {
			// Trading with
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Trading_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Vendor.Id, entry.Vendor.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			// Materials header
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Trading_1",
				color = UserInterfaceUtility.DescriptionColor
			});
			foreach (var craftingObject in GetRequiredObjectsToCraft(entry.Result)) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Trading_2",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(craftingObject.objectID),
						craftingObject.amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
		
		private static List<CraftingObject> GetRequiredObjectsToCraft((ObjectID Id, int Variation) item) {
			var objectInfo = PugDatabase.GetObjectInfo(item.Id, item.Variation);
			return objectInfo.requiredObjectsToCraft.Where(craftingObject => craftingObject.objectID != ObjectID.None).ToList();
		}
	}
}