using System;
using HarmonyLib;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.UserInterface.Browser {
	public class ItemBrowserUI : ItemBrowserWindow {
		public static event Action<ItemBrowserUI> OnInit;
		public static event Action<ItemBrowserUI> OnUninit;

		[SerializeField]
		private Transform windows;
		[SerializeField]
		private GameObject tileGridPrefab;
		[SerializeField]
		private ObjectListContainerWindow objectListContainerWindow;
		[SerializeField]
		private ObjectEntriesWindow objectEntriesWindow;

		private bool _entriesOpenedOutsideOfBrowser;
		private GameObject _tileGrid;
		private Transform _tileGridRenderAnchor;

		private void Awake() {
			gameObject.SetActive(false);
			OnInit?.Invoke(this);

			_tileGridRenderAnchor = Manager.camera.GetRenderAnchor();
			_tileGrid = Instantiate(tileGridPrefab, _tileGridRenderAnchor);
		}
		
		private void OnDestroy() {
			OnUninit?.Invoke(this);
			
			if (_tileGrid != null) {
				Destroy(_tileGrid);
				Manager.camera.ReturnRenderAnchor(_tileGridRenderAnchor);
			}
		}

		protected override void OnShow(bool isFirstTimeShowing) {
			Manager.ui.DeselectAnySelectedUIElement();
			
			if (isFirstTimeShowing) {
				objectListContainerWindow.IsShowing = true;
				objectListContainerWindow.SetItemsTab();
				objectEntriesWindow.IsShowing = false;
			}
			
			UpdateScale();
			HideMapIfShowing();
			PlayToggleSound();
			
			Manager.input.SetActiveInputField(null);
		}

		protected override void OnHide() {
			PlayToggleSound();
			_entriesOpenedOutsideOfBrowser = false;
		}

		public bool ShowObjectEntries(ObjectDataCD objectData, ObjectEntryType type) {
			if (!objectEntriesWindow.PushObjectData(objectData, type, false)) {
				UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.NoSourcesOrUsages, this);
				return false;
			}

			_entriesOpenedOutsideOfBrowser = !IsShowing;

			IsShowing = true;
			objectListContainerWindow.IsShowing = false;
			objectEntriesWindow.IsShowing = true;
			
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericOpen, this);

			return true;
		}
		
		public void ShowObjectList() {
			IsShowing = true;
			objectListContainerWindow.IsShowing = true;
			objectEntriesWindow.IsShowing = false;
			objectEntriesWindow.Clear();
			_entriesOpenedOutsideOfBrowser = false;
		}

		public void GoBack() {
			if (Manager.input.textInputIsActive) {
				Manager.input.activeInputField.Deactivate(true);
			} else if (objectEntriesWindow.HasAnyHistory) {
				objectEntriesWindow.PopObjectData();
				UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericClose, this);
			} else if (objectEntriesWindow.IsShowing) {
				ShowObjectList();
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

			if (Manager.main.player != null && Manager.main.player.guestMode)
				IsShowing = false;
		}

		private void UpdateScale() {
			windows.localScale = Manager.ui.CalcGameplayUITargetScaleMultiplier();
		}
		
		private void UpdateGoBack() {
			if (Manager.menu.IsAnyMenuActive() || Manager.input.textInputIsActive)
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
			
			/*[HarmonyPatch(typeof(InGameButtonHintsUI), "LateUpdate")]
			[HarmonyPrefix]
			private static bool InGameButtonHintsUI_LateUpdate(InGameButtonHintsUI __instance) {
				// Hide button hints in bottom right
				if (ItemBrowserAPI.ItemBrowserUI != null && !ItemBrowserAPI.ItemBrowserUI.IsShowing)
					return true;
				
				__instance.container.SetActive(false);
				return false;
			}*/
			
			[HarmonyPatch(typeof(UIManager), "get_isAnyInventoryShowing")]
			[HarmonyPostfix]
			private static void UIManager_get_isAnyInventoryShowing(UIManager __instance, ref bool __result) {
				if (ItemBrowserAPI.ItemBrowserUI != null && ItemBrowserAPI.ItemBrowserUI.IsShowing)
					__result = true;
			}
		}
	}
}