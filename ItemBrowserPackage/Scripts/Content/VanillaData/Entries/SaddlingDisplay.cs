using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class SaddlingDisplay : ObjectEntryDisplay<Saddling> {
		public ItemBrowserSlot normalSlot;
		public ItemBrowserSlot saddledSlot;
		public ItemBrowserSlot saddleSlot;
		
		public override IEnumerable<Saddling> OnSort(IEnumerable<Saddling> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.SaddledType));
		}
		
		protected override void OnRender(Saddling entry) {
			normalSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.NormalType
			});
			saddledSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.SaddledType
			});
			saddleSlot.Icon = new BasicSlotIcon(entry.Saddles.Select(saddle => new ObjectDataCD { objectID = saddle}).ToArray());
		}

		protected override void OnRenderDescription(Saddling entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Saddling_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.NormalType)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}