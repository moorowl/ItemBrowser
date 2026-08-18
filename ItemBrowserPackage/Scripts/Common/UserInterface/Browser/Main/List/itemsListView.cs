using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.SortingAndFiltering;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ItemsListView : ObjectListView {
		public override List<Sorter> GetSorters() {
			return ItemBrowserAPI.Registry.ItemSorters;
		}

		public override List<(string Group, Filter Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.ItemFilters;
		}

		public override List<ObjectDataCD> GetIncludedObjects() {
			return ItemBrowserAPI.Registry.Items.ToList();
		}
	}
}