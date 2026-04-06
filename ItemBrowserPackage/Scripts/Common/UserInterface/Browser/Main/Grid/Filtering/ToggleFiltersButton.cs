using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ToggleFiltersButton : ItemBrowserButton {
		public GridView gridView;
		public FiltersPanel filtersPanel;

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = filtersPanel.IsShowing ? "ItemBrowser-General/HideFilters" : "ItemBrowser-General/ShowFilters"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-General/FilteredResults",
					formatFields = new[] {
						gridView.IncludedObjects.ToString(),
						(gridView.IncludedObjects + gridView.ExcludedObjects).ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				}
			};
			if (filtersPanel.HasBeenModified)
				UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/RestoreDefaults", "UISecondInteract");
			
			return lines;
		}
	}
}