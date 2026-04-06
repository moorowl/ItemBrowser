using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class StructureContentsDisplay : ObjectEntryDisplay<StructureContents> {
		public ItemBrowserSlot resultSlot;
		public PugText structureTypeText;
		public PugText structureNameText;

		public override IEnumerable<StructureContents> OnSort(IEnumerable<StructureContents> entries) {
			return entries.OrderByDescending(entry => entry.Dungeon != null ? 1 : 0)
				.ThenByDescending(entry => entry.Result.Amount)
				.ThenByDescending(entry => entry.Scene ?? entry.Dungeon);
		}
		
		protected override void OnRender(StructureContents entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			}, entry.Scene != null ? entry.Result.Amount : 1);

			if (entry.Scene != null) {
				structureTypeText.Render("ItemBrowser-StructureTypes/Scene");
				structureNameText.Render(StructureUtility.GetPersistentSceneName(entry.Scene));
			} else if (entry.Dungeon != null) {
				structureTypeText.Render("ItemBrowser-StructureTypes/Dungeon");
				structureNameText.Render(entry.Dungeon);
			}
		}

		protected override void OnRenderDescription(StructureContents entry, EntryDescriptionButton description) {
			if (entry.Scene != null) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/StructureContents_1_Scene",
					formatFields = new[] {
						StructureUtility.GetPersistentSceneName(entry.Scene)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/StructureContents_2_Scene",
					formatFields = new[] {
						entry.Result.Amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			} else if (entry.Dungeon != null) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/StructureContents_1_Dungeon",
					formatFields = new[] {
						entry.Dungeon
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/StructureContents_2_Dungeon",
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
	}
}