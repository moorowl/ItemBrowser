using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class UnlockingDisplay : ObjectEntryDisplay<Unlocking> {
		public ItemBrowserSlot outputSlot;
		public ItemBrowserSlot inputSlot;
		public ItemBrowserSlot keySlot;
		
		protected override void OnRender(Unlocking entry) {
			outputSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.OutputObject.Id,
				variation = entry.OutputObject.Variation
			});
			inputSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.InputObject.Id,
				variation = entry.InputObject.Variation
			});
			keySlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Key.Id,
				variation = entry.Key.Variation
			});
		}

		protected override void OnRenderDescription(Unlocking entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Unlocking_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Key.Id, entry.Key.Variation),
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.InputObject.Id, entry.InputObject.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Unlocking_1",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.OutputObject.Id, entry.OutputObject.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}