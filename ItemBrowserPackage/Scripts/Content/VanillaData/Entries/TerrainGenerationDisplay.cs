using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class TerrainGenerationDisplay : ObjectEntryDisplay<TerrainGeneration> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot rightSourceSlot;
		public ItemBrowserSlot leftSourceSlot;
		public PugText plusText;
		
		protected override void OnRender(TerrainGeneration entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			});

			var generatesInTileset = entry.GeneratesInTileset != null;
			if (generatesInTileset) {
				// Biome + Tileset
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(entry.GeneratesInBiome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.wall, entry.GeneratesInTileset.Value);
			} else {
				// Biome + Any tileset
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(entry.GeneratesInBiome);
			}
		}

		protected override void OnRenderDescription(TerrainGeneration entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/TerrainGeneration_0",
				formatFields = new[] {
					$"BiomeNames/{entry.GeneratesInBiome}"
				},
				color = UserInterfaceUtils.DescriptionColor
			});
			
			if (entry.GeneratesInTileset != null) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/TerrainGeneration_1",
					formatFields = new[] {
						TileUtils.GetLocalizedDisplayName(TileType.wall, entry.GeneratesInTileset.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}