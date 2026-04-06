using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SortButton : ItemBrowserButton {
		public GridView gridView;
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = gridView.CurrentSorter.Name,
					color = UserInterfaceUtility.DescriptionColor
				}
			};
		}
	}
}