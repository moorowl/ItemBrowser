using System;
using System.Collections.Generic;
using HarmonyLib;
using ItemBrowser.Api;
using ItemBrowser.Utilities.Extensions;
using ItemBrowser.Api.Entries;
using PlayerState;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser {
	[HarmonyPatch]
	internal static class InputHandler {
		private const int ModCategory = 39000;
		private const PlayerInput.InputType ToggleBrowserInput = (PlayerInput.InputType) 39000;
		private const PlayerInput.InputType ShowSourcesInput = (PlayerInput.InputType) 39001;
		private const PlayerInput.InputType ShowUsagesInput = (PlayerInput.InputType) 39002;
		private const PlayerInput.InputType ShowTechnicalInfoInput = (PlayerInput.InputType) 39003;
		private const PlayerInput.InputType SpawnItemInput = (PlayerInput.InputType) 39004;
		private const PlayerInput.InputType ToggleTileGridInput = (PlayerInput.InputType) 39005;

		public static bool IsShowTechnicalInfoHeld => Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(ShowTechnicalInfoInput);
		public static bool IsSpawnItemPressed => Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(SpawnItemInput);
		public static bool IsPickUpTenHeld => Manager.input.singleplayerInputModule.IsButtonCurrentlyDown(PlayerInput.InputType.PICK_UP_10);
		public static bool IsToggleTileGridPressed => Manager.input.singleplayerInputModule.WasButtonPressedDownThisFrame(ToggleTileGridInput);
		
		[HarmonyPatch(typeof(InputManager), "LateUpdate")]
		[HarmonyPostfix]
		public static void InputManager_LateUpdate(InputManager __instance) {
			var player = Manager.main.player;
			
			if (player == null || player.guestMode || ItemBrowserAPI.ItemBrowserUI == null || Time.timeScale == 0f || EntityUtility.GetComponentData<PlayerStateCD>(player.entity, player.world).isStateLocked || !Manager.main.currentSceneHandler.isSceneHandlerReady)
				return;
			
			var input = Manager.input.singleplayerInputModule;
			if (input.WasButtonPressedDownThisFrame(ToggleBrowserInput))
				ItemBrowserAPI.ItemBrowserUI.IsShowing = !ItemBrowserAPI.ItemBrowserUI.IsShowing;

			if (Manager.ui.currentSelectedUIElement is SlotUIBase slot) {
				var containedObjectData = slot.GetContainedObject().objectData;
				if (containedObjectData.objectID != ObjectID.None) {
					if (input.WasButtonPressedDownThisFrame(ShowSourcesInput))
						ItemBrowserAPI.ItemBrowserUI.ShowObjectEntries(containedObjectData, ObjectEntryType.Source);

					if (input.WasButtonPressedDownThisFrame(ShowUsagesInput))
						ItemBrowserAPI.ItemBrowserUI.ShowObjectEntries(containedObjectData, ObjectEntryType.Usage);
				}
			}
		}
		
		[HarmonyPatch(typeof(UserData), "yDABbxiARLBWAQcRokAdOcDrDbkT")]
		[HarmonyPrefix]
		public static void OnRewiredDataInit(UserData __instance) {
			InputAdder.AddCategory(__instance, new InputAdder.CategoryConfiguration(ModCategory, "ItemBrowser:Browser")
				.SetTag("gameplay")
			);
			
			InputAdder.AddAction(__instance, new InputAdder.ActionConfiguration((int) ToggleBrowserInput, "ItemBrowser:ToggleBrowser")
				.SetCategory(ModCategory)
				.SetDefaultKeyboardBinding(KeyboardKeyCode.Z)
			);
			InputAdder.AddAction(__instance, new InputAdder.ActionConfiguration((int) ShowSourcesInput, "ItemBrowser:ShowSources")
				.SetCategory(ModCategory)
				.SetDefaultKeyboardBinding(KeyboardKeyCode.O)
			);
			InputAdder.AddAction(__instance, new InputAdder.ActionConfiguration((int) ShowUsagesInput, "ItemBrowser:ShowUsages")
				.SetCategory(ModCategory)
				.SetDefaultKeyboardBinding(KeyboardKeyCode.U)
			);
			InputAdder.AddAction(__instance, new InputAdder.ActionConfiguration((int) SpawnItemInput, "ItemBrowser:SpawnItem")
				.SetCategory(ModCategory)
				.SetDefaultMouseBinding(5)
				.SetDefaultControllerBinding(15)
			);
			InputAdder.AddAction(__instance, new InputAdder.ActionConfiguration((int) ShowTechnicalInfoInput, "ItemBrowser:ShowTechnicalInfo")
				.SetCategory(ModCategory)
				.SetDefaultKeyboardBinding(KeyboardKeyCode.LeftShift)
				.SetDefaultControllerBinding(14)
			);
			InputAdder.AddAction(__instance, new InputAdder.ActionConfiguration((int) ToggleTileGridInput, "ItemBrowser:ToggleTileGrid")
				.SetCategory(ModCategory)
				.SetDefaultKeyboardBinding(KeyboardKeyCode.F6, ModifierKey.Shift)
			);
		}
	}
}