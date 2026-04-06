using System.Collections.Generic;
using ItemBrowser.Utilities.Extensions;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;

namespace ItemBrowser.Common.Input {
	public static class InputAdder { 
		public static void AddKeybind(UserData userData, KeybindConfiguration config) {
			var newAction = new InputAction();
			newAction.SetValue("_id", config.Id);
			newAction.SetValue("_categoryId", config.CategoryId);
			newAction.SetValue("_name", $"ControlMapper/{config.Name ?? config.Id.ToString()}");
			newAction.SetValue("_type", InputActionType.Button);
			newAction.SetValue("_descriptiveName", $"ControlMapper/{config.Name ?? config.Id.ToString()}");
			newAction.SetValue("_userAssignable", true);

			userData.GetValue<List<InputAction>>("actions").Add(newAction);
			userData.GetValue<ActionCategoryMap>("actionCategoryMap").AddAction(newAction.categoryId, config.Id);
			
			if (config.KeyboardKey != null) {
				var keyboardActionElementMap = new ActionElementMap();
				keyboardActionElementMap.SetValue("_actionId", config.Id);
				keyboardActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				keyboardActionElementMap.SetValue("_actionCategoryId", newAction.categoryId);
				keyboardActionElementMap.SetValue("_keyboardKeyCode", config.KeyboardKey.Value);
				
				if (config.KeyboardKeyModifier != null)
					keyboardActionElementMap.SetValue("_modifierKey1", config.KeyboardKeyModifier.Value);
				
				if (!userData.HasKeyboardMapInCategory(config.InputCategoryId))
					userData.CreateKeyboardMap(config.InputCategoryId, 0);

				var keyboardMap = userData.GetKeyboardMap(config.InputCategoryId, 0);
				keyboardMap.actionElementMaps.Add(keyboardActionElementMap);
			}
			
			if (config.MouseElementId != null) {
				var mouseActionElementMap = new ActionElementMap();
				mouseActionElementMap.SetValue("_actionId", config.Id);
				mouseActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				mouseActionElementMap.SetValue("_actionCategoryId", newAction.categoryId);
				mouseActionElementMap.SetValue("_elementIdentifierId", config.MouseElementId.Value);

				if (!userData.HasMouseMapInCategory(config.InputCategoryId))
					userData.CreateMouseMap(config.InputCategoryId, 0);
				
				var mouseMap = userData.GetMouseMap(config.InputCategoryId, 0);
				mouseMap.actionElementMaps.Add(mouseActionElementMap);
			}
			
			if (config.JoystickElementId != null) {
				var joystickActionElementMap = new ActionElementMap();
				joystickActionElementMap.SetValue("_actionId", config.Id);
				joystickActionElementMap.SetValue("_elementType", ControllerElementType.Button);
				joystickActionElementMap.SetValue("_axisRange", AxisRange.Full);
				joystickActionElementMap.SetValue("_invert", false);
				joystickActionElementMap.SetValue("_actionCategoryId", newAction.categoryId);
				joystickActionElementMap.SetValue("_elementIdentifierId", config.JoystickElementId.Value);

				if (!userData.HasJoystickMapInCategory(default, config.InputCategoryId))
					userData.CreateJoystickMap(config.InputCategoryId, default, 0);
				
				var joystickMap = userData.GetJoystickMap(config.InputCategoryId, default, 0);
				joystickMap.actionElementMaps.Add(joystickActionElementMap);
			}
		}

		public record KeybindConfiguration {
			public readonly int Id;
			public readonly int CategoryId;
			public readonly int InputCategoryId;
			
			public string Name { get; set; }
			public KeyboardKeyCode? KeyboardKey { get; set; }
			public ModifierKey? KeyboardKeyModifier { get; set; }
			public int? MouseElementId { get; set; }
			public int? JoystickElementId { get; set; }

			public KeybindConfiguration(int id, int categoryId, int inputCategoryId) {
				Id = id;
				CategoryId = categoryId;
				InputCategoryId = inputCategoryId;
			}
		}
	}
}