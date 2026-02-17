using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class UnlockingDisplay : ObjectEntryDisplay<Unlocking> {
		public ItemBrowserSlot outputSlot;
		public ItemBrowserSlot inputSlot;
		public ItemBrowserSlot keySlot;
		
		protected override void OnRender(Unlocking entry) {
			outputSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.OutputObject.Id,
				variation = entry.OutputObject.Variation
			});
			inputSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.InputObject.Id,
				variation = entry.InputObject.Variation
			});
			keySlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Key.Id,
				variation = entry.Key.Variation
			});
		}

		protected override void OnRenderDescription(Unlocking entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Unlocking_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Key.Id, entry.Key.Variation),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.InputObject.Id, entry.InputObject.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Unlocking_1",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.OutputObject.Id, entry.OutputObject.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}