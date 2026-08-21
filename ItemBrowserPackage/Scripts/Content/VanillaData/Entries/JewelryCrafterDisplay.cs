using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class JewelryCrafterDisplay : ObjectEntryDisplay<JewelryCrafter> {
		public ItemBrowserSlot unpolishedSlot;
		public ItemBrowserSlot polishedSlot;
		public PugText chanceText;
		
		protected override void OnRender(JewelryCrafter entry) {
			unpolishedSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.UnpolishedVersion
			});
			polishedSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.PolishedVersion
			});
			chanceText.Render($"{UserInterfaceUtility.FormatChance(entry.Chance.Min)}-{UserInterfaceUtility.FormatChance(entry.Chance.Max)}%");
		}

		protected override void OnRenderDescription(JewelryCrafter entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/JewelryCrafter_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.UnpolishedVersion)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});

			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/JewelryCrafter_1",
				formatFields = new[] {
					UserInterfaceUtility.FormatChance(entry.Chance.Min),
					UserInterfaceUtility.FormatChance(entry.Chance.Max)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});	
		}
		
		public static void OpenCraftingSkillPage() {
			UserInterfaceUtility.OpenSkillPage(SkillID.Crafting);
		}
	}
}