using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class BreedingDisplay : ObjectEntryDisplay<Breeding> {
		public ItemBrowserSlot parentSlot;
		public ItemBrowserSlot childSlot;
		
		protected override void OnRender(Breeding entry) {
			parentSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.ParentType
			});
			childSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.ChildType
			});
		}

		protected override void OnRenderDescription(Breeding entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Breeding_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.ParentType)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Breeding_1",
				formatFields = new[] {
					entry.MealsRequired.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			
			// 60% chance to inherit the color of a random parent
			// 40% chance to mutate and be given a random color
			// - Variation 1: 33.3%
		}
	}
}