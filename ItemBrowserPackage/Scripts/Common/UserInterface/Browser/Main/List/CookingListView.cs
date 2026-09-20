using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.SortingAndFiltering;

namespace ItemBrowser.Common.UserInterface.Browser { 
	public class CookingListView : ObjectListView {
		public override List<Sorter> GetSorters() {
			return ItemBrowserAPI.Registry.CookingSorters;
		}

		public override List<(string Group, Filter Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.CookingFilters;
		}

		public override HashSet<ObjectDataCD> GetIncludedObjects() {
			return ItemBrowserAPI.Registry.CookingObjects;
		}
	}
}