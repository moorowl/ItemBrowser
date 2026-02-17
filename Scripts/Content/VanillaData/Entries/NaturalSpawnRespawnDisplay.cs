using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class NaturalSpawnRespawnDisplay : ObjectEntryDisplay<NaturalSpawnRespawn> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot rightSourceSlot;
		public ItemBrowserSlot leftSourceSlot;
		public PugText plusText;
		public PugText chanceText;
		
		protected override void OnRender(NaturalSpawnRespawn entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			});
			
			if (entry.SpawnCheck.biome != Biome.None) {
				// Specific biome + Tile
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(entry.SpawnCheck.biome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(entry.SpawnCheck.tileType, entry.TilesetToSpawnOn);
			} else {
				// Any biome + Tile
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(entry.SpawnCheck.tileType, entry.TilesetToSpawnOn);
			}

			var spawnChance = UserInterfaceUtils.FormatChance(LootUtils.GetChanceForActiveWorld(entry.SpawnCheck.spawnChance));
			chanceText.Render(spawnChance + "%");
		}

		protected override void OnRenderDescription(NaturalSpawnRespawn entry, EntryDescriptionButton description) {
			if (entry.SpawnCheck.biome != Biome.None) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_0_SpecificBiome",
					formatFields = new[] {
						$"BiomeNames/{entry.SpawnCheck.biome}"
					},
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_0_AnyBiome",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(LootUtils.GetChanceForActiveWorld(entry.SpawnCheck.spawnChance))
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_2",
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_3",
				formatFields = new[] {
					TileUtils.GetLocalizedDisplayName(entry.SpawnCheck.tileType, entry.TilesetToSpawnOn)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			var adjacentTiles = entry.SpawnCheck.adjacentTiles.list;
			if (adjacentTiles.Count > 0) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_4",
					color = UserInterfaceUtils.DescriptionColor
				});
				
				foreach (var adjacentTile in adjacentTiles) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_3",
						formatFields = new[] {
							TileUtils.GetLocalizedDisplayName(adjacentTile.tileType, adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : null)
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}
			}
			
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_5",
				color = UserInterfaceUtils.DescriptionColor
			});
			if (entry.SpawnCheck.spawnChanceDecay > 0f) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_6",
					formatFields = new[] {
						entry.SpawnCheck.spawnChanceDecay.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}

			if (entry.SpawnCheck.maxSpawnPerTile.GetValueForCurrentPlatform() > 0f) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_7",
					formatFields = new[] {
						entry.SpawnCheck.maxSpawnPerTile.GetValueForCurrentPlatform().ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			if (entry.SpawnCheck.maxSpawnsPerRespawn > 0) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_8",
					formatFields = new[] {
						entry.SpawnCheck.maxSpawnsPerRespawn.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			if (entry.SpawnCheck.minTilesRequired > 0) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnRespawn_9",
					formatFields = new[] {
						entry.SpawnCheck.minTilesRequired.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}