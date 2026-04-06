using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class OreBoulderExtractionDisplay : ObjectEntryDisplay<OreBoulderExtraction> {
		public ItemBrowserSlot oreBoulderSlot;
		public ItemBrowserSlot resultSlot;
		
		protected override void OnRender(OreBoulderExtraction entry) {
			oreBoulderSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.OreBoulder
			});
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			}, entry.TotalOre);
		}
		
		protected override void OnRenderDescription(OreBoulderExtraction entry, EntryDescriptionButton description) {
			// Drilled from
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/OreBoulderExtraction_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.OreBoulder)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			// x ore total
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/OreBoulderExtraction_1",
				formatFields = new[] {
					entry.TotalOre.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}