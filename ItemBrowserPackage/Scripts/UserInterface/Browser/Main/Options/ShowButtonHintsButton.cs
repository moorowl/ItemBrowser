using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.UserInterface.Browser {
	public class ShowButtonHintsButton : ItemBrowserButton {
		protected override void LateUpdate() {
			IsToggled = Options.Instance.ShowButtonHints;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			Options.Instance.ShowButtonHints = !Options.Instance.ShowButtonHints;
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return base.GetHoverTitle();
			
			return new TextAndFormatFields {
				text = IsToggled ? "ItemBrowser-Options/ShowButtonHintsEnabled" : "ItemBrowser-Options/ShowButtonHintsDisabled"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return base.GetHoverDescription();
			
			return new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-Options/ShowButtonHintsDesc",
					color = UserInterfaceUtils.DescriptionColor
				}
			};
		}
	}
}