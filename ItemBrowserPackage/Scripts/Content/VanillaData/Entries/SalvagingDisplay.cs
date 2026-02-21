using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class SalvagingDisplay : ObjectEntryDisplay<Salvaging> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot sourceSlot;
		public ItemBrowserSlot stationSlot;

		public override IEnumerable<Salvaging> OnSort(IEnumerable<Salvaging> entries) {
			return entries
				.OrderBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.ItemSalvaged));
		}
		
		protected override void OnRender(Salvaging entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			}, entry.ResultAmount);
			sourceSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.ItemSalvaged
			});
			stationSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD[] {
				new() {
					objectID = ObjectID.SalvageAndRepairStation
				},
				new() {
					objectID = ObjectID.Shredder
				}
			});
		}

		protected override void OnRenderDescription(Salvaging entry, EntryDescriptionButton description) {
			// Salvaged from
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Salvaging_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.ItemSalvaged)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (entry.ResultAmount.Min != entry.ResultAmount.Max) {
				// Drops x-x
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Salvaging_1_Durability",
					formatFields = new[] {
						entry.ResultAmount.Min.ToString(),
						entry.ResultAmount.Max.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				// Always drops x
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Salvaging_1",
					formatFields = new[] {
						entry.ResultAmount.Max.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}