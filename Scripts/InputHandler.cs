using System.Collections.Generic;
using HarmonyLib;
using ItemBrowser.Api;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities.Extensions;
using PlayerState;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser {
	[HarmonyPatch]
	internal static class InputHandler {
		private const PlayerInput.InputType ToggleBrowserInput = (PlayerInput.InputType) 39000;
		private const PlayerInput.InputType ShowSourcesInput = (PlayerInput.InputType) 39001;
		private const PlayerInput.InputType ShowUsagesInput = (PlayerInput.InputType) 39002;
		private const PlayerInput.InputType ShowTechnicalInfoInput = (PlayerInput.InputType) 39003;
		private const PlayerInput.InputType SpawnItemInput = (PlayerInput.InputType) 39004;
		private const PlayerInput.InputType ToggleTileGridInput = (PlayerInput.InputType) 39005;

		public static bool IsShowTechnicalInfoHeld => Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(ShowTechnicalInfoInput);
		public static bool IsSpawnItemPressed => Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(SpawnItemInput);
		public static bool IsPickUpTenHeld => Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_10);
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
					if (input.WasButtonPressedDownThisFrame(ShowSourcesInput))
						ItemBrowserAPI.ItemBrowserUI.ShowDetails(containedObjectData, DetailsTab.Sources);

					if (input.WasButtonPressedDownThisFrame(ShowUsagesInput))
						ItemBrowserAPI.ItemBrowserUI.ShowDetails(containedObjectData, DetailsTab.Usages);
				}
			}
		}
		
		[HarmonyPatch(typeof(InputManager), "Init")]
		[HarmonyPrefix]
		public static void InputManager_Init(InputManager __instance) {
			var inputManagerBase = Resources.Load<InputManager_Base>("Rewired Input Manager");
			var userData = inputManagerBase.userData;

			AddKeybind(userData, (int) ToggleBrowserInput, "ItemBrowser-ToggleBrowser", new DefaultKeybindData {
				KeyboardKey = KeyboardKeyCode.Z
			});
			AddKeybind(userData, (int) ShowSourcesInput, "ItemBrowser-ShowSources", new DefaultKeybindData {
				KeyboardKey = KeyboardKeyCode.O
			});
			AddKeybind(userData, (int) ShowUsagesInput, "ItemBrowser-ShowUsages", new DefaultKeybindData {
				KeyboardKey = KeyboardKeyCode.U
			});
			AddKeybind(userData, (int) SpawnItemInput, "ItemBrowser-SpawnItem", new DefaultKeybindData {
				MouseElementId = 5,
				JoystickElementId = 15
			});
			AddKeybind(userData, (int) ShowTechnicalInfoInput, "ItemBrowser-ShowTechnicalInfo", new DefaultKeybindData {
				KeyboardKey = KeyboardKeyCode.LeftShift,
				JoystickElementId = 14
			});
			AddKeybind(userData, (int) ToggleTileGridInput, "ItemBrowser-ToggleTileGrid", new DefaultKeybindData {
				KeyboardKey = KeyboardKeyCode.F6,
				KeyboardKeyModifier = ModifierKey.Shift
			});
		}

		private static void AddKeybind(UserData userData, int id, string name, DefaultKeybindData defaults) {
			const int gameplayCategoryId = 17;
			const int keyboardMapIndex = 5;
			const int mouseMapIndex = 5;
			const int joystickMapIndex = 11;
				
			var newAction = new InputAction();
			newAction.SetValue("_id", id);
			newAction.SetValue("_categoryId", gameplayCategoryId);
			newAction.SetValue("_name", $"ControlMapper/{name}");
			newAction.SetValue("_type", InputActionType.Button);
			newAction.SetValue("_descriptiveName", $"ControlMapper/{name}");
			newAction.SetValue("_userAssignable", true);

			userData.GetValue<List<InputAction>>("actions").Add(newAction);
			userData.GetValue<ActionCategoryMap>("actionCategoryMap").AddAction(gameplayCategoryId, id);

			if (defaults.KeyboardKey != null) {
				var keyboardMap = userData.GetValue<List<ControllerMap_Editor>>("keyboardMaps")[keyboardMapIndex];
				
				var keyboardActionElementMap = new ActionElementMap();
				keyboardActionElementMap.SetValue("_actionId", id);
				keyboardActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				keyboardActionElementMap.SetValue("_actionCategoryId", gameplayCategoryId);
				keyboardActionElementMap.SetValue("_keyboardKeyCode", defaults.KeyboardKey.Value);
				
				if (defaults.KeyboardKeyModifier != null)
					keyboardActionElementMap.SetValue("_modifierKey1", defaults.KeyboardKeyModifier.Value);
				
				keyboardMap.actionElementMaps.Add(keyboardActionElementMap);		
			}
			
			if (defaults.MouseElementId != null) {
				var mouseActionElementMap = new ActionElementMap();
				mouseActionElementMap.SetValue("_actionId", id);
				mouseActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				mouseActionElementMap.SetValue("_actionCategoryId", gameplayCategoryId);
				mouseActionElementMap.SetValue("_elementIdentifierId", defaults.MouseElementId.Value);
				
				if (!userData.HasMouseMapInCategory(gameplayCategoryId))
					userData.CreateMouseMap(gameplayCategoryId, 0);
				
				var mouseMap = userData.GetMouseMap(gameplayCategoryId, 0);
				mouseMap.actionElementMaps.Add(mouseActionElementMap);
			}
			
			if (defaults.JoystickElementId != null) {
				var joystickActionElementMap = new ActionElementMap();
				joystickActionElementMap.SetValue("_actionId", id);
				joystickActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				joystickActionElementMap.SetValue("_actionCategoryId", gameplayCategoryId);
				joystickActionElementMap.SetValue("_elementIdentifierId", defaults.JoystickElementId.Value);
				
				if (!userData.HasJoystickMapInCategory(default, gameplayCategoryId))
					userData.CreateJoystickMap(gameplayCategoryId, default, 0);
				
				var joystickMap = userData.GetJoystickMap(gameplayCategoryId, default, 0);
				joystickMap.actionElementMaps.Add(joystickActionElementMap);	
			}
		}

		private record DefaultKeybindData {
			public KeyboardKeyCode? KeyboardKey { get; set; }
			public ModifierKey? KeyboardKeyModifier { get; set; }
			public int? MouseElementId { get; set; }
			public int? JoystickElementId { get; set; }
		}
	}
}