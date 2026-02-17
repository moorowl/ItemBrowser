using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.UserInterface.Browser {
	public class ShowSourceModButton : ItemBrowserButton {
		protected override void LateUpdate() {
			IsToggled = Options.Instance.ShowSourceMod;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			Options.Instance.ShowSourceMod = !Options.Instance.ShowSourceMod;
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return base.GetHoverTitle();
			
			return new TextAndFormatFields {
				text = IsToggled ? "ItemBrowser-Options/ShowSourceModEnabled" : "ItemBrowser-Options/ShowSourceModDisabled"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return base.GetHoverDescription();
			
			return new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-Options/ShowSourceModDesc",
					color = UserInterfaceUtils.DescriptionColor
				}
			};
		}
	}
}