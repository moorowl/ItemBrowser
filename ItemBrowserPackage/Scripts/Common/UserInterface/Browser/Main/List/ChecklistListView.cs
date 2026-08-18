using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;

namespace ItemBrowser.Common.UserInterface.Browser { 
	public class ChecklistListView : ObjectListView {
		public ChecklistProgressBar progressBar;
		
		public override List<Sorter> GetSorters() {
			return ItemBrowserAPI.Registry.ChecklistSorters;
		}

		public override List<(string Group, Filter Filter)> GetFilters() {
			return ItemBrowserAPI.Registry.ChecklistFilters;
		}

		public override List<ObjectDataCD> GetIncludedObjects() {
			return ItemBrowserAPI.Registry.ChecklistObjects.ToList();
		}
		
		protected override void OnShow(bool isFirstTimeShowing) {
			base.OnShow(isFirstTimeShowing);
			
			OptionsManager.Instance.OnTagChanged += UpdateProgressBarIfTagsChanged;
			
			UpdateProgressBar();
		}

		protected override void OnHide() {
			base.OnHide();
			
			OptionsManager.Instance.OnTagChanged -= UpdateProgressBarIfTagsChanged;
		}

		private void UpdateProgressBarIfTagsChanged(ObjectDataCD objectData, ObjectTagType tag, bool isAdded) {
			if (tag == ObjectTagType.Collected)
				UpdateProgressBar();
		}

		private void UpdateProgressBar() {
			progressBar.UpdateProgress();
		}
	}
}