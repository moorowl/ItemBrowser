using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using ItemBrowser.Utilities.Extensions;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using Rewired.UI.ControlMapper;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ItemBrowser {
	public class InputAdder {
		public static event Action<UserData> OnInit;

		private static readonly List<InputActionCategory> AddedCategories = new();
		private static readonly List<InputAction> AddedActions = new();
		
		public static void AddCategory(UserData userData, CategoryConfiguration configuration) {
			var category = new InputActionCategory();
			category.SetValue("_id", configuration.Id);
			category.SetValue("_name", configuration.Name);
			category.SetValue("_descriptiveName", configuration.Name);
			category.SetValue("_tag", configuration.Tag);

			userData.GetValue<List<InputActionCategory>>("actionCategories").Add(category);
			userData.GetValue<ActionCategoryMap>("actionCategoryMap").AddCategory(category.id);

			var mapCategory = new InputMapCategory();
			mapCategory.SetValue("_id", configuration.Id);
			mapCategory.SetValue("_name", configuration.Name);
			mapCategory.SetValue("_descriptiveName", configuration.Name);
			mapCategory.SetValue("_tag", configuration.Tag);
			
			userData.GetValue<List<InputMapCategory>>("mapCategories").Add(mapCategory);

			var spPlayer = userData.GetPlayer(1);
			var mapping = new Player_Editor.Mapping();
			mapping.SetValue("_categoryId", configuration.Id);
			mapping.SetValue("_enabled", true);

			spPlayer.GetValue<List<Player_Editor.Mapping>>("_defaultKeyboardMaps").Add(mapping);
			spPlayer.GetValue<List<Player_Editor.Mapping>>("_defaultJoystickMaps").Add(mapping);
			spPlayer.GetValue<List<Player_Editor.Mapping>>("_defaultMouseMaps").Add(mapping);
			
			var keyboardMaps = userData.GetValue<List<ControllerMap_Editor>>("keyboardMaps");
			keyboardMaps.Add(new ControllerMap_Editor {
				actionElementMaps = new List<ActionElementMap>(),
				categoryId = category.id,
				name = configuration.Name
			});
			
			var joystickMaps = userData.GetValue<List<ControllerMap_Editor>>("joystickMaps");
			joystickMaps.Add(new ControllerMap_Editor {
				actionElementMaps = new List<ActionElementMap>(),
				categoryId = category.id,
				name = configuration.Name
			});
			
			var mouseMaps = userData.GetValue<List<ControllerMap_Editor>>("mouseMaps");
			mouseMaps.Add(new ControllerMap_Editor {
				actionElementMaps = new List<ActionElementMap>(),
				categoryId = category.id,
				name = configuration.Name
			});
			
			AddedCategories.Add(category);
		}
		
		public static void AddAction(UserData userData, ActionConfiguration configuration) {
			var newAction = new InputAction();
			newAction.SetValue("_id", configuration.Id);
			newAction.SetValue("_categoryId", configuration.Category);
			newAction.SetValue("_name", configuration.Name);
			newAction.SetValue("_type", InputActionType.Button);
			newAction.SetValue("_descriptiveName", configuration.Name);
			newAction.SetValue("_userAssignable", true);

			userData.GetValue<List<InputAction>>("actions").Add(newAction);
			userData.GetValue<ActionCategoryMap>("actionCategoryMap").AddAction(configuration.Category, configuration.Id);

			if (configuration.DefaultKeyboardElement != null) {
				var keyboardMap = userData.GetValue<List<ControllerMap_Editor>>("keyboardMaps")?.FirstOrDefault(keyboardMap => keyboardMap.categoryId == configuration.Category);
				if (keyboardMap != null) {
					var keyboardActionElementMap = new ActionElementMap();
					keyboardActionElementMap.SetValue("_actionId", configuration.Id);
					keyboardActionElementMap.SetValue("_elementType", ControllerElementType.Button);
					keyboardActionElementMap.SetValue("_actionCategoryId", configuration.Category);
					keyboardActionElementMap.SetValue("_keyboardKeyCode", configuration.DefaultKeyboardElement.Value);
					keyboardActionElementMap.SetValue("_modifierKey1", configuration.DefaultKeyboardModifier);
				
					keyboardMap.actionElementMaps.Add(keyboardActionElementMap);		
				}
			}
			
			if (configuration.DefaultMouseElement != null) {
				var mouseMap = userData.GetValue<List<ControllerMap_Editor>>("mouseMaps")?.FirstOrDefault(mouseMap => mouseMap.categoryId == configuration.Category);
				if (mouseMap != null) {
					var mouseActionElementMap = new ActionElementMap();
					mouseActionElementMap.SetValue("_actionId", configuration.Id);
					mouseActionElementMap.SetValue("_elementType", ControllerElementType.Button);
					mouseActionElementMap.SetValue("_actionCategoryId", configuration.Category);
					mouseActionElementMap.SetValue("_elementIdentifierId", configuration.DefaultMouseElement.Value);
				
					mouseMap.actionElementMaps.Add(mouseActionElementMap);		
				}
			}
			
			if (configuration.DefaultControllerElement != null) {
				var joystickMap = userData.GetValue<List<ControllerMap_Editor>>("joystickMaps")?.FirstOrDefault(joystickMap => joystickMap.categoryId == configuration.Category);
				if (joystickMap != null) {
					var joystickActionElementMap = new ActionElementMap();
					joystickActionElementMap.SetValue("_actionId", configuration.Id);
					joystickActionElementMap.SetValue("_elementType", ControllerElementType.Button);
					joystickActionElementMap.SetValue("_actionCategoryId", configuration.Category);
					joystickActionElementMap.SetValue("_elementIdentifierId", configuration.DefaultControllerElement.Value);
					joystickActionElementMap.SetValue("_axisRange", AxisRange.Full);
					joystickActionElementMap.SetValue("_invert", false);
				
					joystickMap.actionElementMaps.Add(joystickActionElementMap);		
				}
			}
			
			AddedActions.Add(newAction);
		}
		
		private static bool TryGetElementIdByName(Controller controller, string elementName, out int elementId) {
			foreach (var element in controller.ElementIdentifiers) {
				if (element.name != elementName)
					continue;
				
				elementId = element.id;
				return true;
			}
			
			elementId = -1;
			return false;
		}
		
		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(InputManager), "Init")]
			[HarmonyPrefix]
			public static void InputManager_Init(InputManager __instance) {
				var inputManagerBase = Resources.Load<InputManager_Base>("Rewired Input Manager");
				var userData = inputManagerBase.userData;

				OnInit?.Invoke(userData);
			}
			
			[HarmonyPatch(typeof(ControlMappingMenu), "CreateCategorySelection")]
			[HarmonyPostfix]
			public static void ControlMappingMenu_CreateCategorySelection(ControlMappingMenu __instance) {
				var gameplayLayout = __instance.GetValue<List<ControlMapping_CategoryLayoutData>>("_mappingLayoutData")[1];
				var categories = gameplayLayout.GetValue<List<CategoryLayoutData>>("_categoryLayoutData");
            
				foreach (var category in AddedCategories) {
					var categoryLayout = new CategoryLayoutData();
					categoryLayout.SetValue("_showActionCategoryName", new[] { true });
					categoryLayout.SetValue("_showActionCategoryDescription", new[] { true });

					var mappingSet = new ControlMapper.MappingSet();
					mappingSet.SetValue("_mapCategoryId", category.id);
					mappingSet.SetValue("_actionIds", new[] { AddedActions.Count(action => action.categoryId == category.id) });
					mappingSet.SetValue("_actionCategoryIds", new[] { category.id });
					
					categoryLayout.SetValue("MappingSet", mappingSet);
					
					categories.Add(categoryLayout);
				}
			}
		}
		
		public class CategoryConfiguration {
			public readonly int Id;
			public readonly string Name;
			
			public string Tag { get; private set; }
			
			public CategoryConfiguration(int id, string name) {
				Id = id;
				Name = name;
				Tag = name;
			}

			public CategoryConfiguration SetTag(string tag) {
				Tag = tag;
				return this;
			}
		}
		
		public class ActionConfiguration {
			public readonly int Id;
			public readonly string Name;
			
			public int Category { get; private set; }
			public KeyboardKeyCode? DefaultKeyboardElement { get; private set; }
			public ModifierKey DefaultKeyboardModifier { get; private set; }
			public int? DefaultMouseElement { get; private set; }
			public int? DefaultControllerElement { get; private set; }
			
			public ActionConfiguration(int id, string name) {
				Id = id;
				Name = $"ControlMapper/{name}";
			}

			public ActionConfiguration SetCategory(int category) {
				Category = category;
				return this;
			}

			public ActionConfiguration SetDefaultKeyboardBinding(KeyboardKeyCode key, ModifierKey keyModifier = ModifierKey.None) {
				DefaultKeyboardElement = key;
				DefaultKeyboardModifier = keyModifier;
				return this;
			}
			
			public ActionConfiguration SetDefaultMouseBinding(int element) {
				DefaultMouseElement = element;
				return this;
			}
			
			public ActionConfiguration SetDefaultControllerBinding(int element) {
				DefaultControllerElement = element;
				return this;
			}
		}
	}
}