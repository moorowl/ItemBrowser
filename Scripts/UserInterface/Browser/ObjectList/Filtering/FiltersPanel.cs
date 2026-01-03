using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class FiltersPanel : UIelement, IScrollable {
		[SerializeField]
		private SpriteRenderer background;
		[SerializeField]
		private UIScrollWindow scrollWindow;
		[SerializeField]
		private FilterHeader headerTemplate;
		[SerializeField]
		private FilterButton buttonTemplate;
		[SerializeField]
		private float headerPaddingTop = 1f;
		[SerializeField]
		private float headerPaddingBottom = 0.4375f;
		[SerializeField]
		private float headerHeight = 0.875f;
		[SerializeField]
		private float filterSpread = 0.625f;

		private readonly List<FilterButton> _filterButtons = new();
		private float _top;
		private float _left;
		
		public bool IsShowing {
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}
		public float WindowWidth => background.size.x;
		public bool HasBeenModified => _filterButtons.Any(button => button.CurrentState != button.Filter.DefaultState());
		public bool HasDynamicFiltersEnabled => _filterButtons.Any(button => button.Filter.FunctionIsDynamic && button.CurrentState != FilterState.None);
		public bool DisplayItemCraftingRequirements => _filterButtons.Any(button => button.Filter.CausesItemCraftingRequirementsToDisplay && button.CurrentState != FilterState.None);
		
		public IEnumerable<IEnumerable<Filter<ObjectDataCD>>> FiltersToInclude => _filterButtons
			.Where(filterButton => filterButton.CurrentState == FilterState.Include)
			.Select(filterButton => filterButton.Filter)
			.GroupBy(filter => filter.Group ?? filter.Name);
		public IEnumerable<IEnumerable<Filter<ObjectDataCD>>> FiltersToExclude => _filterButtons
			.Where(filterButton => filterButton.CurrentState == FilterState.Exclude)
			.Select(filterButton => filterButton.Filter)
			.GroupBy(filter => filter.Group ?? filter.Name);
		
		public void AddHeader(string term) {
			_left = 0f;
			_top -= headerPaddingTop;
			
			var header = Instantiate(headerTemplate, scrollWindow.scrollingContent);
			header.transform.localPosition = new Vector3(_left, _top, 0f);
			header.gameObject.SetActive(true);
			header.SetTerm(term);

			_left = 0f;
			_top -= headerPaddingBottom;
		}
		
		public void AddFilter(Filter<ObjectDataCD> filter) {
			if (_left >= 52f / 10f) {
				_left = 0f;
				_top -= filterSpread;
			}
			
			var button = Instantiate(buttonTemplate, scrollWindow.scrollingContent);
			button.transform.localPosition = new Vector3(_left, _top, 0f);
			button.gameObject.SetActive(true);
			button.SetFilter(filter);
			
			_filterButtons.Add(button);
			childElements.Add(button);

			_left += filterSpread;
		}

		public void ResetToDefaults() {
			foreach (var button in _filterButtons)
				button.ResetState();
		}

		public void Clear() {
			_top = headerHeight / 2f;
			_filterButtons.Clear();
			childElements.Clear();
			scrollWindow.ResetScroll();	
			
			for (var i = 0; i < scrollWindow.scrollingContent.childCount; i++)
				Destroy(scrollWindow.scrollingContent.GetChild(i).gameObject);
		}

		public void OnFilterStateChanged(Filter<ObjectDataCD> filter) {
			foreach (var button in _filterButtons) {
				var otherFilter = button.Filter;
				if (otherFilter == filter || otherFilter.Group != filter.Group || (filter.Group == null && otherFilter.Group == null))
					continue;

				button.CurrentState = FilterState.None;
			}
		}

		public void UpdateContainingElements(float scroll) { }

		public bool IsBottomElementSelected() {
			return false;
		}

		public bool IsTopElementSelected() {
			return false;
		}

		public float GetCurrentWindowHeight() {
			return Mathf.Abs(_top) + (filterSpread / 2f);
		}
	}
}