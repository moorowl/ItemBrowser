using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class FilterResults {
		private readonly HashSet<ObjectDataCD> _matches;

		private FilterResults(HashSet<ObjectDataCD> matches) {
			_matches = matches;
		}

		public bool Matches(ObjectDataCD objectData) {
			return _matches.Contains(objectData);
		}
		
		public static FilterResults Create(Filter filter) {
			var matches = new HashSet<ObjectDataCD>(128);
			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				if (filter.Function(objectData))
					matches.Add(objectData);
			}

			return new FilterResults(matches);
		}

		public static bool Equals(FilterResults a, FilterResults b) {
			if (a == null || b == null || a._matches.Count != b._matches.Count)
				return false;

			return a._matches.SequenceEqual(b._matches);
		}
	}
}