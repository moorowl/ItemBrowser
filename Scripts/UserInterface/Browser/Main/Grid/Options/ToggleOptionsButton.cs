using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ToggleOptionsButton : ItemBrowserButton {
		public OptionsPanel optionsPanel;
		public Sprite ascendingSprite;
		public Sprite descendingSprite;
		public List<SpriteRenderer> spritesToUpdate;
		
		private bool _previousState;
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (optionsPanel.IsToggled == _previousState)
				return;
			
			var newSprite = optionsPanel.IsToggled ? descendingSprite : ascendingSprite;
			foreach (var sr in spritesToUpdate)
				sr.sprite = newSprite;
				
			_previousState = optionsPanel.IsToggled;
		}

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = optionsPanel.IsToggled ? "ItemBrowser-General/HideOptions" : "ItemBrowser-General/ShowOptions"
			};
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			optionsPanel.IsToggled = !optionsPanel.IsToggled;
		}
	}
}