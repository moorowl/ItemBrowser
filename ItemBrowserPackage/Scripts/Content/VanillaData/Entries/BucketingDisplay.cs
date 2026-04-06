using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;
using PugTilemap;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class BucketingDisplay : ObjectEntryDisplay<Bucketing> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot liquidOrPitSlot;
		public ItemBrowserSlot bucketSlot;
		
		protected override void OnRender(Bucketing entry) {
			if (entry.IsEmptying) {
				bucketSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
					objectID = entry.FilledBucket.Id,
					variation = entry.FilledBucket.Variation
				});
				resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
					objectID = entry.EmptyBucket.Id,
					variation = entry.EmptyBucket.Variation
				});
				liquidOrPitSlot.Icon = new TileSlotIcon(TileType.pit);
			} else {
				resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
					objectID = entry.FilledBucket.Id,
					variation = entry.FilledBucket.Variation
				});
				bucketSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
					objectID = entry.EmptyBucket.Id,
					variation = entry.EmptyBucket.Variation
				});
				liquidOrPitSlot.Icon = new TileSlotIcon(TileType.water, entry.LiquidType);
			}
		}

		protected override void OnRenderDescription(Bucketing entry, EntryDescriptionButton description) {
			if (entry.IsEmptying) {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/Bucketing_0_Pit",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.FilledBucket.Id, entry.FilledBucket.Variation),
						TileUtility.GetLocalizedDisplayName(TileType.water, entry.LiquidType) ?? "???"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/Bucketing_0_Liquid",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.EmptyBucket.Id, entry.EmptyBucket.Variation),
						TileUtility.GetLocalizedDisplayName(TileType.water, entry.LiquidType) ?? "???"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
	}
}