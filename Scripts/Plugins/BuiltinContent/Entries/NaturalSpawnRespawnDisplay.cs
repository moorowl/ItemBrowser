using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class NaturalSpawnRespawnDisplay : ObjectEntryDisplay<NaturalSpawnRespawn> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot rightSourceSlot;
		[SerializeField]
		private BasicItemSlot leftSourceSlot;
		[SerializeField]
		private PugText plusText;
		[SerializeField]
		private PugText chanceText;

		public override void RenderSelf() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			});

			var isFromBiome = Entry.SpawnCheck.biome != Biome.None;
			
			if (isFromBiome) {
				// Specific biome + Tile
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.SpawnCheck.biome);
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn);
			} else {
				// Any biome + Tile
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.DisplayedObject = new DisplayedObject.Tile(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn);
			}

			var chance = LootUtils.GetChanceForActiveWorld(Entry.SpawnCheck.spawnChance);
			chanceText.Render((chance * 100).ToString("0.##") + "%");
			
			RenderMoreInfo(isFromBiome, chance);
		}

		private void RenderMoreInfo(bool isFromBiome, float spawnChance) {
			if (isFromBiome) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_0_SpecificBiome",
					formatFields = new[] {
						$"BiomeNames/{Entry.SpawnCheck.biome}"
					},
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_0_AnyBiome",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(spawnChance)	
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_2",
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_3",
				formatFields = new[] {
					TileUtils.GetLocalizedDisplayName(Entry.SpawnCheck.tileType, Entry.TilesetToSpawnOn)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			var adjacentTiles = Entry.SpawnCheck.adjacentTiles.list;
			if (adjacentTiles.Count > 0) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_4",
					color = UserInterfaceUtils.DescriptionColor
				});
				
				foreach (var adjacentTile in adjacentTiles) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_3",
						formatFields = new[] {
							TileUtils.GetLocalizedDisplayName(adjacentTile.tileType, adjacentTile.mustAlsoMatchTileset ? adjacentTile.tileset : null)
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}
			}
			
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_5",
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_6",
				formatFields = new[] {
					Entry.SpawnCheck.spawnChanceDecay.ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_7",
				formatFields = new[] {
					Entry.SpawnCheck.maxSpawnPerTile.GetValueForCurrentPlatform().ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_8",
				formatFields = new[] {
					Entry.SpawnCheck.maxSpawnsPerRespawn.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnRespawn_9",
				formatFields = new[] {
					Entry.SpawnCheck.minTilesRequired.ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}