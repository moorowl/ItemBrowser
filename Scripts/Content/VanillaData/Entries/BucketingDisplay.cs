using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class BucketingDisplay : ObjectEntryDisplay<Bucketing> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot liquidOrPitSlot;
		public ItemBrowserSlot bucketSlot;
		
		protected override void OnRender(Bucketing entry) {
			if (entry.IsEmptying) {
				bucketSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.FilledBucket.Id,
					variation = entry.FilledBucket.Variation
				});
				resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.EmptyBucket.Id,
					variation = entry.EmptyBucket.Variation
				});
				liquidOrPitSlot.DisplayedObject = new DisplayedObject.Tile(TileType.pit);
			} else {
				resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.FilledBucket.Id,
					variation = entry.FilledBucket.Variation
				});
				bucketSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.EmptyBucket.Id,
					variation = entry.EmptyBucket.Variation
				});
				liquidOrPitSlot.DisplayedObject = new DisplayedObject.Tile(TileType.water, entry.LiquidType);
			}
		}

		protected override void OnRenderDescription(Bucketing entry, EntryDescriptionButton description) {
			if (entry.IsEmptying) {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/Bucketing_0_Pit",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.FilledBucket.Id, entry.FilledBucket.Variation),
						TileUtils.GetLocalizedDisplayName(TileType.water, entry.LiquidType) ?? "???"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/Bucketing_0_Liquid",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.EmptyBucket.Id, entry.EmptyBucket.Variation),
						TileUtils.GetLocalizedDisplayName(TileType.water, entry.LiquidType) ?? "???"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}