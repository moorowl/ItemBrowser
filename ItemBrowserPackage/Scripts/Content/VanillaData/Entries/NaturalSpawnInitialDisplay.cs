using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class NaturalSpawnInitialDisplay : ObjectEntryDisplay<NaturalSpawnInitial> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot rightSourceSlot;
		public ItemBrowserSlot leftSourceSlot;
		public PugText plusText;
		public PugText chanceText;

		protected override void OnRender(NaturalSpawnInitial entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			});
			
			if (entry.SpawnCheck.biome != Biome.None) {
				// Specific biome + Tile
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.Icon = new BiomeSlotIcon(entry.SpawnCheck.biome);
				rightSourceSlot.Icon = new TileSlotIcon(entry.SpawnCheck.tileType, entry.TilesetToSpawnOn);
			} else {
				// Any biome + Tile
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.Icon = new TileSlotIcon(entry.SpawnCheck.tileType, entry.TilesetToSpawnOn);
			}

			var spawnChance = UserInterfaceUtility.FormatChance(LootUtility.GetChanceForActiveWorld(entry.SpawnCheck.spawnChance));
			chanceText.Render(spawnChance + "%");
		}

		protected override void OnRenderDescription(NaturalSpawnInitial entry, EntryDescriptionButton description) {
			if (entry.SpawnCheck.biome != Biome.None) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_0_SpecificBiome",
					formatFields = new[] {
						$"BiomeNames/{entry.SpawnCheck.biome}"
					},
					color = UserInterfaceUtility.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_0_AnyBiome",
					color = UserInterfaceUtility.DescriptionColor
				});
			}
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_1",
				formatFields = new[] {
					UserInterfaceUtility.FormatChance(LootUtility.GetChanceForActiveWorld(entry.SpawnCheck.spawnChance))	
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_2",
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_3",
				formatFields = new[] {
					TileUtility.GetLocalizedDisplayName(entry.SpawnCheck.tileType, entry.TilesetToSpawnOn)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});

			var adjacentTiles = entry.SpawnCheck.adjacentTiles.list;
			if (adjacentTiles.Count > 0) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_4",
					color = UserInterfaceUtility.DescriptionColor
				});
				
				foreach (var adjacentTile in adjacentTiles) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnInitial_3",
						formatFields = new[] {
							TileUtility.GetLocalizedDisplayName(adjacentTile.tileType, adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : null)
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtility.DescriptionColor
					});
				}
			}
		}
	}
}