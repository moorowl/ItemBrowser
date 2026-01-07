using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class NaturalSpawnAroundObjectDisplay : ObjectEntryDisplay<NaturalSpawnAroundObject> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot biomeOrTilesetSlot;
		[SerializeField]
		private BasicItemSlot seasonSlot;
		[SerializeField]
		private BasicItemSlot entitySlot;
		[SerializeField]
		private PugText plusTextRight;
		[SerializeField]
		private PugText plusTextLeft;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			});
			entitySlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Entity.Id,
				variation = Entry.Entity.Variation
			});
			
			biomeOrTilesetSlot.gameObject.SetActive(false);
			seasonSlot.gameObject.SetActive(false);
			plusTextRight.gameObject.SetActive(false);
			plusTextLeft.gameObject.SetActive(false);
			
			if (Entry.SpawnsInBiomes.Count > 0) {
				biomeOrTilesetSlot.gameObject.SetActive(true);
				biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.BiomeIcon(Entry.SpawnsInBiomes.ToArray());
				plusTextRight.gameObject.SetActive(true);
			}
			if (Entry.SpawnsInTileset != null) {
				biomeOrTilesetSlot.gameObject.SetActive(true);
				biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.Tile(TileType.ground, Entry.SpawnsInTileset.Value);
				plusTextRight.gameObject.SetActive(true);
			}
			if (Entry.SpawnsInSeason != null) {
				if (Entry.SpawnsInBiomes.Count == 0) {
					biomeOrTilesetSlot.gameObject.SetActive(true);
					biomeOrTilesetSlot.DisplayedObject = new DisplayedObject.SeasonIcon(Entry.SpawnsInSeason.Value);
					plusTextRight.gameObject.SetActive(true);
				} else {
					seasonSlot.gameObject.SetActive(true);
					seasonSlot.DisplayedObject = new DisplayedObject.SeasonIcon(Entry.SpawnsInSeason.Value);
					plusTextLeft.gameObject.SetActive(true);
				}
			}
		}

		private void RenderMoreInfo() {
			if (Entry.SpawnsInBiomes.Count > 0) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_0_SpecificBiome",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Entity.Id, Entry.Entity.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});

				foreach (var biome in Entry.SpawnsInBiomes) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_9",
						formatFields = new[] {
							$"BiomeNames/{biome}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});
				}
			} else if (Entry.SpawnsInTileset != null) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_0_SpecificTile",
					formatFields = new[] {
						TileUtils.GetLocalizedDisplayName(TileType.ground, Entry.SpawnsInTileset.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_0_AnyBiome",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Entity.Id, Entry.Entity.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			if (Entry.NeedToBeInsideBiome) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_1",
					color = UserInterfaceUtils.DescriptionColor
				});	
			}

			if (Entry.SpawnsInSeason != null) {
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_2",
					formatFields = new[] {
						$"Seasons/{Entry.SpawnsInSeason}"
					},
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_3",
				color = UserInterfaceUtils.DescriptionColor
			});
			
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_4",
				formatFields = new[] {
					Entry.SpawnRadius.ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (Entry.DespawnRadius > 0f) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_5",
					formatFields = new[] {
						Entry.DespawnRadius.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
			if (Entry.SpawnLimit > 0) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_6",
					formatFields = new[] {
						Entry.SpawnLimit.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});	
			}
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_7",
				formatFields = new[] {
					Entry.SpawnCooldown.Min.ToString(LocalizationManager.CurrentCulture),
					Entry.SpawnCooldown.Max.ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (Entry.SpawnLimit > 0 && Entry.SpawnLimitReachedCooldown.Max > 0f) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/NaturalSpawnAroundObject_8",
					formatFields = new[] {
						Entry.SpawnLimitReachedCooldown.Min.ToString(LocalizationManager.CurrentCulture),
						Entry.SpawnLimitReachedCooldown.Max.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}