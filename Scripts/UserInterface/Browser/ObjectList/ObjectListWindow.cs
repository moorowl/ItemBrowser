using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using PugMod;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public abstract class ObjectListWindow : ItemBrowserWindow {
		private const float DynamicFiltersRefreshInterval = 1f;
		
		[SerializeField]
		private VirtualObjectList objectList;
		[SerializeField]
		private TextInputField searchInput;
		[SerializeField]
		private TextInputField[] otherSearchInputsToSync;
		[SerializeField]
		private SpriteMask searchInputMask;
		[SerializeField]
		private FiltersPanel[] filtersPanels;
		public Transform tabButtonsAnchor;
		public Transform optionsPanelAnchor;
		
		private bool _refreshList;
		private bool _preserveScrollOnRefresh;
		private float _refreshedListTime;

		public int IncludedObjects { get; private set; }
		public int ExcludedObjects { get; private set; }
		
		private string SearchTerm => searchInput.GetInputText();
		private string _lastSearchTerm = string.Empty;
		
		private List<Sorter<ObjectDataCD>> _sorters;
		private int _currentSorterIndex;
		public bool UseReverseSorting { get; set; }
		public Sorter<ObjectDataCD> CurrentSorter => _sorters[_currentSorterIndex];
		
		private readonly SearchFilter<ObjectDataCD> _searchFilter = new((objectData, names) => {
			var displayName = ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation);
			names.Add(displayName ?? objectData.objectID.ToString());
			if (displayName == null)
				names.Add(objectData.objectID.ToString());
			names.Add(((int) objectData.objectID).ToString());
		});

		private FiltersPanel PrimaryFiltersPanel => filtersPanels[0];
		
		protected override void OnShow(bool isFirstTimeShowing) {
			objectList.ShowContainerUI();
			
			if (isFirstTimeShowing) {
				SetupFiltersAndSorting();
				RefreshList(false);
			} else {
				RefreshList(true);
			}
			
			objectList.ShowContainerUI();
			if (isFirstTimeShowing)
				objectList.TrySelectSlot(0);
			
			AdjustWindowPosition();
		}

		protected override void OnHide() {
			objectList.HideContainerUI();
		}

		private void LateUpdate() {
			var currentSearchTerm = SearchTerm;
			if (currentSearchTerm != _lastSearchTerm) {
				foreach (var otherSearchInput in otherSearchInputsToSync)
					otherSearchInput.SetInputText(currentSearchTerm);
				
				AdjustSearchFieldPosition();
				RequestListRefresh(false);
				_lastSearchTerm = currentSearchTerm;
			}

			if ((PrimaryFiltersPanel.HasDynamicFiltersEnabled || Options.DefaultDiscoveredFilter) && Time.time >= _refreshedListTime + DynamicFiltersRefreshInterval)
				RequestListRefresh(true);
			
			if (_refreshList) {
				RefreshList(_preserveScrollOnRefresh);
				_preserveScrollOnRefresh = true;
				_refreshList = false;
			}
			
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				objectList.TrySelectSlot(0);
		}
		
		private void SetupFiltersAndSorting() {
			// Setup sorters
			_currentSorterIndex = 0;
			_sorters = GetSorters();
			UseReverseSorting = true;
			
			// Setup filters
			PrimaryFiltersPanel.Clear();
			var filterGroups = GetFilters().GroupBy(x => x.Group)
				.ToDictionary(group => group.Key, group => group.Select(x => x.Filter).ToList());

			foreach (var group in filterGroups) {
				PrimaryFiltersPanel.AddHeader(group.Key);
				foreach (var filter in group.Value)
					PrimaryFiltersPanel.AddFilter(filter);
			}
		}
		
		protected abstract List<Sorter<ObjectDataCD>> GetSorters();
		
		protected abstract List<(string Group, Filter<ObjectDataCD> Filter)> GetFilters();
		
		protected abstract List<ObjectDataCD> GetIncludedObjects();
		
		public void ToggleFiltersPanel() {
			var shouldShow = !PrimaryFiltersPanel.IsShowing;
			foreach (var panel in filtersPanels)
				panel.IsShowing = shouldShow;

			if (UserInterfaceUtils.IsUsingMouse) {
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
			transform.localPosition = new Vector3(Mathf.Round(PrimaryFiltersPanel.IsShowing ? -((PrimaryFiltersPanel.WindowWidth / 2f) + (1f / 16f)) : 0f), transform.localPosition.y, transform.localPosition.z);
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
			_refreshList = true;
			if (!preserveScrollPosition)
				_preserveScrollOnRefresh = false;
		}
		
		private void RefreshList(bool preserveScrollPosition) {
			_searchFilter.Term = SearchTerm;
			
			// Filtering
			var allObjects = GetIncludedObjects();
			var filteredObjects = allObjects
				.Where(MatchesFilters)
				.OrderBy(objectData => Options.FavoritedObjects.Contains(objectData) ? 1 : 0)
				.ThenBy(objectData => CurrentSorter.Function(objectData))
				.ThenBy(objectData => _sorters[0].Function(objectData))
				.ToList();

			if (UseReverseSorting)
				filteredObjects.Reverse();
			
			IncludedObjects = filteredObjects.Count;
			ExcludedObjects = allObjects.Count - IncludedObjects;
			
			objectList.SetObjects(filteredObjects, preserveScrollPosition);
			_refreshedListTime = Time.time;
		}

		private bool MatchesFilters(ObjectDataCD objectData) {
			return _searchFilter.Function(objectData)
			       && (!Options.DefaultDiscoveredFilter || ObjectUtils.HasBeenDiscovered(objectData.objectID, objectData.variation, true))
			       && PrimaryFiltersPanel.FiltersToInclude.All(group => group.Any(filter => filter.Matches(objectData)))
			       && !PrimaryFiltersPanel.FiltersToExclude.Any(group => group.Any(filter => filter.Matches(objectData)));
		}
	}
}