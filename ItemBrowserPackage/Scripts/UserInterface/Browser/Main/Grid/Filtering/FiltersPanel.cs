using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class FiltersPanel : UIelement, IScrollable {
		public SpriteRenderer background;
		public UIScrollWindow scrollWindow;
		public Transform container;
		public FilterHeader headerTemplate;
		public FilterButton buttonTemplate;
		public GameObject dividerTemplate;
		public float dividerPadding = 2f / 16f;
		public float dividerSideStart = -2.3125f;

		private readonly List<FilterButton> _filterButtons = new();
		private float _height;

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

		public void AddFilterGroup(string term, List<Filter<ObjectDataCD>> filters) {
			if (filters.Count == 0)
				return;
			
			AddHeader(term);
			AddFilters(filters);
		}
		
		private void AddHeader(string term) {
			var header = Instantiate(headerTemplate, container);
			header.gameObject.SetActive(true);
			header.SetTerm(term);
			var headerHeight = UserInterfaceUtils.CalculateHeight(header);
			
			_height -= headerHeight / 2f;
			header.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= headerHeight / 2f;

			AddDivider();
		}
		
		private void AddDivider() {
			var divider = Instantiate(dividerTemplate, container);
			divider.gameObject.SetActive(true);
			var dividerHeight = UserInterfaceUtils.CalculateHeight(divider);
			
			_height -= dividerHeight / 2f;
			divider.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= dividerHeight / 2f;
		}
		
		private void AddFilters(List<Filter<ObjectDataCD>> filters) {
			for (var i = 0; i < filters.Count; i++) {
				var filter = filters[i];

				var filterButton = Instantiate(buttonTemplate, container);
				filterButton.SetFilter(filter);
				filterButton.gameObject.SetActive(true);
				var filterButtonSize = UserInterfaceUtils.CalculateHeight(filterButton);

				var maxFilterButtonsPerRow = (int) (scrollWindow.windowWidth / filterButtonSize);
				var filterButtonIndexInColumn = i % maxFilterButtonsPerRow;

				if (filterButtonIndexInColumn == 0)
					_height -= filterButtonSize / 2f;
				
				filterButton.transform.localPosition = new Vector3(dividerSideStart + ((filterButtonSize + dividerPadding) * filterButtonIndexInColumn), _height, 0f);
				
				if (i == filters.Count - 1 || filterButtonIndexInColumn == maxFilterButtonsPerRow - 1)
					_height -= (filterButtonSize / 2f) + dividerPadding;
				
				_filterButtons.Add(filterButton);
			}
		}

		public void ResetToDefaults() {
			foreach (var button in _filterButtons)
				button.ResetState();
		}

		public void Clear() {
			_height = 0f;
			_filterButtons.Clear();
			scrollWindow.ResetScroll();	
			
			for (var i = 0; i < container.childCount; i++)
				Destroy(container.GetChild(i).gameObject);
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
			return Mathf.Abs(_height) - dividerPadding;
		}
	}
}