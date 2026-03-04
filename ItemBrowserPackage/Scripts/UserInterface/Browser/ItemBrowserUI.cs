using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ItemBrowser.Api;
using ItemBrowser.Api.Themes;
using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.UserInterface.Browser {
	public class ItemBrowserUI : ItemBrowserView {
		public static event Action<ItemBrowserUI> OnInit;
		public static event Action<ItemBrowserUI> OnUninit;
		
		public Transform root;
		public GameObject background;
		public GameObject tileGridPrefab;
		public MainView mainView;
		public DetailsView detailsView;

		public ItemBrowserTheme CurrentTheme { get; private set; }
		private List<ItemBrowserTheme> _allThemes;
		private readonly List<ThemedRenderer> _themedRenderers = new();
		
		private GameObject _tileGrid;
		private Transform _tileGridRenderAnchor;

		private float _timeToAutoUpdateObjectsToHighlightInInventory;
		private readonly HashSet<ObjectID> _objectsToHighlightInInventory = new();

		private void Awake() {
			gameObject.SetActive(false);
			OnInit?.Invoke(this);

			UpdateThemeOrdering();
			var currentThemeIndex = _allThemes.FindIndex(theme => theme.Address == Options.Instance.Theme);
			ApplyTheme(_allThemes[currentThemeIndex > -1 ? currentThemeIndex : 0]);
			
			_tileGridRenderAnchor = Manager.camera.GetRenderAnchor();
			_tileGrid = Instantiate(tileGridPrefab, _tileGridRenderAnchor);

			ThemedRenderer.OnEnabled += AddThemedRenderer;
			ThemedRenderer.OnDisabled += RemoveThemedRenderer;
			ItemBrowserAPI.OnClientLanguageChanged += UpdateThemeOrdering;
		}
		
		private void OnDestroy() {
			ThemedRenderer.OnEnabled -= AddThemedRenderer;
			ThemedRenderer.OnDisabled -= RemoveThemedRenderer;
			ItemBrowserAPI.OnClientLanguageChanged -= UpdateThemeOrdering;
			
			OnUninit?.Invoke(this);
			
			if (_tileGrid != null) {
				Destroy(_tileGrid);
				Manager.camera.ReturnRenderAnchor(_tileGridRenderAnchor);
			}
		}
		
		private void UpdateThemeOrdering() {
			_allThemes = ItemBrowserAPI.Registry.Themes.Values
				.OrderBy(theme => theme.DisplayOrder)
				.ThenBy(theme => API.Localization.GetLocalizedTerm(theme.Term) ?? theme.Term)
				.ToList();
		}

		private void ApplyTheme(ItemBrowserTheme theme) {
			CurrentTheme = theme;
			Options.Instance.Theme = CurrentTheme.Address;
			
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
			
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericOpen, this);

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
			} else if (detailsView.HasPreviousStates) {
				detailsView.PopState();
				UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericClose, this);
			} else if (detailsView.IsShowing) {
				ShowGrid();
				UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericClose, this);
			} else {
				IsShowing = false;
			}
		}
		
		private void LateUpdate() {
			UpdateScale();
			UpdateGoBack();
			HideMapIfShowing();
			UpdateSwapToInventory();

			if (Time.time >= _timeToAutoUpdateObjectsToHighlightInInventory) {
				UpdateObjectsToHighlightInInventory();
				_timeToAutoUpdateObjectsToHighlightInInventory = Time.time + Random.Range(1f, 2f);
			}

			if (Manager.main.player != null && Manager.main.player.guestMode)
				IsShowing = false;
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

		private void UpdateSwapToInventory() {
			var player = Manager.main.player;
			if (!player.guestMode && player.inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.TOGGLE_INVENTORY))
				if (Manager.ui.isPlayerInventoryShowing)
					player.CloseAnyOpenInventory();
				else
					player.OpenPlayerInventory();
		}

		private void UpdateObjectsToHighlightInInventory() {
			_objectsToHighlightInInventory.Clear();

			if (mainView.gridWithItemsView.searchInput.HighlightSearchResults) {
				foreach (var objectData in mainView.gridWithItemsView.FilteredObjects)
					_objectsToHighlightInInventory.Add(objectData.objectID);
			}
			
			if (mainView.gridWithCreaturesView.searchInput.HighlightSearchResults) {
				foreach (var objectData in mainView.gridWithCreaturesView.FilteredObjects)
					_objectsToHighlightInInventory.Add(objectData.objectID);
			}
		}
		
		private static void HideMapIfShowing() {
			if (Manager.ui.isShowingMap)
				Manager.ui.HideMap();
		}

		private void PlayToggleSound() {
			if (Manager.main.player == null)
				return;
			
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ToggleBrowser, this);
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
			
			[HarmonyPatch(typeof(UIMouse), "UpdateSlotHighlights")]
			[HarmonyPostfix]
			private static void UIMouse_UpdateSlotHighlights(UIMouse __instance) {
				if (ItemBrowserAPI.ItemBrowserUI != null) {
					var objectsToHighlight = ItemBrowserAPI.ItemBrowserUI._objectsToHighlightInInventory;
					
					if (Manager.ui.isChestInventoryUIShowing)
						TryHighlightItemSlots(Manager.ui.chestInventoryUI.itemSlots, objectsToHighlight);

					if (Manager.ui.isPlayerInventoryShowing) {
						TryHighlightItemSlots(Manager.ui.playerInventoryUI.itemSlots, objectsToHighlight);

						foreach (var pouchInventory in ((InventoryUI) Manager.ui.playerInventoryUI).pouchSlotsContainers)
							TryHighlightItemSlots(pouchInventory.itemSlots, objectsToHighlight);
					}

					if (Manager.ui.itemSlotsBar.isShowing)
						TryHighlightItemSlots(Manager.ui.itemSlotsBar.itemSlots, objectsToHighlight);
				}
			}

			private static void TryHighlightItemSlots(List<SlotUIBase> itemSlots, HashSet<ObjectID> objectsToHighlight) {
				foreach (var slot in itemSlots) {
					if (slot.highlightBorder == null || slot.icon == null || !slot.isShowing)
						continue;

					var highlightAnyItem = objectsToHighlight.Count > 0;
					var highlightThisItem = objectsToHighlight.Contains(slot.GetContainedObject().objectID);

					if (highlightAnyItem) {
						slot.highlightBorder.gameObject.SetActive(highlightThisItem);
						slot.icon.SetAlpha(highlightThisItem ? 1f : 0.2f);
					} else {
						slot.highlightBorder.gameObject.SetActive(false);
						slot.icon.SetAlpha(1f);
					}
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