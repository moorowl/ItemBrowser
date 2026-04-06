using System.Collections.Generic;
using System.Linq;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class Sorter {
		public delegate IOrderedEnumerable<ObjectDataCD> SorterDelegate(IEnumerable<ObjectDataCD> items);

		public readonly string Name;
		public bool Localize { get; set; } = true;
		public SorterDelegate Function { get; set; }

		public Sorter(string name) {
			Name = name;
		}
	}
}