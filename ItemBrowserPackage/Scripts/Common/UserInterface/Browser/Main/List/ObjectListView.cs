using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.Options.DiscoveredObjects;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public abstract class ObjectListView : MainSubView {
		private const float DynamicFiltersRefreshInterval = 1f;
		
		public VirtualObjectList objectList;
		public SearchBar searchInput;
		public ObjectListView[] otherListsToSyncSearchWith;
		public FiltersPanel[] filtersPanels;
		public bool canHideUndiscoveredObjects;
		
		private bool _requestedListRefresh;
		private bool _requestedListRefreshPreservesScroll;
		private float _lastListRefreshFromDynamicFiltersTime;
		private Task<(bool PreserveScroll, List<VirtualObjectList.Entry> Entries)> _listRefreshTask;

		private List<ObjectDataCD> _filteredObjects = new();
		public IEnumerable<ObjectDataCD> FilteredObjects => _filteredObjects;
		public int IncludedObjects { get; private set; }
		public int ExcludedObjects { get; private set; }
		public int ExcludedObjectsFromFilters { get; private set; }
		
		private FiltersPanel PrimaryFiltersPanel => filtersPanels[0];
		private readonly Dictionary<Filter, FilterResults> _filterResults = new();
		private FilterResults _discoveredFilterResults;
		
		private string _lastSearchTerm = string.Empty;
		private bool _isSearchTermEmpty;
		private SearchResults _searchResults;
		private Task<SearchResults> _updateSearchResultsTask;
		public bool HighlightSearchResults { get; set; }

		private readonly List<Sorter> _sorters = new();
		private readonly List<SorterResults> _sorterResults = new();
		private int _currentSorterIndex;
		public bool UseReverseSorting { get; set; }
		public Sorter CurrentSorter => _currentSorterIndex < 0 || _currentSorterIndex >= _sorters.Count ? null : _sorters[_currentSorterIndex];

		private readonly List<Group> _groups = new();
		private readonly List<GroupResults> _groupResults = new();
		private int _currentGroupIndex;
		private readonly Dictionary<int, bool> _currentGroupCollapsed = new();
		public Group CurrentGroup => _currentGroupIndex < 0 || _currentGroupIndex >= _groups.Count ? null : _groups[_currentGroupIndex];
		
		protected override void OnShow(bool isFirstTimeShowing) {
			base.OnShow(isFirstTimeShowing);
			
			if (isFirstTimeShowing) {
				SetupFiltersAndSorting();
				RequestListRefresh(false);
			} else {
				RequestListRefresh(true);
			}

			if (UpdateDiscoveredFilterResults())
				RequestListRefresh(true);
			
			UpdateListRefresh();

			OptionsManager.Instance.OnTagChanged += OnTagsChanged;
		}

		protected override void OnHide() {
			base.OnHide();

			OptionsManager.Instance.OnTagChanged -= OnTagsChanged;
		}

		private void SetupFiltersAndSorting() {
			_searchResults = SearchResults.Create("", GetIncludedObjects());

			// Setup sorters
			_currentSorterIndex = 0;
			_sorters.Clear();
			_sorters.AddRange(GetSorters());
			_sorterResults.Clear();
			UseReverseSorting = true;
			
			foreach (var sorter in _sorters)
				_sorterResults.Add(SorterResults.Create(sorter, GetIncludedObjects()));
			
			// Setup groups
			_currentGroupIndex = 0;
			_groups.Clear();
			_groupResults.Clear();
			_currentGroupCollapsed.Clear();

			if (this is CookingListView) {
				_groups.Add(new Group("Cooking") {
					Names = new[] {
						"ItemBrowser-Groups/Ingredients",
						"ItemBrowser-Groups/CookedFood"
					},
					Function = objectData => PugDatabase.HasComponent<CookedFoodCD>(objectData) ? 1 : 0
				});
			} else {
				_groups.Add(new Group("None"));
			}
			
			foreach (var group in _groups)
				_groupResults.Add(GroupResults.Create(group, GetIncludedObjects()));
			
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

			UpdateSearchAndListRefresh();
			TryAutoSelectFirstSlot();
		}

		public void UpdateSearchAndListRefresh() {
			UpdateSearch(false);
			UpdateListRefresh();
		}
		
		private void OnTagsChanged(ObjectDataCD objectData, ObjectTagType tag, bool isAdded) {
			if (tag == ObjectTagType.Favorited || (tag == ObjectTagType.Discovered && UpdateDiscoveredFilterResults()))
				RequestListRefresh(true);
		}
		
		private bool UpdateDiscoveredFilterResults() {
			var updatedDiscoveryModeFilterResults = FilterResults.Create(new Filter(string.Empty) {
				Function = DiscoveredTracker.HasBeenDiscovered
			}, GetIncludedObjects());

			if (_discoveredFilterResults == null || !FilterResults.Equals(updatedDiscoveryModeFilterResults, _discoveredFilterResults)) {
				_discoveredFilterResults = updatedDiscoveryModeFilterResults;
				return true;
			}

			return false;
		}

		private bool UpdateDynamicFilterResults() {
			if (PrimaryFiltersPanel.ActiveDynamicFilters.Any() && Time.time >= _lastListRefreshFromDynamicFiltersTime + DynamicFiltersRefreshInterval) {
				var shouldRefresh = false;
				var includedObjects = GetIncludedObjects();

				foreach (var filter in PrimaryFiltersPanel.ActiveDynamicFilters) {
					var updatedFilterResults = FilterResults.Create(filter, includedObjects);

					if (!_filterResults.ContainsKey(filter) || !FilterResults.Equals(updatedFilterResults, _filterResults[filter])) {
						_filterResults[filter] = updatedFilterResults;
						shouldRefresh = true;
						// Debug.Log($"updating dynamic filter results for {filter.Name}");
					}
				}
				
				_lastListRefreshFromDynamicFiltersTime = Time.time;

				return shouldRefresh;
			}

			return false;
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

			_searchResults = SearchResults.Create(currentSearchTerm, GetIncludedObjects());
			_lastSearchTerm = currentSearchTerm;
			
			RequestListRefresh(false);
		}

		public void SetSearchTermFromOtherList(string term) {
			searchInput.SetInputText(term);

			UpdateSearch(true);
		}

		private void TryAutoSelectFirstSlot() {
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				objectList.TrySelectListItem(0);
		}
		
		public void OnFilterStateChanged(Filter filter) {
			if (filter == null)
				return;
			
			_filterResults[filter] = FilterResults.Create(filter, GetIncludedObjects());
			
			RequestListRefresh(false);
		}

		public bool IsIndexInGroupCollapsed(int index) {
			return _currentGroupCollapsed.GetValueOrDefault(index, false);
		}

		public void SetIndexInGroupCollapsed(int index, bool isCollapsed) {
			_currentGroupCollapsed[index] = isCollapsed;
			RequestListRefresh(true);
		}
		
		public void ToggleFiltersPanel() {
			var shouldShow = !PrimaryFiltersPanel.IsShowing;
			foreach (var panel in filtersPanels)
				panel.IsShowing = shouldShow;
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

		public void RequestListRefresh(bool preserveScrollPosition) {
			_requestedListRefresh = true;
			if (!preserveScrollPosition)
				_requestedListRefreshPreservesScroll = false;
		}

		private void UpdateListRefresh() {
			if (_listRefreshTask is { IsCompleted: true }) {
				if (_listRefreshTask.IsCompletedSuccessfully) {
					_filteredObjects = _listRefreshTask.Result.Entries.Select(entry => entry.ObjectData).ToList();
					objectList.SetEntries(_listRefreshTask.Result.Entries, ExcludedObjectsFromFilters, _listRefreshTask.Result.PreserveScroll);
				} else {
					Logger.LogException(_listRefreshTask.Exception);
					_requestedListRefresh = true;
				}
				
				_listRefreshTask = null;
			}

			if (IsShowing && _listRefreshTask == null && UpdateDynamicFilterResults())
				RequestListRefresh(true);
			
			if (_requestedListRefresh && _listRefreshTask == null && _currentSorterIndex < _sorterResults.Count) {
				_listRefreshTask = RunListRefreshTask(_requestedListRefreshPreservesScroll);
				_requestedListRefreshPreservesScroll = true;
				_requestedListRefresh = false;
			}
		}
		
		private Task<(bool, List<VirtualObjectList.Entry>)> RunListRefreshTask(bool preserveScrollPosition) {
			return Task.Run(() => {
				IncludedObjects = 0;
				ExcludedObjectsFromFilters = 0;

				var allObjects = GetIncludedObjects();
				var results = new List<VirtualObjectList.Entry>();

				foreach (var grouping in GetIncludedObjects().Where(objectData => _isSearchTermEmpty || _searchResults.Matches(objectData)).GroupBy(objectData => _groupResults[_currentGroupIndex].GetGroup(objectData))) {
					var groupName = _groupResults[_currentGroupIndex].GetGroupName(grouping.Key);
					var groupCount = grouping.Count();
					var headerIndex = -1;

					if (groupName != null) {
						headerIndex = results.Count;

						if (IsIndexInGroupCollapsed(grouping.Key)) {
							results.Add(VirtualObjectList.Entry.Header(grouping.Key, grouping.Where(MatchesFilters).Count(), groupName));
							continue;
						} else {
							// replaced further down
							results.Add(default);
						}
					}

					var includedObjectsFromFilters = 0;

					if (UseReverseSorting) {
						var sortedObjectDatas = grouping
							.Where(MatchesFilters)
							.OrderByDescending(objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Favorited) ? 1 : 0)
							.ThenByDescending(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
							.ThenByDescending(objectData => _sorterResults[0].GetScore(objectData));

						foreach (var objectData in sortedObjectDatas) {
							results.Add(VirtualObjectList.Entry.Object(objectData));
							IncludedObjects++;
							includedObjectsFromFilters++;
						}
					} else {
						var sortedObjectDatas = grouping
							.Where(MatchesFilters)
							.OrderByDescending(objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Favorited) ? 1 : 0)
							.ThenBy(objectData => _sorterResults[_currentSorterIndex].GetScore(objectData))
							.ThenBy(objectData => _sorterResults[0].GetScore(objectData));
						
						foreach (var objectData in sortedObjectDatas) {
							results.Add(VirtualObjectList.Entry.Object(objectData));
							IncludedObjects++;
							includedObjectsFromFilters++;
						}
					}

					if (headerIndex != -1)
						results[headerIndex] = VirtualObjectList.Entry.Header(grouping.Key, includedObjectsFromFilters, groupName);
					
					var excludedObjectsFromFilters = groupCount - includedObjectsFromFilters;
					if (excludedObjectsFromFilters > 0 && (headerIndex == -1 || includedObjectsFromFilters > 0))
						results.Add(VirtualObjectList.Entry.ExtraAmount(excludedObjectsFromFilters));
				}

				ExcludedObjects = allObjects.Count - IncludedObjects;

				return (preserveScrollPosition, results);
			});
		}

		private bool MatchesFilters(ObjectDataCD objectData) {
			return !(canHideUndiscoveredObjects && OptionsManager.Instance.DiscoveryMode && _isSearchTermEmpty && !_discoveredFilterResults.Matches(objectData))
			       && PrimaryFiltersPanel.FiltersToInclude.All(group => group.Any(filter => _filterResults[filter].Matches(objectData)))
			       && !PrimaryFiltersPanel.FiltersToExclude.Any(group => group.Any(filter => _filterResults[filter].Matches(objectData)));
		}
		
		public abstract List<Sorter> GetSorters();
		
		public abstract List<(string Group, Filter Filter)> GetFilters();
		
		public abstract HashSet<ObjectDataCD> GetIncludedObjects();
	}
}