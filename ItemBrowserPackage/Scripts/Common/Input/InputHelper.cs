using HarmonyLib;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using PlayerState;
using Rewired;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.Input {
	[HarmonyPatch]
	internal static class InputHelper {
		private const PlayerInput.InputType ToggleBrowserInput = (PlayerInput.InputType) 39000;
		private const PlayerInput.InputType ShowSourcesInput = (PlayerInput.InputType) 39001;
		private const PlayerInput.InputType ShowUsagesInput = (PlayerInput.InputType) 39002;
		private const PlayerInput.InputType SpawnItemInput = (PlayerInput.InputType) 39004;
		private const PlayerInput.InputType ToggleTileGridInput = (PlayerInput.InputType) 39005;
		private const PlayerInput.InputType ShowTechnicalInfoInput = (PlayerInput.InputType) 39006;

		public static bool IsShowTechnicalInfoHeld => OptionsManager.Instance.AlwaysShowTechnicalInfo || Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(ShowTechnicalInfoInput);
		public static bool IsSpawnItemPressed => Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(SpawnItemInput);
		public static bool IsPickUpTenHeld => Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_10);
		public static bool IsPickUpStackHeld => Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_HALF);
		public static bool IsToggleFavoritePressed => Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.LOCKING_TOGGLE);
		public static bool IsToggleTileGridPressed => Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(ToggleTileGridInput);
		
		[HarmonyPatch(typeof(InputManager), "LateUpdate")]
		[HarmonyPostfix]
		public static void InputManager_LateUpdate(InputManager __instance) {
			var player = Manager.main.player;
			
			if (player == null || ItemBrowserAPI.ItemBrowserUI == null || Time.timeScale == 0f || player.guestMode || player.instrumentHandler.IsPlayingInstrument || EntityUtility.GetComponentData<PlayerStateCD>(player.entity, player.world).isStateLocked || !Manager.main.currentSceneHandler.isSceneHandlerReady)
				return;
			
			var input = Manager.input.singleplayerInputModule;
			if (input.WasButtonPressedDownThisFrame(ToggleBrowserInput))
				ItemBrowserAPI.ItemBrowserUI.IsShowing = !ItemBrowserAPI.ItemBrowserUI.IsShowing;

			if (Manager.ui.currentSelectedUIElement is SlotUIBase slot) {
				var containedObjectData = slot.GetContainedObject().objectData;
				if (containedObjectData.objectID != ObjectID.None) {
					if (input.WasButtonPressedDownThisFrame(ShowSourcesInput) && !ItemBrowserAPI.ItemBrowserUI.ShowDetails(containedObjectData, DetailsTab.Sources))
						UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.NoSourcesOrUsages, slot);

					if (input.WasButtonPressedDownThisFrame(ShowUsagesInput) && !ItemBrowserAPI.ItemBrowserUI.ShowDetails(containedObjectData, DetailsTab.Usages))
						UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.NoSourcesOrUsages, slot);
				}
			}
		}
		
		[HarmonyPatch(typeof(InputManager), "Init")]
		[HarmonyPrefix]
		public static void InputManager_Init(InputManager __instance) {
			var inputManagerBase = Resources.Load<InputManager_Base>("Rewired Input Manager");
			var userData = inputManagerBase.userData;
			
			const int gameplayCategoryId = 17;
			const int gameplayInputCategoryId = 13;
			
			InputAdder.AddKeybind(userData, new InputAdder.KeybindConfiguration((int) ToggleBrowserInput, gameplayCategoryId, gameplayInputCategoryId) {
				Name = "ItemBrowser-ToggleBrowser",
				KeyboardKey = KeyboardKeyCode.Z
			});
			InputAdder.AddKeybind(userData, new InputAdder.KeybindConfiguration((int) ShowSourcesInput, gameplayCategoryId, gameplayInputCategoryId) {
				Name = "ItemBrowser-ShowSources",
				KeyboardKey = KeyboardKeyCode.O
			});
			InputAdder.AddKeybind(userData, new InputAdder.KeybindConfiguration((int) ShowUsagesInput, gameplayCategoryId, gameplayInputCategoryId) {
				Name = "ItemBrowser-ShowUsages",
				KeyboardKey = KeyboardKeyCode.U
			});
			InputAdder.AddKeybind(userData, new InputAdder.KeybindConfiguration((int) SpawnItemInput, gameplayCategoryId, gameplayInputCategoryId) {
				Name = "ItemBrowser-SpawnItem",
				MouseElementId = 5,
				JoystickElementId = 15
			});
			InputAdder.AddKeybind(userData, new InputAdder.KeybindConfiguration((int) ShowTechnicalInfoInput, gameplayCategoryId, gameplayInputCategoryId) {
				Name = "ItemBrowser-ShowTechnicalInfo",
				KeyboardKey = KeyboardKeyCode.LeftAlt,
				JoystickElementId = 14
			});
			InputAdder.AddKeybind(userData, new InputAdder.KeybindConfiguration((int) ToggleTileGridInput, gameplayCategoryId, gameplayInputCategoryId) {
				Name = "ItemBrowser-ToggleTileGrid",
				KeyboardKey = KeyboardKeyCode.F6,
				KeyboardKeyModifier = ModifierKey.Shift
			});
		}
	}
}