using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class DropsWhenDamagedDisplay : ObjectEntryDisplay<DropsWhenDamaged> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot entitySlot;
		[SerializeField]
		private PugText damageToDropText;

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
			damageToDropText.Render(string.Format(API.Localization.GetLocalizedTerm("ItemBrowser:AmountPerDamage"), Entry.DamageRequiredToDrop));
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/DropsWhenDamaged_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Entity.Id, Entry.Entity.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/DropsWhenDamaged_1",
				formatFields = new[] {
					Entry.DamageRequiredToDrop.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			if (Entry.HealthRequiredToDrop > 0) {
				var maxHealth = PugDatabase.GetComponent<HealthCD>(Entry.Entity.Id, Entry.Entity.Variation).maxHealth;
				
				MoreInfo.AddPadding();
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/DropsWhenDamaged_2",
					formatFields = new[] {
						Entry.HealthRequiredToDrop.ToString(),
						maxHealth.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}