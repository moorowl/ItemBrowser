using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class DropsWhenDamagedDisplay : ObjectEntryDisplay<DropsWhenDamaged> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot entitySlot;
		public PugText damageToDropText;
		
		protected override void OnRender(DropsWhenDamaged entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			});
			entitySlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Entity.Id,
				variation = entry.Entity.Variation
			});
			damageToDropText.Render(string.Format(API.Localization.GetLocalizedTerm("ItemBrowser-General/AmountPerDamage"), entry.DamageRequiredToDrop));
		}

		protected override void OnRenderDescription(DropsWhenDamaged entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/DropsWhenDamaged_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/DropsWhenDamaged_1",
				formatFields = new[] {
					entry.DamageRequiredToDrop.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			if (entry.HealthRequiredToDrop > 0) {
				var maxHealth = PugDatabase.GetComponent<HealthCD>(entry.Entity.Id, entry.Entity.Variation).maxHealth;
				
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/DropsWhenDamaged_2",
					formatFields = new[] {
						entry.HealthRequiredToDrop.ToString(),
						maxHealth.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}