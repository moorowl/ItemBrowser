using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.Options.DiscoveredObjects;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class DetailsView : ItemBrowserView {
		private const int MaxHistory = 100;
		
		public EntriesView entriesSourceView;
		public EntriesView entriesUsageView;
		public SelectedObjectSlot selectedObjectSlot;
		public PugText selectedTabLabel;
		public PugText selectedTabCountLabel;
		public SwapTabButton nextTabButton;
		public SwapTabButton previousTabButton;
		public InfoboxPanel infoboxPanel;

		private DetailsState _currentState = new();
		private readonly Stack<DetailsState> _previousStateStack = new();
		private readonly List<DetailsState> _previousStateHistory = new();
		private readonly List<DetailsTab> _allAvailableTabs = new();
		private readonly List<DetailsTab> _allTabs = new() {
			DetailsTab.Sources,
			DetailsTab.Usages
		};
		
		public ObjectDataCD SelectedObject => _currentState.ObjectData;
		public DetailsTab SelectedTab => _currentState.Tab;
		public bool IsSelectedObjectNonObtainable { get; private set; }
		public IEnumerable<DetailsState> History => _previousStateHistory;
		
		protected override void OnShow(bool isFirstTimeShowing) {
			if (SelectedObject.objectID != ObjectID.None)
				ApplyState(GetCurrentState());
		}

		protected override void OnHide() {
			DiscoveredTracker.ClearTemporarilyDiscovered(SelectedObject);
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			UpdateControllerInput();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (nextTabButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				SwapToNextTab();
			if (previousTabButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				SwapToPreviousTab();
			
			ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(ButtonHint.CycleSourceLeft);
			ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(ButtonHint.CycleSourceRight);

			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement)
				TrySelectSelectedObjectSlot();
		}
		
		public void TrySelectSelectedObjectSlot() {
			if (!UserInterfaceUtility.IsUsingMouseAndKeyboard)
				UserInterfaceUtility.SelectAndMoveMouseTo(selectedObjectSlot);
		}
		
		public bool PushState(DetailsState state, bool clearPreviousStates = false, bool force = false) {
			state.ObjectData = new ObjectDataCD {
				objectID = state.ObjectData.objectID,
				variation = ObjectUtility.GetPrimaryVariation(state.ObjectData)
			};
			
			if (!force && state.ObjectData.Equals(SelectedObject) && state.Tab == SelectedTab)
				return false;

			if (!IsTabAvailable(state.Tab, state.ObjectData))
				return false;

			if (clearPreviousStates) {
				_previousStateStack.Clear();
			} else if (SelectedObject.objectID != ObjectID.None && (!SelectedObject.Equals(state.ObjectData) || state.Tab != SelectedTab)) {
				_previousStateStack.Push(GetCurrentState());
			}

			ApplyState(state);

			return true;
		}
		
		public bool PushState(ObjectDataCD objectData, DetailsTab initialTab, bool clearPreviousStates = false, bool force = false) {
			return PushState(new DetailsState {
				ObjectData = objectData,
				Tab = initialTab
			}, force, clearPreviousStates);
		}

		public bool PopState() {
			AddCurrentStateToHistory();
			
			if (!_previousStateStack.TryPop(out var previousState))
				return false;
			
			ApplyState(previousState);
			return true;
		}

		public void AddCurrentStateToHistory() {
			var stateForHistory = GetCurrentState();
			if (stateForHistory.ObjectData.objectID != ObjectID.None) {
				_previousStateHistory.RemoveAll(previousStateForHistory => previousStateForHistory.EqualsForHistory(stateForHistory));
				_previousStateHistory.Add(stateForHistory);
				if (_previousStateHistory.Count > MaxHistory)
					_previousStateHistory.RemoveAt(0);
			}
		}
		
		private void ApplyState(DetailsState state) {
			AddCurrentStateToHistory();
			
			var previousState = _currentState with {};
			_currentState = state;

			var selectedObjectChanged = !_currentState.ObjectData.Equals(previousState.ObjectData);
			var selectedTabChanged = _currentState.Tab != previousState.Tab;
			
			if (selectedObjectChanged) {
				IsSelectedObjectNonObtainable = ObjectUtility.IsNonObtainable(SelectedObject.objectID, SelectedObject.variation);

				UpdateAvailableTabs();
				
				selectedObjectSlot.SetObjectData(SelectedObject);
				nextTabButton.canBeClicked = _allAvailableTabs.Count >= 2;
				previousTabButton.canBeClicked = _allAvailableTabs.Count >= 2;
			}

			if (selectedObjectChanged || selectedTabChanged) {
				var selectedTabLabelTerm = IsSelectedObjectNonObtainable
					? $"ItemBrowser-DetailsTabs/{SelectedTab}_NonObtainable"
					: $"ItemBrowser-DetailsTabs/{SelectedTab}";
				selectedTabLabel.Render(selectedTabLabelTerm);
				selectedTabCountLabel.Render($"{_allAvailableTabs.IndexOf(SelectedTab) + 1}/{_allAvailableTabs.Count}");
				
				if (nextTabButton.canBeClicked)
					nextTabButton.SetTab(_allAvailableTabs[GetNextTabIndex(1)]);
				if (previousTabButton.canBeClicked)
					previousTabButton.SetTab(_allAvailableTabs[GetNextTabIndex(-1)]);
			}

			if (selectedTabChanged) {
				foreach (var tab in _allTabs)
					GetTabView(tab).IsShowing = SelectedTab == tab;
			}
			
			DiscoveredTracker.ClearTemporarilyDiscovered(previousState.ObjectData);
			DiscoveredTracker.SetTemporarilyDiscovered(SelectedObject);
			
			GetTabView(SelectedTab).OnApplyState(_currentState, previousState);
			infoboxPanel.OnApplyState(_currentState, previousState);
			
			AddCurrentStateToHistory();
		}
		
		public void ClearState() {
			DiscoveredTracker.ClearTemporarilyDiscovered(_currentState.ObjectData);
			
			_currentState = new DetailsState();
			_previousStateStack.Clear();
		}
		
		public void SwapSelectedTab(DetailsTab tab) {
			if (!IsTabAvailable(tab, _currentState.ObjectData))
				return;
			
			ApplyState(GetCurrentState() with {
				Tab = tab
			});
		}
		
		public void SwapToNextTab() {
			SwapSelectedTab(_allAvailableTabs[GetNextTabIndex(1)]);
		}
		
		public void SwapToPreviousTab() {
			SwapSelectedTab(_allAvailableTabs[GetNextTabIndex(-1)]);
		}

		private int GetNextTabIndex(int offset) {
			var index = _allAvailableTabs.IndexOf(SelectedTab) + offset;
			if (index >= _allAvailableTabs.Count)
				index = 0;
			if (index < 0)
				index = _allAvailableTabs.Count - 1;

			return index;
		}

		private bool IsTabAvailable(DetailsTab tab, ObjectDataCD objectData) {
			return tab switch {
				DetailsTab.Sources => ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DetailsTab.Usages => ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Usage, objectData).Any(),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		
		private DetailsSubView GetTabView(DetailsTab tab) {
			return tab switch {
				DetailsTab.Sources => entriesSourceView,
				DetailsTab.Usages => entriesUsageView,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		private DetailsState GetCurrentState() {
			return new DetailsState {
				ObjectData = SelectedObject,
				Tab = SelectedTab,
				EntriesSourceCategory = entriesSourceView.SelectedCategory,
				EntriesSourceCategoryTerm = entriesSourceView.SelectedCategoryTerm,
				EntriesSourceScrollProgress = entriesSourceView.entriesList.scrollWindow.GetScrollValue(),
				EntriesUsageCategory = entriesUsageView.SelectedCategory,
				EntriesUsageCategoryTerm = entriesUsageView.SelectedCategoryTerm,
				EntriesUsageScrollProgress = entriesUsageView.entriesList.scrollWindow.GetScrollValue()
			};
		}

		private void UpdateAvailableTabs() {
			_allAvailableTabs.Clear();

			foreach (var tab in _allTabs) {
				if (IsTabAvailable(tab, SelectedObject))
					_allAvailableTabs.Add(tab);
			}
		}
	}
}