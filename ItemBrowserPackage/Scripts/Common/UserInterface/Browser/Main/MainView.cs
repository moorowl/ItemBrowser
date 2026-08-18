using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class MainView : ItemBrowserView {
		public ItemsListView itemsListView;
		public CreaturesListView creaturesListView;
		public HistoryView historyView;
		public OptionsView optionsView;
		public ChecklistListView checklistListView;
		public Transform tabButtonsRoot;
		public ItemBrowserButton itemsTabButton;
		public ItemBrowserButton creaturesTabButton;
		public ItemBrowserButton historyTabButton;
		public ItemBrowserButton optionsTabButton;
		public ItemBrowserButton checklistTabButton;

		private MainTab _selectedTab;
		private UIelement _lastSelectedElement;

		private readonly List<MainTab> _allTabs = new() {
			MainTab.Items,
			MainTab.Creatures,
			MainTab.Checklist,
			MainTab.History,
			MainTab.Options
		};
		private readonly List<MainTab> _allAvailableTabs = new();

		protected override void OnShow(bool isFirstTimeShowing) {
			UpdateAvailableTabs();
			TrySelectLastSelectedElement();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateAvailableTabs();
			UpdateControllerInput();
			UpdateLastSelectedElement();

			// update is done from here to allow search result changes to happen immediately on other lists
			itemsListView.UpdateSearchAndListRefresh();
			creaturesListView.UpdateSearchAndListRefresh();
			checklistListView.UpdateSearchAndListRefresh();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_IN_MAP) || inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				SwapToNextTab();
			if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_OUT_MAP) || inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				SwapToPreviousTab();
			
			ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(ButtonHint.CycleTabLeft);
			ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(ButtonHint.CycleTabRight);
		}
		
		private void TrySelectLastSelectedElement() {
			if (_lastSelectedElement != null && !UserInterfaceUtility.IsUsingMouseAndKeyboard)
				UserInterfaceUtility.SelectAndMoveMouseTo(_lastSelectedElement);
		}

		private void UpdateLastSelectedElement() {
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				return;
			
			_lastSelectedElement = Manager.ui.currentSelectedUIElement;
		}

		public void SwapSelectedTab(MainTab tabToSelect) {
			if (tabToSelect == _selectedTab)
				return;

			var previousTab = _selectedTab;
			_selectedTab = tabToSelect;

			foreach (var tab in _allTabs) {
				GetTabView(tab).IsShowing = _selectedTab == tab;
				GetTabButton(tab).IsToggled = _selectedTab == tab;
			}

			var selectedTabView = GetTabView(_selectedTab);
			tabButtonsRoot.SetParent(selectedTabView.tabButtonsAnchor, false);

			if (previousTab != MainTab.None)
				ItemBrowserSounds.PlayChangeMainTab(this, _selectedTab);
		}

		public void SwapToItemsTab() {
			SwapSelectedTab(MainTab.Items);
		}

		public void SwapToCreaturesTab() {
			SwapSelectedTab(MainTab.Creatures);
		}
		
		public void SwapToHistoryTab() {
			SwapSelectedTab(MainTab.History);
		}
		
		public void SwapToOptionsTab() {
			SwapSelectedTab(MainTab.Options);
		}
		
		public void SwapToChecklistTab() {
			SwapSelectedTab(MainTab.Checklist);
		}

		private void SwapToNextTab() {
			SwapSelectedTab(_allAvailableTabs[GetNextTabIndex(1)]);
		}

		private void SwapToPreviousTab() {
			SwapSelectedTab(_allAvailableTabs[GetNextTabIndex(-1)]);
		}
		
		private int GetNextTabIndex(int offset) {
			var index = _allAvailableTabs.IndexOf(_selectedTab) + offset;
			if (index >= _allAvailableTabs.Count)
				index = 0;
			if (index < 0)
				index = _allAvailableTabs.Count - 1;

			return index;
		}
		
		private MainSubView GetTabView(MainTab tab) {
			return tab switch {
				MainTab.Items => itemsListView,
				MainTab.Creatures => creaturesListView,
				MainTab.History => historyView,
				MainTab.Options => optionsView,
				MainTab.Checklist => checklistListView,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		
		private ItemBrowserButton GetTabButton(MainTab tab) {
			return tab switch {
				MainTab.Items => itemsTabButton,
				MainTab.Creatures => creaturesTabButton,
				MainTab.History => historyTabButton,
				MainTab.Options => optionsTabButton,
				MainTab.Checklist => checklistTabButton,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		
		private bool IsTabAvailable(MainTab tab) {
			return tab switch {
				MainTab.Checklist => OptionsManager.Instance.ShowChecklist,
				_ => true
			};
		}
		
		private void UpdateAvailableTabs() {
			_allAvailableTabs.Clear();

			foreach (var tab in _allTabs) {
				var isAvailable = IsTabAvailable(tab);
				if (isAvailable)
					_allAvailableTabs.Add(tab);
				
				GetTabButton(tab).gameObject.SetActive(isAvailable);
			}
		}
	}
}