using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using PugMod;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class LockedChestDropsDisplay : ObjectEntryDisplay<LockedChestDrops> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot biomeSlot;
		public ItemBrowserSlot blockSlot;
		public PugText chanceText;
		
		protected override void OnRender(LockedChestDrops entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			});
			biomeSlot.DisplayedObject = new DisplayedObject.BiomeIcon(entry.RequiredBiome);
			blockSlot.DisplayedObject = new DisplayedObject.Tile(TileType.wall, entry.RequiredTileset);
			
			chanceText.Render(UserInterfaceUtils.FormatChance(entry.Chance) + "%");
		}

		protected override void OnRenderDescription(LockedChestDrops entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/LockedChestDrops_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(PugDatabase.TryGetTileItemInfo(TileType.wall, (int) entry.RequiredTileset).objectID),
					API.Localization.GetLocalizedTerm($"BiomeNames/{entry.RequiredBiome}") ?? "???"
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser-ObjectEntryDescriptions/LockedChestDrops_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(entry.Chance)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}