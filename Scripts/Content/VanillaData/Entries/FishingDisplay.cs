using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class FishingDisplay : ObjectEntryDisplay<Fishing> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot rightSourceSlot;
		public ItemBrowserSlot leftSourceSlot;
		public PugText plusText;
		public PugText chanceText;
		public PugText catchTypeText;
		
		protected override void OnRender(Fishing entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			});
			
			if (entry.Biome != Biome.None) {
				// Biome + Normal water
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(entry.Biome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, Tileset.Dirt);
			} else {
				// Any biome + specific liquid
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, entry.Tileset);
			}
			
			chanceText.Render(UserInterfaceUtils.FormatChance(entry.Chance) + "%");
			catchTypeText.Render($"ItemBrowser-CatchTypes/{entry.Type}");
		}

		protected override void OnRenderDescription(Fishing entry, EntryDescriptionButton description) {
			if (entry.Biome != Biome.None) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Fishing_0_Biome",
					formatFields = new[] {
						$"BiomeNames/{entry.Biome}"
					},
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Fishing_0_Liquid",
					formatFields = new[] {
						TileUtils.GetLocalizedDisplayName(TileType.water, entry.Tileset)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			description.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser-ObjectEntryDescriptions/Fishing_1_{entry.Type}",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(entry.Chance)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}