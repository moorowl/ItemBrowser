using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;

namespace ItemBrowser.UserInterface.Browser {
	public class GridWithCreaturesView : GridView {
		protected override List<Sorter<ObjectDataCD>> GetSorters() {
			return ItemBrowserAPI.Registry.CreatureSorters;
		}

		protected override List<(string Group, Filter<ObjectDataCD> Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.CreatureFilters;
		}

		protected override List<ObjectDataCD> GetIncludedObjects() {
			return ObjectUtils.GetAllObjects().Where(ItemBrowserAPI.IsCreatureIndexed).ToList();
		}
	}
}