using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public abstract class GridView : MainSubView {
		private const float DynamicFiltersRefreshInterval = 1f;
		
		public ObjectGrid objectList;
		public SearchBar searchInput;
		public SearchBar[] otherSearchInputsToSync;
		public SpriteMask searchInputMask;
		public FiltersPanel[] filtersPanels;
		
		private bool _requestedListRefresh;
		private bool _requestedListRefreshPreservesScroll;
		private float _lastListRefreshTime;
		private Task<(bool PreserveScroll, List<ObjectDataCD> Objects)> _listRefreshTask;

		private List<ObjectDataCD> _filteredObjects = new();
		public IEnumerable<ObjectDataCD> FilteredObjects => _filteredObjects;
		public int IncludedObjects { get; private set; }
		public int ExcludedObjects { get; private set; }
		
		private FiltersPanel PrimaryFiltersPanel => filtersPanels[0];
		private readonly Dictionary<Filter, FilterResults> _filterResults = new();
		
		private string _lastSearchTerm = string.Empty;
		private SearchResults _searchResults;

		private List<Sorter> _sorters;
		private readonly List<SorterResults> _sorterResults = new();
		private int _currentSorterIndex;
		public bool UseReverseSorting { get; set; }
		public Sorter CurrentSorter => _sorters[_currentSorterIndex];
		
		protected override void OnShow(bool isFirstTimeShowing) {
			objectList.ShowContainerUI();
			
			if (isFirstTimeShowing) {
				SetupFiltersAndSorting();
				RequestListRefresh(false);
			} else {
				RequestListRefresh(true);
			}
			
			objectList.ShowContainerUI();

			UpdateListRefresh();
			AdjustWindowPosition();
		}

		protected override void OnHide() {
			objectList.HideContainerUI();
		}
		
		private void SetupFiltersAndSorting() {
			_searchResults = SearchResults.Create("");
			
			// Setup filters
			PrimaryFiltersPanel.Clear();

			var filterGroups = GetFilters().GroupBy(x => x.Group)
				.ToDictionary(group => group.Key, group => group.Select(x => x.Filter).ToList());

			foreach (var group in filterGroups) {
				PrimaryFiltersPanel.AddFilterGroup(group.Key, group.Value);

				foreach (var filter in group.Value)
					OnFilterStateChanged(filter);
			}

			// Setup sorters
			_currentSorterIndex = 0;
			_sorters = GetSorters();
			_sorterResults.Clear();
			UseReverseSorting = true;
			
			foreach (var sorter in _sorters)
				_sorterResults.Add(SorterResults.Create(sorter));
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateSearch();
			UpdateListRefresh();
			TryAutoSelectFirstSlot();
		}

		private void UpdateSearch() {
			var currentSearchTerm = searchInput.GetInputText();
			if (currentSearchTerm == _lastSearchTerm)
				return;

			foreach (var otherSearchInput in otherSearchInputsToSync)
				otherSearchInput.SetInputText(currentSearchTerm);

			AdjustSearchFieldPosition();
			RequestListRefresh(false);

			_searchResults = SearchResults.Create(currentSearchTerm);
			_lastSearchTerm = currentSearchTerm;
		}

		private void TryAutoSelectFirstSlot() {
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				objectList.TrySelectSlot(0);
		}
		
		public void OnFilterStateChanged(Filter filter) {
			if (filter == null)
				return;
			
			_filterResults[filter] = FilterResults.Create(filter);
			
			RequestListRefresh(false);
		}
		
		public void ToggleFiltersPanel() {
			var shouldShow = !PrimaryFiltersPanel.IsShowing;
			foreach (var panel in filtersPanels)
				panel.IsShowing = shouldShow;

			if (UserInterfaceUtility.IsUsingMouse && OptionsManager.Instance.PanelsShiftLayout) {
				Manager.ui.DeselectAnySelectedUIElement();
				Manager.ui.mouse.UpdateMouseUIInput(out _, out _);				
			}

			AdjustWindowPosition();
		}

		public void NextSort() {
			_currentSorterIndex++;
			if (_currentSorterIndex >= _sorters.Count)
				_currentSorterIndex = 0;

			RequestListRefresh(false);
		}
		
		public void PrevSort() {
			_currentSorterIndex--;
			if (_currentSorterIndex < 0)
				_currentSorterIndex = _sorters.Count - 1;

			RequestListRefresh(false);
		}

		public void CycleSortOrder() {
			UseReverseSorting = !UseReverseSorting;
			RequestListRefresh(false);
		}

		public void ClearSearch() {
			searchInput.ResetText();
			foreach (var otherSearchInput in otherSearchInputsToSync)
				otherSearchInput.ResetText();
		}
		
		private void AdjustWindowPosition() {
			var shiftLayout = PrimaryFiltersPanel.IsShowing && OptionsManager.Instance.PanelsShiftLayout;
			transform.localPosition = new Vector3(Mathf.Round(shiftLayout ? -((PrimaryFiltersPanel.WindowWidth / 2f) + (1f / 16f)) : 0f), transform.localPosition.y, transform.localPosition.z);
		}
		
		private void AdjustSearchFieldPosition() {
			var maskUnitWidth = searchInputMask.transform.localScale.x / 16f;
			var searchInputPosition = searchInput.pugText.transform.localPosition;
			searchInputPosition.x = -1f * Mathf.Max(0f, searchInput.pugText.dimensions.width - maskUnitWidth);
			searchInput.pugText.transform.localPosition = searchInputPosition;

			var member = typeof(TextInputField).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "Update");
			API.Reflection.Invoke(member, searchInput);
		}

		public void RequestListRefresh(bool preserveScrollPosition) {
			_requestedListRefresh = true;
			if (!preserveScrollPosition)
				_requestedListRefreshPreservesScroll = false;
		}

		private void UpdateListRefresh() {
			UpdateAutoListRefresh();
			
			if (_listRefreshTask is { IsCompletedSuccessfully: true }) {
				_filteredObjects = _listRefreshTask.Result.Objects;
				objectList.SetObjects(_filteredObjects, _listRefreshTask.Result.PreserveScroll);
				_lastListRefreshTime = Time.time;
				_listRefreshTask = null;
			}

			if (_requestedListRefresh && _listRefreshTask == null) {
				_listRefreshTask = RunListRefreshTask(_requestedListRefreshPreservesScroll);
				_requestedListRefreshPreservesScroll = true;
				_requestedListRefresh = false;
			}
		}
		
		private void UpdateAutoListRefresh() {
			if ((PrimaryFiltersPanel.HasDynamicFiltersEnabled || OptionsManager.Instance.DiscoveryMode) && Time.time >= _lastListRefreshTime + DynamicFiltersRefreshInterval)
				RequestListRefresh(true);
		}
		
		private Task<(bool, List<ObjectDataCD>)> RunListRefreshTask(bool preserveScrollPosition) {
			// Filtering
			return Task.Run(() => {
				var allObjects = GetIncludedObjects();
				var filteredObjects = UseReverseSorting
					? allObjects
						.Where(MatchesFilters)
						.OrderByDescending(objectData => OptionsManager.Instance.IsFavorited(objectData) ? 1 : 0)
						.ThenByDescending(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
						.ThenByDescending(objectData => _sorterResults[0].GetScore(objectData))
						.ToList()
					: allObjects
						.Where(MatchesFilters)
						.OrderByDescending(objectData => OptionsManager.Instance.IsFavorited(objectData) ? 1 : 0)
						.ThenBy(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
						.ThenBy(objectData => _sorterResults[0].GetScore(objectData))
						.ToList();

				IncludedObjects = filteredObjects.Count;
				ExcludedObjects = allObjects.Count - IncludedObjects;

				return (preserveScrollPosition, filteredObjects);
			});

			/*var allObjects = GetIncludedObjects();
			var filteredObjects = UseReverseSorting
				? allObjects
					.Where(MatchesFilters)
					.OrderByDescending(objectData => OptionsManager.Instance.IsFavorited(objectData) ? 1 : 0)
					.ThenByDescending(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
					.ThenByDescending(objectData => _sorterResults[0].GetScore(objectData))
					.ToList()
				: allObjects
					.Where(MatchesFilters)
					.OrderByDescending(objectData => OptionsManager.Instance.IsFavorited(objectData) ? 1 : 0)
					.ThenBy(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
					.ThenBy(objectData => _sorterResults[0].GetScore(objectData))
					.ToList();

			_filteredObjects = filteredObjects;
			IncludedObjects = _filteredObjects.Count;
			ExcludedObjects = allObjects.Count - IncludedObjects;

			objectList.SetObjects(_filteredObjects, preserveScrollPosition);
			_refreshedListTime = Time.time;*/
		}

		private bool MatchesFilters(ObjectDataCD objectData) {
			return _searchResults.Matches(objectData)
			       && (!OptionsManager.Instance.DiscoveryMode || ObjectUtility.HasBeenDiscovered(objectData, true))
			       && PrimaryFiltersPanel.FiltersToInclude.All(group => group.Any(filter => _filterResults[filter].Matches(objectData)))
			       && !PrimaryFiltersPanel.FiltersToExclude.Any(group => group.Any(filter => _filterResults[filter].Matches(objectData)));
		}
		
		protected abstract List<Sorter> GetSorters();
		
		protected abstract List<(string Group, Filter Filter)> GetFilters();
		
		protected abstract List<ObjectDataCD> GetIncludedObjects();
	}
}