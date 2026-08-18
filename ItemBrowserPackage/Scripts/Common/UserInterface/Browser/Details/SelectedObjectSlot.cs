using ItemBrowser.Common.UserInterface.SlotIcons;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SelectedObjectSlot : ItemBrowserSlot {
		public ItemBrowserUI itemBrowserUI;

		public void SetObjectData(ObjectDataCD objectData) {
			Icon = new BasicSlotIcon(objectData);
		}
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			itemBrowserUI.GoBack();
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			TryShowButtonHint(ButtonHint.GoBack);
		}
	}
}