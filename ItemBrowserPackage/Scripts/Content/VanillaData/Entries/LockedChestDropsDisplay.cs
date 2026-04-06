using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;
using PugMod;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class LockedChestDropsDisplay : ObjectEntryDisplay<LockedChestDrops> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot biomeSlot;
		public ItemBrowserSlot blockSlot;
		public PugText chanceText;
		
		protected override void OnRender(LockedChestDrops entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			});
			biomeSlot.Icon = new BiomeSlotIcon(entry.RequiredBiome);
			blockSlot.Icon = new TileSlotIcon(TileType.wall, entry.RequiredTileset);
			
			chanceText.Render(UserInterfaceUtility.FormatChance(entry.Chance) + "%");
		}

		protected override void OnRenderDescription(LockedChestDrops entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/LockedChestDrops_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(PugDatabase.TryGetTileItemInfo(TileType.wall, (int) entry.RequiredTileset).objectID),
					API.Localization.GetLocalizedTerm($"BiomeNames/{entry.RequiredBiome}") ?? "???"
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser-ObjectEntryDescriptions/LockedChestDrops_1",
				formatFields = new[] {
					UserInterfaceUtility.FormatChance(entry.Chance)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}