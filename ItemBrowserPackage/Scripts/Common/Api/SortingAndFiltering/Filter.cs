using System;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public record Filter {
		public delegate bool FilterDelegate(ObjectDataCD item);

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
		public FilterAndSorterScope Scope { get; set; } = FilterAndSorterScope.All;
		
		public Filter(string name, string description = null) {
			Name = name;
			Description = description ?? $"{name}Desc";
		}
	}
}