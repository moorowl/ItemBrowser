using System;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class BackgroundPerksDisplay : ObjectEntryDisplay<BackgroundPerks> {
		private static readonly Lazy<SkillIconsTable> SkillIconsTable = new(() => Resources.Load<SkillIconsTable>("SkillIconsTable"));
		
		public ItemBrowserButton background;
		public SpriteRenderer backgroundIcon;
		public ItemBrowserSlot resultSlot;
		
		protected override void OnRender(BackgroundPerks entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation,
				amount = entry.Result.Amount
			});
			background.optionalTitle.mTerm = $"Roles/{entry.Background}";
			backgroundIcon.sprite = SkillIconsTable.Value.GetIcon(entry.BackgroundSkill).icon;
		}

		protected override void OnRenderDescription(BackgroundPerks entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/BackgroundPerks_0",
				formatFields = new[] {
					background.optionalTitle.mTerm
				},
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/BackgroundPerks_1",
				formatFields = new[] {
					entry.Result.Amount.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
		}
	}
}