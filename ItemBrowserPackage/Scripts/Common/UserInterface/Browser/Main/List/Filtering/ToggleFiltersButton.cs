using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ToggleFiltersButton : ItemBrowserButton {
		public ObjectListView objectListView;
		public FiltersPanel filtersPanel;

		protected override void LateUpdate() {
			base.LateUpdate();
			
			if (filtersPanel.HasBeenModified)
				TryShowButtonHint(ButtonHint.RestoreDefaults);
		}

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
						objectListView.IncludedObjects.ToString(),
						(objectListView.IncludedObjects + objectListView.ExcludedObjects).ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				},
				new() {
					text = "ItemBrowser-General/FilteredResultsDesc",
					formatFields = new[] {
						objectListView.ExcludedObjects.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				}
			};

			return lines;
		}
	}
}