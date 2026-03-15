using System;
using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ItemBrowser.UserInterface.Browser {
	public class MainView : ItemBrowserView {
		public GridWithItemsView gridWithItemsView;
		public GridWithCreaturesView gridWithCreaturesView;
		public HistoryView historyView;
		public OptionsView optionsView;
		public Transform tabButtonsRoot;
		public ItemBrowserButton itemsTabButton;
		public ItemBrowserButton creaturesTabButton;
		public ItemBrowserButton historyTabButton;
		public ItemBrowserButton optionsTabButton;

		private MainTab _selectedTab;
		private UIelement _lastSelectedElement;

		private readonly List<MainTab> _allTabs = new() {
			MainTab.Items,
			MainTab.Creatures,
			MainTab.History,
			MainTab.Options
		};

		protected override void OnShow(bool isFirstTimeShowing) {
			TrySelectLastSelectedElement();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateControllerInput();
			UpdateLastSelectedElement();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtils.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_IN_MAP) || inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				SwapToNextTab();
			if (inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_OUT_MAP) || inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				SwapToPreviousTab();
		}
		
		private void TrySelectLastSelectedElement() {
			if (_lastSelectedElement != null && !UserInterfaceUtils.IsUsingMouseAndKeyboard)
				UserInterfaceUtils.SelectAndMoveMouseTo(_lastSelectedElement);
		}

		private void UpdateLastSelectedElement() {
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement || !SnapPoint.HasSnapPoint(Manager.ui.currentSelectedUIElement))
				return;
			
			_lastSelectedElement = Manager.ui.currentSelectedUIElement;
		}

		public void SwapSelectedTab(MainTab tabToSelect) {
			_selectedTab = tabToSelect;

			foreach (var tab in _allTabs) {
				GetTabView(tab).IsShowing = _selectedTab == tab;
				GetTabButton(tab).IsToggled = _selectedTab == tab;
			}

			var selectedTabView = GetTabView(_selectedTab);
			tabButtonsRoot.SetParent(selectedTabView.tabButtonsAnchor, false);
			
			// if the filters panel is open, changing tab would shift the layout and cause tooltips to stay active
			if (UserInterfaceUtils.IsUsingMouse) {
				Manager.ui.DeselectAnySelectedUIElement();
				Manager.ui.mouse.UpdateMouseUIInput(out _, out _);				
			}

			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTabOrCategory, this);
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

		private void SwapToNextTab() {
			SwapSelectedTab(_allTabs[GetNextTabIndex(1)]);
		}

		private void SwapToPreviousTab() {
			SwapSelectedTab(_allTabs[GetNextTabIndex(-1)]);
		}
		
		private int GetNextTabIndex(int offset) {
			var index = _allTabs.IndexOf(_selectedTab) + offset;
			if (index >= _allTabs.Count)
				index = 0;
			if (index < 0)
				index = _allTabs.Count - 1;

			return index;
		}
		
		private MainSubView GetTabView(MainTab tab) {
			return tab switch {
				MainTab.Items => gridWithItemsView,
				MainTab.Creatures => gridWithCreaturesView,
				MainTab.History => historyView,
				MainTab.Options => optionsView,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		
		private ItemBrowserButton GetTabButton(MainTab tab) {
			return tab switch {
				MainTab.Items => itemsTabButton,
				MainTab.Creatures => creaturesTabButton,
				MainTab.History => historyTabButton,
				MainTab.Options => optionsTabButton,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}