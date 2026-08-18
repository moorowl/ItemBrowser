using System.Collections.Generic;
using System.Linq;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public record Sorter {
		public delegate IOrderedEnumerable<ObjectDataCD> SorterDelegate(IEnumerable<ObjectDataCD> items);
		public delegate string AdditionalInfoDelegate(ObjectDataCD item);

		public readonly string Name;
		public bool Localize { get; set; } = true;
		public SorterDelegate Function { get; set; }
		public AdditionalInfoDelegate AdditionalInfoFunction { get; set; }
		public FilterAndSorterScope Scope { get; set; } = FilterAndSorterScope.All;

		public Sorter(string name) {
			Name = name;
		}
	}
}