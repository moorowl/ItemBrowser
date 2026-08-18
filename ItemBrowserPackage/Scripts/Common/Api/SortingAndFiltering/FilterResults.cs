using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class FilterResults {
		public HashSet<ObjectDataCD> Results;

		private FilterResults(HashSet<ObjectDataCD> matches) {
			Results = matches;
		}

		public bool Matches(ObjectDataCD objectData) {
			if (objectData.variation > 0 && !ObjectUtility.IsPrimaryVariation(objectData))
				return Results.Contains(objectData) || Results.Contains(new ObjectDataCD { objectID = objectData.objectID });
			
			return Results.Contains(objectData);
		}
		
		public static FilterResults Create(Filter filter, List<ObjectDataCD> objectsToFilter) {
			var matches = new HashSet<ObjectDataCD>(objectsToFilter.Count);
			foreach (var objectData in objectsToFilter) {
				if (filter.Function(objectData))
					matches.Add(objectData);
			}

			return new FilterResults(matches);
		}

		public static bool Equals(FilterResults a, FilterResults b) {
			if (a == null || b == null || a.Results.Count != b.Results.Count)
				return false;

			return a.Results.SequenceEqual(b.Results);
		}
	}
}