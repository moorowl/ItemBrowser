using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class VirtualObjectListGroupItem : ItemBrowserSlot {
		public Sprite expandIcon;
		public Sprite collapseIcon;
		
		private string _group;
		
		public void SetGroup(string group, VirtualObjectList craftingSelectorUI) {
			_group = group;
			slotsUIContainer = craftingSelectorUI;
		}
	}
}