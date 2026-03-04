using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.UserInterface.Browser {
	public class ThemeButton : ItemBrowserButton {
		public ItemBrowserUI itemBrowserUI;

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			itemBrowserUI.SwapToNextTheme();
		}
		
		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);
			
			itemBrowserUI.SwapToPreviousTheme();
			TryPlayClickSound();
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = "ItemBrowser-Options/Theme",
				formatFields = new[] {
					API.Localization.GetLocalizedTerm(itemBrowserUI.CurrentTheme.Term) ?? itemBrowserUI.CurrentTheme.Term
				},
				dontLocalizeFormatFields = true
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return base.GetHoverDescription();
			
			return new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-Options/ThemeDesc",
					color = UserInterfaceUtils.DescriptionColor
				}
			};
		}
	}
}