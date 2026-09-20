using System;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public record Group {
		public delegate int GroupDelegate(ObjectDataCD objectData);

		public readonly string Name;
		public readonly string Description;

		public string[] NameFormatFields { get; set; }
		public bool LocalizeNameFormatFields { get; set; } = true;
		public string[] DescriptionFormatFields { get; set; }
		public bool LocalizeDescriptionFormatFields { get; set; } = true;
		
		public string[] Names { get; set; } = Array.Empty<string>();
		public GroupDelegate Function { get; set; } = _ => 0;
		public FilterAndSorterScope Scope { get; set; } = FilterAndSorterScope.All;
		
		public Group(string name, string description = null) {
			Name = name;
			Description = description ?? $"{name}Desc";
		}
	}
}