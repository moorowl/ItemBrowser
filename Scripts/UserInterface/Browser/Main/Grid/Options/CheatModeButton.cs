using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.UserInterface.Browser {
	public class CheatModeButton : ItemBrowserButton {
		public static bool CanBeToggled => Manager.saves.IsCreativeModeCharacter() || Manager.main.player.adminPrivileges >= 1;
		
		protected override void LateUpdate() {
			canBeClicked = CanBeToggled;
			IsToggled = Options.Instance.CheatMode && canBeClicked;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!canBeClicked)
				return;
			
			Options.Instance.CheatMode = !Options.Instance.CheatMode;
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return base.GetHoverTitle();

			return new TextAndFormatFields {
				text = IsToggled ? "ItemBrowser-Options/CheatModeEnabled" : "ItemBrowser-Options/CheatModeDisabled"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return base.GetHoverDescription();
			
			return new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-Options/CheatModeDesc",
					color = UserInterfaceUtils.DescriptionColor
				}
			};
		}
	}
}