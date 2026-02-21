using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.UserInterface.Browser {
	public class ClearFavoritesButton : ItemBrowserButton {
		protected override void LateUpdate() {
			canBeClicked = Options.Instance.FavoritesCount > 0;

			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!canBeClicked)
				return;
			
			Options.Instance.RemoveAllFavorites();
			
			foreach (var itemSlot in API.Rendering.UICamera.transform.GetComponentsInChildren<ItemBrowserSlot>(true))
				itemSlot.OnFavoritedStateChanged();
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return base.GetHoverTitle();
			
			return new TextAndFormatFields {
				text = "ItemBrowser-Options/ClearFavorites"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return base.GetHoverDescription();
			
			return new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-Options/ClearFavoritesDesc",
					formatFields = new[] {
						Options.Instance.FavoritesCount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				}
			};
		}
	}
}