using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class ArchaeologistDropsDisplay : ObjectEntryDisplay<ArchaeologistDrops> {
		public ItemBrowserSlot resultSlot;
		public PugText chanceText;
		
		protected override void OnRender(ArchaeologistDrops entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			});
			chanceText.Render($"{UserInterfaceUtility.FormatChance(entry.Chance.Min)}-{UserInterfaceUtility.FormatChance(entry.Chance.Max)}%");
		}
		
		protected override void OnRenderDescription(ArchaeologistDrops entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/ArchaeologistDrops_0",
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser-ObjectEntryDescriptions/ArchaeologistDrops_1",
				formatFields = new[] {
					UserInterfaceUtility.FormatChance(entry.Chance.Min),
					UserInterfaceUtility.FormatChance(entry.Chance.Max)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}

		public static void OpenMiningSkillPage() {
			UserInterfaceUtility.OpenSkillPage(SkillID.Mining);
		}
	}
}