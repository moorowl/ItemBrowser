using System;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	[Flags]
	public enum FilterAndSorterScope {
		None = 0,
		Items = 1,
		Creatures = 2,
		Checklist = 4,
		Cooking = 8,
		All = Items | Creatures | Checklist | Cooking,
	}
}