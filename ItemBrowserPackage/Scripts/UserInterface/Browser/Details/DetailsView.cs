using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;

namespace ItemBrowser.UserInterface.Browser {
	public class DetailsView : ItemBrowserView {
		public EntriesView entriesSourceView;
		public EntriesView entriesUsageView;
		public SelectedObjectSlot selectedObjectSlot;
		public PugText selectedTabLabel;
		public SwapTabButton nextTabButton;
		public SwapTabButton previousTabButton;

		public ObjectDataCD SelectedObject => _currentState.ObjectData;
		public DetailsTab SelectedTab => _currentState.Tab;
		public bool IsSelectedObjectNonObtainable { get; private set; }
		public bool HasPreviousStates => _previousStates.Count > 0;

		private DetailsState _currentState = new();
		private readonly Stack<DetailsState> _previousStates = new();
		private readonly List<DetailsTab> _allAvailableTabs = new();
		private readonly List<DetailsTab> _allTabs = new() {
			DetailsTab.Sources,
			DetailsTab.Usages
		};
		
		protected override void OnShow(bool isFirstTimeShowing) {
			TrySelectSelectedObjectSlot();
			
			if (SelectedObject.objectID != ObjectID.None)
				ApplyState(GetCurrentState());
		}

		private void LateUpdate() {
			UpdateControllerInput();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtils.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (nextTabButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				SwapToNextTab();
			if (previousTabButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				SwapToPreviousTab();
		}
		
		public void TrySelectSelectedObjectSlot() {
			if (!UserInterfaceUtils.IsUsingMouseAndKeyboard)
				UserInterfaceUtils.SelectAndMoveMouseTo(selectedObjectSlot);
		}

		public bool PushState(ObjectDataCD objectData, DetailsTab initialTab, bool clearPreviousStates = false) {
			if (objectData.Equals(SelectedObject) && initialTab == SelectedTab)
				return false;

			if (!IsTabAvailable(initialTab, objectData))
				return false;

			if (clearPreviousStates) {
				_previousStates.Clear();
			} else if (SelectedObject.objectID != ObjectID.None && (!SelectedObject.Equals(objectData) || initialTab != SelectedTab)) {
				_previousStates.Push(GetCurrentState());
			}

			ApplyState(new DetailsState {
				ObjectData = objectData,
				Tab = initialTab
			});
			
			return true;
		}

		public bool PopState() {
			if (!_previousStates.TryPop(out var previousState))
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
		}
		
		public void ClearState() {
			_currentState = new DetailsState();
			_previousStates.Clear();
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
				EntriesSourceScrollProgress = entriesSourceView.entriesList.scrollWindow.GetScrollValue(),
				EntriesUsageCategory = entriesUsageView.SelectedCategory,
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