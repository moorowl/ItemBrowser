using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CattleProduceDisplay : ObjectEntryDisplay<CattleProduce> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot cattleSlot;
		public ItemBrowserSlot[] feedSlots;
		
		protected override void OnRender(CattleProduce entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			}, entry.Amount);
			cattleSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Cattle
			}); 
			
			foreach (var slot in feedSlots)
				slot.gameObject.SetActive(false);
			
			for (var i = 0; i < entry.SuitableFeed.Count; i++) {
				if (i >= feedSlots.Length)
					break;
				
				var slot = feedSlots[i];
				slot.gameObject.SetActive(true);
				slot.Icon = new TagSlotIcon(entry.SuitableFeed[i], entry.SuitableFeedRequired);
			}
		}

		protected override void OnRenderDescription(CattleProduce entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = entry.SuitableFeedRequired != 1 ? "ItemBrowser-ObjectEntryDescriptions/CattleProduce_0_Plural" : "ItemBrowser-ObjectEntryDescriptions/CattleProduce_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Cattle),
					entry.SuitableFeedRequired.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/CattleProduce_1",
				color = UserInterfaceUtility.DescriptionColor
			});
			foreach (var feed in entry.SuitableFeed) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/CattleProduce_2",
					formatFields = new[] {
						$"ItemBrowser-ObjectCategoryNames/{feed}"
					},
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
	}
}