using System;
using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SwapCategoryButton : ItemBrowserButton {
		public EntriesView entriesView;
		public SpriteRenderer[] icons;
		public ButtonStyle style;

		private int _categoryIndex;
		private int _entriesInCategory;
		private ObjectEntryCategory _category;

		public enum ButtonStyle {
			Top,
			CycleLeft,
			CycleRight
		}

		public void SetCategory(int categoryIndex, int entriesInCategory, ObjectEntryCategory category) {
			_categoryIndex = categoryIndex;
			_entriesInCategory = entriesInCategory;
			_category = category;

			var objectInfo = PugDatabase.GetObjectInfo(category.Icon);
			foreach (var icon in icons)
				icon.sprite = objectInfo.smallIcon ?? objectInfo.icon;
			
			LateUpdate();
		}

		protected override void LateUpdate() {
			IsToggled = entriesView.SelectedCategory == _categoryIndex;
			
			base.LateUpdate();
			
			if (!UserInterfaceUtility.IsUsingMouseAndKeyboard)
				TryShowButtonHint(style == ButtonStyle.CycleRight ? ButtonHint.CycleTabRight : ButtonHint.CycleTabLeft);
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (IsToggled || !canBeClicked)
				return;
			
			UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.ChangeTabOrCategory, this);
			entriesView.SetCategory(_categoryIndex);
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return null;

			return new TextAndFormatFields {
				text = style switch {
					ButtonStyle.Top => _category.GetTitle(entriesView.detailsView.IsSelectedObjectNonObtainable),
					ButtonStyle.CycleLeft or ButtonStyle.CycleRight => optionalTitle.mTerm,
					_ => throw new ArgumentOutOfRangeException()
				}
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return null;

			if (style == ButtonStyle.Top) {
				return new List<TextAndFormatFields> {
					new() {
						text = _entriesInCategory == 1 ? $"ItemBrowser-EntriesAmount/{entriesView.entryType}" : $"ItemBrowser-EntriesAmount/{entriesView.entryType}_Plural",
						formatFields = new[] {
							_entriesInCategory.ToString()
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtility.DescriptionColor
					}
				};
			}
			
			if (style is ButtonStyle.CycleLeft or ButtonStyle.CycleRight) {
				var lines = new List<TextAndFormatFields> {
					new() {
						text = _category.GetTitle(entriesView.detailsView.IsSelectedObjectNonObtainable),
						color = UserInterfaceUtility.DescriptionColor
					}
				};

				return lines;
			}
			
			return base.GetHoverDescription();
		}
	}
}