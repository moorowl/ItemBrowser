using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class SeedExtractingDisplay : ObjectEntryDisplay<SeedExtracting> {
		public ItemBrowserSlot extractedSlot;
		public ItemBrowserSlot seedSlot;
		public ItemBrowserSlot seedExtractorSlot;

		public override IEnumerable<SeedExtracting> OnSort(IEnumerable<SeedExtracting> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Extracted.Id, entry.Extracted.Variation))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Plant.Id, entry.Plant.Variation));
		}
		
		protected override void OnRender(SeedExtracting entry) {
			extractedSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Extracted.Id,
				variation = entry.Extracted.Variation
			}, entry.ExtractedAmount);
			seedSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Plant.Id,
				variation = entry.Plant.Variation
			});
			seedExtractorSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Extractor.Id,
				variation = entry.Extractor.Variation
			});
		}

		protected override void OnRenderDescription(SeedExtracting entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/SeedExtracting_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Plant.Id, entry.Plant.Variation),
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Extractor.Id, entry.Extractor.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/SeedExtracting_1",
				formatFields = new[] {
					UserInterfaceUtility.FormatRange(entry.ExtractedAmount),
					UserInterfaceUtility.FormatDuration(entry.TimeToExtract)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}