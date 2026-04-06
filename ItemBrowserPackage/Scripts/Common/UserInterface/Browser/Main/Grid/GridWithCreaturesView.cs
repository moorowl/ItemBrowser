using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class GridWithCreaturesView : GridView {
		protected override List<Sorter> GetSorters() {
			return ItemBrowserAPI.Registry.CreatureSorters;
		}

		protected override List<(string Group, Filter Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.CreatureFilters;
		}

		protected override List<ObjectDataCD> GetIncludedObjects() {
			return ObjectUtility.GetAllObjects().Where(ItemBrowserAPI.IsCreatureIndexed).ToList();
		}
	}
}