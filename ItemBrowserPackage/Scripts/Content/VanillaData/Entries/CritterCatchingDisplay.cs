using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CritterCatchingDisplay : ObjectEntryDisplay<CritterCatching> {
		public ItemBrowserSlot critterSlot;
		public ItemBrowserSlot biomeOrTilesetSlot;
		public ItemBrowserSlot critterCatcherSlot;

		public override IEnumerable<CritterCatching> OnSort(IEnumerable<CritterCatching> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Critter.Id, entry.Critter.Variation));
		}
		
		protected override void OnRender(CritterCatching entry) {
			critterSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Critter.Id,
				variation = entry.Critter.Variation
			});
			critterCatcherSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.CritterCatcher.Id,
				variation = entry.CritterCatcher.Variation
			});

			if (entry.SpawnsOnTileset != null) {
				biomeOrTilesetSlot.Icon = new TileSlotIcon(TileType.ground, entry.SpawnsOnTileset.Value);
			} else if (entry.SpawnsInBiomes.Count > 0) {
				biomeOrTilesetSlot.Icon = new BiomeSlotIcon(entry.SpawnsInBiomes.ToArray());
			} else {
				biomeOrTilesetSlot.Icon = new BasicSlotIcon(new ObjectDataCD());
			}
		}

		protected override void OnRenderDescription(CritterCatching entry, EntryDescriptionButton description) {
			if (entry.SpawnsOnTileset != null) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/CritterCatching_0_SpecificTileset",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.CritterCatcher.Id, entry.CritterCatcher.Variation),
						TileUtility.GetLocalizedDisplayName(TileType.ground, entry.SpawnsOnTileset.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			} else if (entry.SpawnsInBiomes.Count > 0) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/CritterCatching_0_SpecificBiome",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.CritterCatcher.Id, entry.CritterCatcher.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});

				foreach (var biome in entry.SpawnsInBiomes) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/CritterCatching_1",
						formatFields = new[] {
							$"BiomeNames/{biome}"
						},
						color = UserInterfaceUtility.DescriptionColor
					});
				}
			}

			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/CritterCatching_2",
				formatFields = new[] {
					(entry.TimeToCatch.Min / 60f).ToString(LocalizationManager.CurrentCulture),
					(entry.TimeToCatch.Max / 60f).ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}