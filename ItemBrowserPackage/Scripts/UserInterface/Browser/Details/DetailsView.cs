using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class DetailsView : ItemBrowserView {
		private const int MaxHistory = 100;
		
		public EntriesView entriesSourceView;
		public EntriesView entriesUsageView;
		public SelectedObjectSlot selectedObjectSlot;
		public PugText selectedTabLabel;
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

		protected override void LateUpdate() {
			base.LateUpdate();
			
			UpdateControllerInput();
			AdjustWindowPosition();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtils.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (nextTabButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				SwapToNextTab();
			if (previousTabButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				SwapToPreviousTab();

			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement)
				TrySelectSelectedObjectSlot();
		}
		
		private void AdjustWindowPosition() {
			var shiftLayout = infoboxPanel.IsShowing && Options.Instance.PanelsShiftLayout;
			transform.localPosition = new Vector3(Mathf.Round(shiftLayout ? -((infoboxPanel.WindowWidth / 2f) + (1f / 16f)) : 0f), transform.localPosition.y, transform.localPosition.z);
		}
		
		public void TrySelectSelectedObjectSlot() {
			if (!UserInterfaceUtils.IsUsingMouseAndKeyboard)
				UserInterfaceUtils.SelectAndMoveMouseTo(selectedObjectSlot);
		}
		
		public bool PushState(DetailsState state, bool clearPreviousStates = false, bool force = false) {
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
			if (!_previousStateStack.TryPop(out var previousState))
				return false;
			
			ApplyState(previousState);
			return true;
		}
		
		private void ApplyState(DetailsState state) {
			var previousState = _currentState with {};
			_currentState = state;

			var selectedObjectChanged = !_currentState.ObjectData.Equals(previousState.ObjectData);
			var selectedTabChanged = _currentState.Tab != previousState.Tab;
			
			if (selectedObjectChanged) {
				IsSelectedObjectNonObtainable = ObjectUtils.IsNonObtainable(SelectedObject.objectID, SelectedObject.variation);

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
				
				if (nextTabButton.canBeClicked)
					nextTabButton.SetTab(_allAvailableTabs[GetNextTabIndex(1)]);
				if (previousTabButton.canBeClicked)
					previousTabButton.SetTab(_allAvailableTabs[GetNextTabIndex(-1)]);
			}

			if (selectedTabChanged) {
				foreach (var tab in _allTabs)
					GetTabView(tab).IsShowing = SelectedTab == tab;
			}
			
			GetTabView(SelectedTab).OnApplyState(_currentState, previousState);
			infoboxPanel.OnApplyState(_currentState, previousState);

			var stateForHistory = GetCurrentState();
			if (!stateForHistory.EqualsForHistory(previousState)) {
				_previousStateHistory.RemoveAll(previousStateForHistory => previousStateForHistory.EqualsForHistory(stateForHistory));
				_previousStateHistory.Add(stateForHistory);
				if (_previousStateHistory.Count > MaxHistory)
					_previousStateHistory.RemoveAt(0);
			}
		}
		
		public void ClearState() {
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