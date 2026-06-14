using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine.Serialization;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SortButton : ItemBrowserButton {
		public ObjectListView objectListView;
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = objectListView.CurrentSorter.Name,
					color = UserInterfaceUtility.DescriptionColor
				}
			};
		}
	}
}