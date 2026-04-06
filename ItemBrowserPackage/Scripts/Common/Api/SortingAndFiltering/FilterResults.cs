using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class FilterResults {
		private readonly HashSet<ObjectDataCD> _matches;
		private readonly Filter _filter;

		private FilterResults(HashSet<ObjectDataCD> matches) {
			_matches = matches;
		}
		
		private FilterResults(Filter filter) {
			_filter = filter;
		}

		public bool Matches(ObjectDataCD objectData) {
			return _matches?.Contains(objectData) ?? _filter.Function(objectData);
		}
		
		public static FilterResults Create(Filter filter) {
			if (filter.FunctionIsDynamic)
				return new FilterResults(filter);
			
			var matches = new HashSet<ObjectDataCD>();
			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				if (filter.Function(objectData))
					matches.Add(objectData);
			}

			return new FilterResults(matches);
		}
	}
}