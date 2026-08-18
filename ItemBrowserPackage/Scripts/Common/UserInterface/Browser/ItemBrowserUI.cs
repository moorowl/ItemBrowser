using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Themes;
using ItemBrowser.Common.Options;
using PugMod;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ItemBrowserUI : ItemBrowserView {
		public const int MaxObjectsToHighlightPerView = 80;
		
		public static event Action<ItemBrowserUI> OnInit;
		public static event Action<ItemBrowserUI> OnUninit;
		
		public Transform root;
		public GameObject background;
		public MainView mainView;
		public DetailsView detailsView;
		public Gradient temporarilyDiscoveredGradient;
		public PugText[] buttonHintLines;
		public ContextView contextView;

		public ItemBrowserTheme CurrentTheme { get; private set; }
		private List<ItemBrowserTheme> _allThemes;
		private readonly List<ThemedRenderer> _themedRenderers = new();
		private readonly List<string> _activeButtonHints = new();

		private float _timeToAutoUpdateObjectsToHighlightInInventory;
		public readonly HashSet<ObjectID> ObjectsToHighlightInInventory = new();

		private void Awake() {
			gameObject.SetActive(false);
			OnInit?.Invoke(this);
			
			contextView.IsShowing = false;

			_allThemes = ItemBrowserTheme.GetAllThemesFromDataBlocks()
				.OrderBy(theme => theme.DisplayOrder)
				.ThenBy(theme => API.Localization.GetLocalizedTerm(theme.Term) ?? theme.Term)
				.ToList();
			var currentThemeIndex = _allThemes.FindIndex(theme => theme.Address == OptionsManager.Instance.Theme);
			ApplyTheme(_allThemes[currentThemeIndex > -1 ? currentThemeIndex : 0]);

			ThemedRenderer.OnEnabled += AddThemedRenderer;
			ThemedRenderer.OnDisabled += RemoveThemedRenderer;
		}
		
		private void OnDestroy() {
			ThemedRenderer.OnEnabled -= AddThemedRenderer;
			ThemedRenderer.OnDisabled -= RemoveThemedRenderer;
			
			OnUninit?.Invoke(this);
		}

		private void ApplyTheme(ItemBrowserTheme theme) {
			CurrentTheme = theme;
			OptionsManager.Instance.Theme = CurrentTheme.Address;
			
			foreach (var themedRenderer in _themedRenderers)
				ApplyTheme(themedRenderer);
		}
		
		private void ApplyTheme(ThemedRenderer themedRenderer) {
			themedRenderer.Apply(CurrentTheme.Address, CurrentTheme.SpriteReplacements, CurrentTheme.ColorReplacements);
		}
		
		public void SwapToNextTheme() {
			ApplyTheme(_allThemes[GetNextThemeIndex(1)]);
		}

		public void SwapToPreviousTheme() {
			ApplyTheme(_allThemes[GetNextThemeIndex(-1)]);
		}
		
		private int GetNextThemeIndex(int offset) {
			var index = _allThemes.FindIndex(theme => theme.Address == CurrentTheme.Address) + offset;
			if (index >= _allThemes.Count)
				index = 0;
			if (index < 0)
				index = _allThemes.Count - 1;

			return index;
		}
		
		private void AddThemedRenderer(ThemedRenderer themedRenderer) {
			_themedRenderers.Add(themedRenderer);
			ApplyTheme(themedRenderer);
		}
		
		private void RemoveThemedRenderer(ThemedRenderer themedRenderer) {
			_themedRenderers.Remove(themedRenderer);
		}

		protected override void OnShow(bool isFirstTimeShowing) {
			Manager.ui.DeselectAnySelectedUIElement();
			
			if (isFirstTimeShowing) {
				mainView.IsShowing = true;
				mainView.SwapToItemsTab();
				detailsView.IsShowing = false;
			}
			
			UpdateScale();
			HideMapIfShowing();
			PlayToggleSound();
			
			_activeButtonHints.Clear();
			UpdateButtonHints();
			
			Manager.input.SetActiveInputField(null);
		}

		protected override void OnHide() {
			PlayToggleSound();
			UpdateObjectsToHighlightInInventory();
		}

		public bool ShowDetails(ObjectDataCD objectData, DetailsTab initialTab) {
			if (!IsShowing && objectData.Equals(detailsView.SelectedObject)) {
				IsShowing = true;
			} else if (!detailsView.PushState(objectData, initialTab)) {
				return false;
			}
			
			IsShowing = true;
			mainView.IsShowing = false;
			detailsView.IsShowing = true;
			
			ItemBrowserSounds.PlayGenericOpen();

			return true;
		}
		
		public bool ShowDetailsFromHistory(DetailsState state) {
			if (!IsShowing && state.ObjectData.Equals(detailsView.SelectedObject)) {
				IsShowing = true;
			} else if (!detailsView.PushState(state, true, true)) {
				return false;
			}
			
			IsShowing = true;
			mainView.IsShowing = false;
			detailsView.IsShowing = true;
			
			ItemBrowserSounds.PlayGenericOpen();

			return true;
		}
		
		public void ShowGrid() {
			IsShowing = true;
			mainView.IsShowing = true;
			detailsView.IsShowing = false;
			detailsView.ClearState();
		}

		public void GoBack() {
			if (Manager.input.textInputIsActive) {
				Manager.input.activeInputField.Deactivate(true);
			} else if (detailsView.PopState()) {
				ItemBrowserSounds.PlayGenericClose();
			} else if (detailsView.IsShowing) {
				ShowGrid();
				ItemBrowserSounds.PlayGenericClose();
			} else {
				IsShowing = false;
			}
		}

		public void ShowButtonHint(ButtonHint hint, object[] formatFields = null) {
			var localizedHint = hint.GetLocalizedDescription(formatFields);
			if (!_activeButtonHints.Contains(localizedHint))
				_activeButtonHints.Add(localizedHint);
		}
		
		protected override void LateUpdate() {
			UpdateScale();
			UpdateGoBack();
			UpdateButtonHints();
			HideMapIfShowing();

			if (Time.time >= _timeToAutoUpdateObjectsToHighlightInInventory) {
				UpdateObjectsToHighlightInInventory();
				_timeToAutoUpdateObjectsToHighlightInInventory = Time.time + Random.Range(1f, 2f);
			}

			if (Manager.main.player != null && Manager.main.player.guestMode)
				IsShowing = false;

			if (Manager.ui.chatWindow.inputFieldPrompt.gameObject.activeSelf)
				Manager.ui.chatWindow.Deactivate(false);
			
			base.LateUpdate();
		}

		private void UpdateScale() {
			root.localScale = Manager.ui.CalcGameplayUITargetScaleMultiplier();
			background.SetActive(!Manager.prefs.hideInGameUI);
		}
		
		private void UpdateGoBack() {
			if (Manager.menu.IsAnyMenuActive() || Manager.input.textInputIsActive || ReferenceEquals(Manager.input.activeInputField, Manager.ui.chatWindow))
				return;

			if (Manager.input.IsMenuStartButtonDown() || Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.CANCEL))
				GoBack();
		}

		private void UpdateButtonHints() {
			foreach (var buttonHintLine in buttonHintLines)
				buttonHintLine.gameObject.SetActive(false);

			for (var i = 0; i < _activeButtonHints.Count; i++) {
				if (i >= _activeButtonHints.Count)
					break;
				
				buttonHintLines[i].gameObject.SetActive(true);
				buttonHintLines[i].Render(_activeButtonHints[_activeButtonHints.Count - 1 - i]);
			}

			_activeButtonHints.Clear();
		}
		
		private void UpdateObjectsToHighlightInInventory() {
			ObjectsToHighlightInInventory.Clear();

			AddObjectsFromGrid(mainView.itemsListView);
			AddObjectsFromGrid(mainView.creaturesListView);
			AddObjectsFromGrid(mainView.checklistListView);

			return;

			void AddObjectsFromGrid(ObjectListView view) {
				if (!view.HighlightSearchResults)
					return;

				var index = 0;
				foreach (var objectData in view.FilteredObjects) {
					if (index >= MaxObjectsToHighlightPerView)
						break;
					
					ObjectsToHighlightInInventory.Add(objectData.objectID);

					index++;
				}
			}
		}
		
		private static void HideMapIfShowing() {
			if (Manager.ui.isShowingMap)
				Manager.ui.HideMap();
		}

		private void PlayToggleSound() {
			if (Manager.load.IsScreenBlack())
				return;
			
			ItemBrowserSounds.PlayToggleBrowser();
		}

		public Color GetTemporarilyDiscoveredColor() {
			return temporarilyDiscoveredGradient?.Evaluate(Time.time % 1f) ?? Color.white;
		}
		
		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(MenuManager), "IsPauseDisabled")]
			[HarmonyPostfix]
			private static void MenuManager_IsPauseDisabled(MenuManager __instance, ref bool __result) {
				if (ItemBrowserAPI.ItemBrowserUI!= null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(PlayerController), "get_isInteractionBlocked")]
			[HarmonyPostfix]
			private static void PlayerController_get_isInteractionBlocked(PlayerController __instance, ref bool __result) {
				// Prevent using items / scrolling hotbar
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(PlayerController), "get_isUIShortCutsBlocked")]
			[HarmonyPostfix]
			private static void PlayerController_get_isUIShortCutsBlocked(PlayerController __instance, ref bool __result) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(PlayerController), "get_isMovingBlocked")]
			[HarmonyPostfix]
			private static void PlayerController_get_isMovingBlocked(PlayerController __instance, ref bool __result) {
				// Pretty sure this doesn't do anything
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = false;
			}
			
			[HarmonyPatch(typeof(UIManager), "get_isMouseShowing")]
			[HarmonyPostfix]
			private static void UIManager_get_isMouseShowing(UIManager __instance, ref bool __result) {
				// Force mouse to appear (for controllers)
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(InGameButtonHintsUI), "LateUpdate")]
			[HarmonyPostfix]
			private static void InGameButtonHintsUI_LateUpdate(InGameButtonHintsUI __instance) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__instance.container.gameObject.SetActive(false);
			}
			
			[HarmonyPatch(typeof(UIMouse), "UpdateMouseMode")]
			[HarmonyPostfix]
			private static void UIMouse_UpdateMouseMode(UIMouse __instance) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing) {
					if (__instance.mouseMode != UIMouse.MouseMode.Normal)
						__instance.SetMouseMode(UIMouse.MouseMode.Normal);
					
					if (__instance.mouseInventory != null && __instance.mouseInventory.HasObject(0))
						__instance.ReleaseGrabbedItemBackToInventory();
				}
			}

			[HarmonyPatch(typeof(SendClientInputSystem), "PlayerInteractionBlocked")]
			[HarmonyPostfix]
			private static void SendClientInputSystem_PlayerInteractionBlocked(SendClientInputSystem __instance, ref bool __result) {
				// Prevent using items / scrolling hotbar
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(SendClientInputSystem), "PlayerInputBlocked")]
			[HarmonyPostfix]
			private static void SendClientInputSystem_PlayerInputBlocked(SendClientInputSystem __instance, ref bool __result) {
				// Prevent moving when using a controller
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing && !Manager.input.singleplayerInputModule.PrefersKeyboardAndMouse() && !Manager.input.textInputIsActive)
					__result = true;
			}
			
			[HarmonyPatch(typeof(ItemSlotsBarUI), "Update")]
			[HarmonyPostfix]
			private static void ItemSlotsBarUI_Update(ItemSlotsBarUI __instance) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing && !Manager.ui.isPlayerInventoryShowing && !__instance.itemSlotsRoot.activeSelf && !__instance.isHintHotbar)
					__instance.itemSlotsRoot.SetActive(true);
			}
			
			[HarmonyPatch(typeof(ShortCutsWindow), "LateUpdate")]
			[HarmonyPostfix]
			private static void ShortCutsWindow_LateUpdate(ShortCutsWindow __instance) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__instance.HideUI();
			}
			
			[HarmonyPatch(typeof(UIManager), "get_isAnyInventoryShowing")]
			[HarmonyPostfix]
			private static void UIManager_get_isAnyInventoryShowing(UIManager __instance, ref bool __result) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
			
			[HarmonyPatch(typeof(UIScrollWindow), "UpdateScroll")]
			[HarmonyPrefix]
			private static bool UIScrollWindow_UpdateScroll(UIScrollWindow __instance) {
				// Disable scrolling in other windows
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing && !IsInsideView(__instance))
					return false;

				return true;
			}
		}
	}
}