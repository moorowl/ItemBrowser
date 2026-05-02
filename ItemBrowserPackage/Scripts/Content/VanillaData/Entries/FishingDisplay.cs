using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class FishingDisplay : ObjectEntryDisplay<Fishing> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot rightSourceSlot;
		public ItemBrowserSlot leftSourceSlot;
		public PugText plusText;
		public PugText chanceText;
		public PugText catchTypeText;
		
		public override IEnumerable<Fishing> OnSort(IEnumerable<Fishing> entries) {
			return entries
				.OrderBy(entry => entry.Biome == Biome.None ? 1 : 0)
				.ThenBy(entry => (int) entry.Tileset * 1000 + (int) entry.Biome)
				.ThenBy(entry => (int) entry.Type)
				.ThenByDescending(entry => (int) (entry.Chance * 65535))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Result));
		}
		
		protected override void OnRender(Fishing entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			});
			
			if (entry.Biome != Biome.None) {
				// Biome + Normal water
				leftSourceSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				leftSourceSlot.Icon = new BiomeSlotIcon(entry.Biome);
				rightSourceSlot.Icon = new TileSlotIcon(TileType.water, Tileset.Dirt);
			} else {
				// Any biome + specific liquid
				leftSourceSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSourceSlot.Icon = new TileSlotIcon(TileType.water, entry.Tileset);
			}
			
			chanceText.Render(UserInterfaceUtility.FormatChance(entry.Chance) + "%");
			catchTypeText.Render($"ItemBrowser-CatchTypes/{entry.Type}");
		}

		protected override void OnRenderDescription(Fishing entry, EntryDescriptionButton description) {
			if (entry.Biome != Biome.None) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Fishing_0_Biome",
					formatFields = new[] {
						$"BiomeNames/{entry.Biome}"
					},
					color = UserInterfaceUtility.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Fishing_0_Liquid",
					formatFields = new[] {
						TileUtility.GetLocalizedDisplayName(TileType.water, entry.Tileset)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
			description.AddLine(new TextAndFormatFields {
				text = $"ItemBrowser-ObjectEntryDescriptions/Fishing_1_{entry.Type}",
				formatFields = new[] {
					UserInterfaceUtility.FormatChance(entry.Chance)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}