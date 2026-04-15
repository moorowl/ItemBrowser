using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class SorterResults {
		private readonly Dictionary<ObjectDataCD, int> _scores;
		
		private SorterResults(Dictionary<ObjectDataCD, int> scores) {
			_scores = scores;
		}

		public int GetScore(ObjectDataCD objectData) {
			return _scores.GetValueOrDefault(objectData, -1);
		}

		public static SorterResults Create(Sorter sorter) {
			var scores = new Dictionary<ObjectDataCD, int>();
			
			foreach (var objectData in sorter.Function(ObjectUtility.GetAllObjects()))
				scores.TryAdd(objectData, scores.Count);

			return new SorterResults(scores);
		}
	}
}