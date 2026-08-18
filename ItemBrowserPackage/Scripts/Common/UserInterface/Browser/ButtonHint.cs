using System.Text;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ButtonHint {
		public static readonly ButtonHint AddFavorite = new("ItemBrowser-ButtonHints/AddFavorite",
			new[] { "ToggleLocking" }
		);
		public static readonly ButtonHint RemoveFavorite = new("ItemBrowser-ButtonHints/RemoveFavorite",
			new[] { "ToggleLocking" }
		);
		public static readonly ButtonHint AddCollected = new("ItemBrowser-ButtonHints/AddCollected",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint RemoveCollected = new("ItemBrowser-ButtonHints/RemoveCollected",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint DiscoverTemporarily = new("ItemBrowser-ButtonHints/DiscoverTemporarily",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint ExcludeFilter = new("ItemBrowser-ButtonHints/ExcludeFilter",
			new[] { "UISecondInteract" }
		);
		public static readonly ButtonHint IncludeFilter = new("ItemBrowser-ButtonHints/IncludeFilter",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint RemoveFilterPrimary = new("ItemBrowser-ButtonHints/RemoveFilter",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint RemoveFilterSecondary = new("ItemBrowser-ButtonHints/RemoveFilter",
			new[] { "UISecondInteract" }
		);
		public static readonly ButtonHint Give = new("ItemBrowser-ButtonHints/Give",
			new[] { "ControlMapper/ItemBrowser-SpawnItem" }
		);
		public static readonly ButtonHint GoBack = new("ItemBrowser-ButtonHints/GoBack",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint RestoreDefaults = new("ItemBrowser-ButtonHints/RestoreDefaults",
			new[] { "UISecondInteract" }
		);
		public static readonly ButtonHint ViewSource = new("ItemBrowser-ButtonHints/ViewSource",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint ViewUsagePrimary = new("ItemBrowser-ButtonHints/ViewUsage",
			new[] { "UIInteract" }
		);
		public static readonly ButtonHint ViewUsageSecondary = new("ItemBrowser-ButtonHints/ViewUsage",
			new[] { "UISecondInteract" }
		);
		public static readonly ButtonHint CycleTabLeft = new("ItemBrowser-ButtonHints/CycleTabLeft",
			new[] { "ZoomOutMap" }
		);
		public static readonly ButtonHint CycleTabRight = new("ItemBrowser-ButtonHints/CycleTabRight",
			new[] { "ZoomInMap" }
		);
		public static readonly ButtonHint CycleCategoryLeft = new("ItemBrowser-ButtonHints/CycleCategoryLeft",
			new[] { "ZoomOutMap" }
		);
		public static readonly ButtonHint CycleCategoryRight = new("ItemBrowser-ButtonHints/CycleCategoryRight",
			new[] { "ZoomInMap" }
		);
		public static readonly ButtonHint CycleSourceLeft = new("ItemBrowser-ButtonHints/CycleSourceLeft",
			new[] { "MapPreviousMarker" }
		);
		public static readonly ButtonHint CycleSourceRight = new("ItemBrowser-ButtonHints/CycleSourceRight",
			new[] { "MapNextMarker" }
		);
        public static readonly ButtonHint SearchClear = new("ItemBrowser-ButtonHints/SearchClear",
			new[] { "UISecondInteract" }
		);
        public static readonly ButtonHint SearchHighlight = new("ItemBrowser-ButtonHints/SearchHighlight",
			new[] { "UIInteract", "UIInteract" }
		);
		public static readonly ButtonHint ToggleBrowser = new("ItemBrowser-ButtonHints/ToggleBrowser",
			new[] { "ControlMapper/ItemBrowser-ToggleBrowser" }
		);
		
		private readonly string _name;
		private readonly string[][] _bindings;

		public ButtonHint(string name, params string[][] bindings) {
			_name = name;
			_bindings = bindings;
		}

		public string GetLocalizedDescription(object[] formatFields = null) {
			var builder = new StringBuilder();

			var name = API.Localization.GetLocalizedTerm(_name) ?? _name;
			if (formatFields is { Length: >= 1 })
				builder.AppendFormat(name, formatFields);
			else
				builder.Append(name);
			
			builder.Append(": ");

			for (var i = 0; i < _bindings.Length; i++) {
				if (i >= 1)
					builder.Append("/");

				for (var j = 0; j < _bindings[i].Length; j++) {
					if (j >= 1)
						builder.Append("+");
					
					var glyph = GetInputGlyph(_bindings[i][j]);
					builder.Append(glyph ?? "?");
				}
			}

			return builder.ToString();
		}
		
		private static string GetInputGlyph(string binding) {
			var prefersJoystick = Manager.input.IsAnyGamepadConnected() && !UserInterfaceUtility.IsUsingMouseOrKeyboard;
			return prefersJoystick ? Manager.ui.GetShortCutString(binding, true, true) : Manager.ui.GetShortCutString(binding, false);
		}
	}
}