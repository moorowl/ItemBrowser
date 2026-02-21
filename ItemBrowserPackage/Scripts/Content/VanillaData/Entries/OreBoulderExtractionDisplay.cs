using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class OreBoulderExtractionDisplay : ObjectEntryDisplay<OreBoulderExtraction> {
		public ItemBrowserSlot oreBoulderSlot;
		public ItemBrowserSlot resultSlot;
		
		protected override void OnRender(OreBoulderExtraction entry) {
			oreBoulderSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.OreBoulder
			});
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			}, entry.TotalOre);
		}
		
		protected override void OnRenderDescription(OreBoulderExtraction entry, EntryDescriptionButton description) {
			// Drilled from
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/OreBoulderExtraction_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.OreBoulder)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			// x ore total
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/OreBoulderExtraction_1",
				formatFields = new[] {
					entry.TotalOre.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}