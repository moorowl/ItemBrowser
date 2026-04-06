using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class SalvagingDisplay : ObjectEntryDisplay<Salvaging> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot sourceSlot;
		public ItemBrowserSlot stationSlot;

		public override IEnumerable<Salvaging> OnSort(IEnumerable<Salvaging> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.ItemSalvaged));
		}
		
		protected override void OnRender(Salvaging entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			}, entry.ResultAmount);
			sourceSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.ItemSalvaged
			});
			stationSlot.Icon = new BasicSlotIcon(new ObjectDataCD[] {
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
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.ItemSalvaged)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
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
					color = UserInterfaceUtility.DescriptionColor
				});
			} else {
				// Always drops x
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Salvaging_1",
					formatFields = new[] {
						entry.ResultAmount.Max.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
	}
}