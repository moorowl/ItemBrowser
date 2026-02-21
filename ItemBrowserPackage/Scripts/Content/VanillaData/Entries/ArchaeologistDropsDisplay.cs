using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class ArchaeologistDropsDisplay : ObjectEntryDisplay<ArchaeologistDrops> {
		public ItemBrowserSlot resultSlot;
		public PugText chanceText;
		
		protected override void OnRender(ArchaeologistDrops entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			});
			chanceText.Render($"{UserInterfaceUtils.FormatChance(entry.Chance.Min)}-{UserInterfaceUtils.FormatChance(entry.Chance.Max)}%");
		}
		
		protected override void OnRenderDescription(ArchaeologistDrops entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/ArchaeologistDrops_0",
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser-ObjectEntryDescriptions/ArchaeologistDrops_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(entry.Chance.Min),
					UserInterfaceUtils.FormatChance(entry.Chance.Max)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}