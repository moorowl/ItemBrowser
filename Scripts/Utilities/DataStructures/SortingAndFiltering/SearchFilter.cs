using System.Collections.Generic;

namespace ItemBrowser.Utilities.DataStructures.SortingAndFiltering {
	public class SearchFilter<T> {
		private static readonly List<string> CachedNamesList = new(4);

		public delegate void NameProviderDelegate(T item, List<string> names);

		private string _processedTerm;
		private string _term;
		public string Term {
			get => _term;
			set {
				_term = value ?? "";
				_processedTerm = ProcessString(_term);
				if (string.IsNullOrWhiteSpace(_processedTerm))
					_processedTerm = null;
			}
		}

		public readonly Filter<T>.FilterDelegate Function;

		public SearchFilter(NameProviderDelegate nameProvider) {
			Function = item => {
				if (_processedTerm == null)
					return true;
				
				CachedNamesList.Clear();
				nameProvider(item, CachedNamesList);

				foreach (var name in CachedNamesList) {
					if (ProcessString(name).Contains(_processedTerm)) {
						return true;
					}
				}

				return false;
			};
		}
		
		private static string ProcessString(string input) {
			return input.Trim().ToLower().Replace("\'", "").Replace(" ", "").Replace("-", "").Replace(".", "");
		}
	}
}