using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CreatureSummoningDisplay : ObjectEntryDisplay<CreatureSummoning> {
		public ItemBrowserSlot creatureSlot;
		public ItemBrowserSlot rightSlot;
		public ItemBrowserSlot leftSlot;
		public PugText plusText;
		
		protected override void OnRender(CreatureSummoning entry) {
			creatureSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Creature.Id,
				variation = entry.Creature.Variation
			});
			
			var hasSummoningArea = entry.SummoningArea.Id != ObjectID.None;
			if (hasSummoningArea) {
				leftSlot.gameObject.SetActive(true);
				plusText.gameObject.SetActive(true);
				
				rightSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.SummoningArea.Id,
					variation = entry.SummoningArea.Variation
				});
				leftSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.SummoningItem.Id,
					variation = entry.SummoningItem.Variation
				});
			} else {
				leftSlot.gameObject.SetActive(false);
				plusText.gameObject.SetActive(false);
				
				rightSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
					objectID = entry.SummoningItem.Id,
					variation = entry.SummoningItem.Variation
				});
			}
		}

		protected override void OnRenderDescription(CreatureSummoning entry, EntryDescriptionButton description) {
			if (entry.SummoningArea.Id != ObjectID.None) {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/CreatureSummoning_0_{entry.SummoningMethod}",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.SummoningItem.Id, entry.SummoningItem.Variation),
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.SummoningArea.Id, entry.SummoningArea.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser-ObjectEntryDescriptions/CreatureSummoning_0_{entry.SummoningMethod}",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.SummoningItem.Id, entry.SummoningItem.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}