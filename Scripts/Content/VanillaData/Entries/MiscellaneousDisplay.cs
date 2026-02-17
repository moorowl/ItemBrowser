using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class MiscellaneousDisplay : ObjectEntryDisplay<Miscellaneous> {
		public ItemBrowserSlot resultSlot;
		public PugText descriptionText;
		
		protected override void OnRender(Miscellaneous entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			}, entry.Result.Amount);

			if (!entry.HasSource) {
				descriptionText.Render(API.Localization.GetLocalizedTerm(entry.Term));
			} else {
				descriptionText.Render(string.Format(
					API.Localization.GetLocalizedTerm(entry.Term),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Source.Id, entry.Source.Variation)
				));	
			}
		}

		protected override void OnRenderDescription(Miscellaneous entry, EntryDescriptionButton description) {
			if (!entry.HasSource) {
				description.AddLine(new TextAndFormatFields {
					text = entry.Term,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = entry.Term,
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Source.Id, entry.Source.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}