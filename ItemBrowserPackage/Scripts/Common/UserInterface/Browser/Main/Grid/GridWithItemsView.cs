using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class GridWithItemsView : GridView {
		protected override List<Sorter> GetSorters() {
			return ItemBrowserAPI.Registry.ItemSorters;
		}

		protected override List<(string Group, Filter Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.ItemFilters;
		}

		protected override List<ObjectDataCD> GetIncludedObjects() {
			return ObjectUtility.GetAllObjects().Where(ItemBrowserAPI.IsItemIndexed).ToList();
		}
	}
}