using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class BreedingDisplay : ObjectEntryDisplay<Breeding> {
		public ItemBrowserSlot parentSlot;
		public ItemBrowserSlot childSlot;
		
		protected override void OnRender(Breeding entry) {
			parentSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.ParentType
			});
			childSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.ChildType
			});
		}

		protected override void OnRenderDescription(Breeding entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Breeding_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.ParentType)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Breeding_1",
				formatFields = new[] {
					entry.MealsRequired.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			
			// 60% chance to inherit the color of a random parent
			// 40% chance to mutate and be given a random color
			// - Variation 1: 33.3%
		}
	}
}