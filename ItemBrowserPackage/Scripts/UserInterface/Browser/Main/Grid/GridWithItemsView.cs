using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;

namespace ItemBrowser.UserInterface.Browser {
	public class GridWithItemsView : GridView {
		protected override List<Sorter<ObjectDataCD>> GetSorters() {
			return ItemBrowserAPI.Registry.ItemSorters;
		}

		protected override List<(string Group, Filter<ObjectDataCD> Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.ItemFilters;
		}

		protected override List<ObjectDataCD> GetIncludedObjects() {
			return ObjectUtils.GetAllObjects().Where(ItemBrowserAPI.IsItemIndexed).ToList();
		}
	}
}