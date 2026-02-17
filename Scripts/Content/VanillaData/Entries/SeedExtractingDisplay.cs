using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class SeedExtractingDisplay : ObjectEntryDisplay<SeedExtracting> {
		public ItemBrowserSlot extractedSlot;
		public ItemBrowserSlot seedSlot;
		public ItemBrowserSlot seedExtractorSlot;

		public override IEnumerable<SeedExtracting> OnSort(IEnumerable<SeedExtracting> entries) {
			return entries
				.OrderBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Extracted.Id, entry.Extracted.Variation))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Plant.Id, entry.Plant.Variation));
		}
		
		protected override void OnRender(SeedExtracting entry) {
			extractedSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Extracted.Id,
				variation = entry.Extracted.Variation
			}, entry.ExtractedAmount);
			seedSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Plant.Id,
				variation = entry.Plant.Variation
			});
			seedExtractorSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Extractor.Id,
				variation = entry.Extractor.Variation
			});
		}

		protected override void OnRenderDescription(SeedExtracting entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/SeedExtracting_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Plant.Id, entry.Plant.Variation),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Extractor.Id, entry.Extractor.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/SeedExtracting_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatRange(entry.ExtractedAmount),
					UserInterfaceUtils.FormatDuration(entry.TimeToExtract)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}