using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.UserInterface.Browser {
	public class SelectedObjectSlot : ItemBrowserSlot {
		public ItemBrowserUI itemBrowserUI;

		public void SetObjectData(ObjectDataCD objectData) {
			DisplayedObject = new DisplayedObject.Basic(objectData);
		}
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			itemBrowserUI.GoBack();
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = base.GetHoverDescription() ?? new List<TextAndFormatFields>();
			UserInterfaceUtils.AppendButtonHint(lines, "ItemBrowser-ButtonHints/GoBack", "UIInteract");

			return lines;
		}
	}
}