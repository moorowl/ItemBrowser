using System;
using System.Collections.Generic;

namespace ItemBrowser.Utilities.DataStructures.SortingAndFiltering {
	public class Filter<T> {
		public delegate bool FilterDelegate(T item);

		public readonly string Name;
		public readonly string Description;

		public ObjectID Icon { get; set; }
		public string Symbol { get; set; } = "?";
		public string[] NameFormatFields { get; set; }
		public bool LocalizeNameFormatFields { get; set; } = true;
		public string[] DescriptionFormatFields { get; set; }
		public bool LocalizeDescriptionFormatFields { get; set; } = true;
		public string Group { get; set; }
		public Func<FilterState> DefaultState { get; set; } = () => FilterState.None;
		public FilterDelegate Function { get; set; }
		// Causes the item list to constantly refresh. Should be used if the function checks something that can change during gameplay
		public bool FunctionIsDynamic { get; set; }
		public bool CausesItemCraftingRequirementsToDisplay { get; set; }
		
		private readonly HashSet<T> _indexedMatches = new();

		public Filter(string name, string description = null) {
			Name = name;
			Description = description ?? $"{name}Desc";
		}

		public void SetupIndexedMatches(IEnumerable<T> items) {
			if (FunctionIsDynamic)
				return;
			
			_indexedMatches.Clear();

			foreach (var item in items) {
				if (Function(item))
					_indexedMatches.Add(item);
			}
		}
		
		public bool Matches(T item) {
			return FunctionIsDynamic ? Function(item) : _indexedMatches.Contains(item);
		}
	}
}