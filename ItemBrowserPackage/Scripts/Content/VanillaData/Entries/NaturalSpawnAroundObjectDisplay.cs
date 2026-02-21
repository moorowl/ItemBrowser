using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class NaturalSpawnAroundObjectDisplay : ObjectEntryDisplay<NaturalSpawnAroundObject> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot biomeOrTilesetSlot;
		public ItemBrowserSlot seasonSlot;
		public ItemBrowserSlot entitySlot;
		public PugText plusTextRight;
		public PugText plusTextLeft;
		
		protected override void OnRender(NaturalSpawnAroundObject entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			});
			entitySlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Entity.Id,
				variation = entry.Entity.Variation
			});
			
			biomeOrTilesetSlot.gameObject.SetActive(false);
			seasonSlot.gameObject.SetActive(false);
			plusTextRight.gameObject.SetActive(false);
			plusTextLeft.gameObject.SetActive(false);
			
			if (entry.SpawnsInBiomes.Count > 0) {
				biomeOrTilesetSlot.gameObject.SetActive(true);
				biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.BiomeIcon(entry.SpawnsInBiomes.ToArray());
				plusTextRight.gameObject.SetActive(true);
			}
			if (entry.SpawnsOnTileset != null) {
				biomeOrTilesetSlot.gameObject.SetActive(true);
				biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.Tile(TileType.ground, entry.SpawnsOnTileset.Value);
				plusTextRight.gameObject.SetActive(true);
			}
		}

		protected override void OnRenderDescription(NaturalSpawnAroundObject entry, EntryDescriptionButton description) {
			if (entry.SpawnsInBiomes.Count > 0) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_0_SpecificBiome",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});

				foreach (var biome in entry.SpawnsInBiomes) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_9",
						formatFields = new[] {
							$"BiomeNames/{biome}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});
				}
			} else if (entry.SpawnsOnTileset != null) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_0_SpecificTile",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation),
						TileUtils.GetLocalizedDisplayName(TileType.ground, entry.SpawnsOnTileset.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_0_AnyBiome",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			if (entry.NeedToBeInsideBiome) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_1",
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
			
			if (entry.OnlySpawnsInCombat) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_10",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_3",
				color = UserInterfaceUtils.DescriptionColor
			});
			
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_4",
				formatFields = new[] {
					entry.SpawnRadius.ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (entry.DespawnRadius > 0f) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_5",
					formatFields = new[] {
						entry.DespawnRadius.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
			if (entry.SpawnLimit > 0) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_6",
					formatFields = new[] {
						entry.SpawnLimit.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_7",
				formatFields = new[] {
					entry.SpawnCooldown.Min.ToString(LocalizationManager.CurrentCulture),
					entry.SpawnCooldown.Max.ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (entry.SpawnLimit > 0 && entry.SpawnLimitReachedCooldown.Max > 0f) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/NaturalSpawnAroundObject_8",
					formatFields = new[] {
						entry.SpawnLimitReachedCooldown.Min.ToString(LocalizationManager.CurrentCulture),
						entry.SpawnLimitReachedCooldown.Max.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}