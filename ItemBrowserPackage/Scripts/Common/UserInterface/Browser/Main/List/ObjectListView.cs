using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using UnityEngine;
using UnityEngine.Serialization;

namespace ItemBrowser.Common.UserInterface.Browser {
	public abstract class ObjectListView : MainSubView {
		private const float DynamicFiltersRefreshInterval = 1f;
		
		public VirtualObjectList objectList;
		public SearchBar searchInput;
		public ObjectListView[] otherListsToSyncSearchWith;
		public SpriteMask searchInputMask;
		public FiltersPanel[] filtersPanels;
		
		private bool _requestedListRefresh;
		private bool _requestedListRefreshPreservesScroll;
		private float _lastListRefreshFromDynamicFiltersTime;
		private Task<(bool PreserveScroll, List<ObjectDataCD> Objects)> _listRefreshTask;

		private List<ObjectDataCD> _filteredObjects = new();
		public IEnumerable<ObjectDataCD> FilteredObjects => _filteredObjects;
		public int IncludedObjects { get; private set; }
		public int ExcludedObjects { get; private set; }
		
		private FiltersPanel PrimaryFiltersPanel => filtersPanels[0];
		private readonly Dictionary<Filter, FilterResults> _filterResults = new();
		private FilterResults _discoveryModeFilterResults;
		
		private string _lastSearchTerm = string.Empty;
		private bool _isSearchTermEmpty;
		private SearchResults _searchResults;
		public bool HighlightSearchResults { get; set; }

		private List<Sorter> _sorters;
		private readonly List<SorterResults> _sorterResults = new();
		private int _currentSorterIndex;
		public bool UseReverseSorting { get; set; }
		public Sorter CurrentSorter => _sorters[_currentSorterIndex];
		
		protected override void OnShow(bool isFirstTimeShowing) {
			if (isFirstTimeShowing) {
				SetupFiltersAndSorting();
				RequestListRefresh(false);
			} else {
				RequestListRefresh(true);
			}

			UpdateDynamicFilterResults();
			UpdateListRefresh();
			AdjustWindowPosition();
		}

		private void SetupFiltersAndSorting() {
			_searchResults = SearchResults.Create("");

			// Setup sorters
			_currentSorterIndex = 0;
			_sorters = GetSorters();
			_sorterResults.Clear();
			UseReverseSorting = true;
			
			foreach (var sorter in _sorters)
				_sorterResults.Add(SorterResults.Create(sorter));
			
			// Setup filters
			PrimaryFiltersPanel.Clear();

			var filterGroups = GetFilters().GroupBy(x => x.Group)
				.ToDictionary(group => group.Key, group => group.Select(x => x.Filter).ToList());

			foreach (var group in filterGroups) {
				PrimaryFiltersPanel.AddFilterGroup(group.Key, group.Value);

				foreach (var filter in group.Value)
					OnFilterStateChanged(filter);
			}
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateDynamicFilterResults();
			UpdateSearch(false);
			UpdateListRefresh();
			TryAutoSelectFirstSlot();
		}

		private void UpdateDynamicFilterResults() {
			var hasDynamicFiltersActive = PrimaryFiltersPanel.ActiveDynamicFilters.Any() || OptionsManager.Instance.DiscoveryMode;

			if (hasDynamicFiltersActive && Time.time >= _lastListRefreshFromDynamicFiltersTime + DynamicFiltersRefreshInterval) {
				var shouldRefresh = false;

				foreach (var filter in PrimaryFiltersPanel.ActiveDynamicFilters) {
					var updatedFilterResults = FilterResults.Create(filter);
					_filterResults[filter] = updatedFilterResults;
					shouldRefresh = true;

					/*if (!_filterResults.ContainsKey(filter) || !FilterResults.Equals(updatedFilterResults, _filterResults[filter])) {
						_filterResults[filter] = updatedFilterResults;
						shouldRefresh = true;
					}*/
				}

				var updatedDiscoveryModeFilterResults = FilterResults.Create(new Filter(string.Empty) {
					Function = objectData => DiscoveredTracker<ObjectDataCD>.HasBeenDiscovered(objectData, out var temporaryTimeRemaining) && temporaryTimeRemaining <= 0f
				});
				if (_discoveryModeFilterResults == null || !FilterResults.Equals(updatedDiscoveryModeFilterResults, _discoveryModeFilterResults)) {
					_discoveryModeFilterResults = updatedDiscoveryModeFilterResults;
					shouldRefresh = true;
				}

				if (shouldRefresh)
					RequestListRefresh(true);
				
				_lastListRefreshFromDynamicFiltersTime = Time.time;
			}
		}

		private void UpdateSearch(bool doNotSyncWithOtherLists) {
			var currentSearchTerm = searchInput.GetInputText();
			_isSearchTermEmpty = string.IsNullOrWhiteSpace(currentSearchTerm);
			if (currentSearchTerm == _lastSearchTerm)
				return;

			if (!doNotSyncWithOtherLists) {
				foreach (var otherList in otherListsToSyncSearchWith)
					otherList.SetSearchTermFromOtherList(currentSearchTerm);	
			}

			AdjustSearchFieldPosition();
			RequestListRefresh(false);

			_searchResults = SearchResults.Create(currentSearchTerm);
			_lastSearchTerm = currentSearchTerm;
		}

		public void SetSearchTermFromOtherList(string term) {
			searchInput.SetInputText(term);
			
			UpdateSearch(true);
			UpdateListRefresh();
		}

		private void TryAutoSelectFirstSlot() {
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				objectList.TrySelectListItem(0);
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

			UpdateSearch(false);
			UpdateListRefresh();
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
			if (_listRefreshTask is { IsCompleted: true }) {
				if (_listRefreshTask.IsCompletedSuccessfully) {
					_filteredObjects = _listRefreshTask.Result.Objects;
					objectList.SetObjects(_filteredObjects, _listRefreshTask.Result.PreserveScroll);
				} else {
					Main.Log($"{nameof(ObjectListView)}+{name}", $"List refresh task didn't complete successfully, retrying");
					Main.Log(_listRefreshTask.Exception);
					_requestedListRefresh = true;
				}
				
				_listRefreshTask = null;
			}

			if (_requestedListRefresh && _listRefreshTask == null) {
				_listRefreshTask = RunListRefreshTask(_requestedListRefreshPreservesScroll);
				_requestedListRefreshPreservesScroll = true;
				_requestedListRefresh = false;
			}
		}

		private Task<(bool, List<ObjectDataCD>)> RunListRefreshTask(bool preserveScrollPosition) {
			// Filtering
			return Task.Run(() => {
				var allObjects = GetIncludedObjects();
				var filteredObjects = UseReverseSorting
					? allObjects
						.Where(MatchesFilters)
						.OrderByDescending(objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Favorited) ? 1 : 0)
						.ThenByDescending(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
						.ThenByDescending(objectData => _sorterResults[0].GetScore(objectData))
						.ToList()
					: allObjects
						.Where(MatchesFilters)
						.OrderByDescending(objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Favorited) ? 1 : 0)
						.ThenBy(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
						.ThenBy(objectData => _sorterResults[0].GetScore(objectData))
						.ToList();

				IncludedObjects = filteredObjects.Count;
				ExcludedObjects = allObjects.Count - IncludedObjects;

				return (preserveScrollPosition, filteredObjects);
			});
		}

		private bool MatchesFilters(ObjectDataCD objectData) {
			return _searchResults.Matches(objectData)
			       && (!OptionsManager.Instance.DiscoveryMode || !_isSearchTermEmpty || _discoveryModeFilterResults.Matches(objectData))
			       && PrimaryFiltersPanel.FiltersToInclude.All(group => group.Any(filter => _filterResults[filter].Matches(objectData)))
			       && !PrimaryFiltersPanel.FiltersToExclude.Any(group => group.Any(filter => _filterResults[filter].Matches(objectData)));
		}
		
		protected abstract List<Sorter> GetSorters();
		
		protected abstract List<(string Group, Filter Filter)> GetFilters();
		
		protected abstract List<ObjectDataCD> GetIncludedObjects();
	}
}