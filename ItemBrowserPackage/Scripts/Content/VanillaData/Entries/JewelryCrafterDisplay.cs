using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class JewelryCrafterDisplay : ObjectEntryDisplay<JewelryCrafter> {
		public ItemBrowserSlot unpolishedSlot;
		public ItemBrowserSlot polishedSlot;
		public PugText chanceText;
		
		protected override void OnRender(JewelryCrafter entry) {
			unpolishedSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.UnpolishedVersion
			});
			polishedSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.PolishedVersion
			});
			chanceText.Render($"{UserInterfaceUtils.FormatChance(entry.Chance.Min)}-{UserInterfaceUtils.FormatChance(entry.Chance.Max)}%");
		}

		protected override void OnRenderDescription(JewelryCrafter entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/JewelryCrafter_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.UnpolishedVersion)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/JewelryCrafter_1",
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