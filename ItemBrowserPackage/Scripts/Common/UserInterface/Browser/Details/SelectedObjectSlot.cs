using System.Collections;
using System.Collections.Generic;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SelectedObjectSlot : ItemBrowserSlot {
		public ItemBrowserUI itemBrowserUI;

		public void SetObjectData(ObjectDataCD objectData) {
			Icon = new BasicSlotIcon(objectData);

			StartCoroutine(PlayBumpAnimationNextFrame());
		}
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			itemBrowserUI.GoBack();
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = base.GetHoverDescription() ?? new List<TextAndFormatFields>();
			UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/GoBack", "UIInteract");

			return lines;
		}

		private IEnumerator PlayBumpAnimationNextFrame() {
			yield return null;
			
			PlayBumpAnimation();
		}
	}
}