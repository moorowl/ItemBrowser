using System.Collections.Generic;
using System.Text;
using I2.Loc;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class UserInterfaceUtils {
		public static bool IsUsingMouse => Manager.input.SystemIsUsingMouse();
		public static bool IsUsingKeyboard => Manager.input.SystemIsUsingKeyboard();
		public static bool IsUsingMouseAndKeyboard => Manager.input.SystemPrefersKeyboardAndMouse();
		public static bool IsUsingMouseOrKeyboard => IsUsingMouse || IsUsingKeyboard;

		public const float DescriptionPadding = 0.125f;
		public static Color DescriptionColor => Manager.text.GetRarityColor(Rarity.Poor);
		
		public static string GetInputGlyph(string binding) {
			var prefersJoystick = Manager.input.IsAnyGamepadConnected() && !IsUsingMouseOrKeyboard;
			return prefersJoystick ? Manager.ui.GetShortCutString(binding, true, true) : Manager.ui.GetShortCutString(binding, false);
		}

		public static void AppendButtonHint(List<TextAndFormatFields> lines, string term, string binding) {
			if (!Options.Instance.ShowButtonHints)
				return;
			
			var glyph = GetInputGlyph(binding);
			if (glyph == null)
				return;
			
			if (lines.Count > 0)
				lines[^1].paddingBeneath = DescriptionPadding;
			
			lines.Add(new TextAndFormatFields {
				text = term,
				formatFields = new[] {
					glyph
				},
				dontLocalizeFormatFields = true,
				color = Color.white * 0.95f
			});
		}

		public static string FormatChance(float chance) {
			return chance switch {
				< 0.0001f => (chance * 100f).ToString("0.####"),
				< 0.001f => (chance * 100f).ToString("0.###"),
				_ => (chance * 100f).ToString("0.##")
			};
		}

		public static string FormatRange((int Min, int Max) amount) {
			return amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString();
		}
		
		public static string FormatDuration(float duration) {
			return duration.ToString("F0");
		}

		public static void SelectAndMoveMouseTo(UIelement element) {
			Manager.ui.DeselectAnySelectedUIElement();
			element.Select();
			Manager.ui.mouse.PlaceMousePositionOnSelectedUIElementWhenControlledByJoystick();
		}
		
		public static float CalculateHeight(GameObject gameObject) {
			var height = 0f;

			foreach (var boxCollider in gameObject.GetComponentsInChildren<BoxCollider>())
				height = Mathf.Max(height, Mathf.Abs(boxCollider.transform.localPosition.y) + Mathf.Abs(boxCollider.size.y));
			
			foreach (var pugText in gameObject.GetComponentsInChildren<PugText>())
				height = Mathf.Max(height, pugText.dimensions.height - Mathf.Abs((pugText.dimensions.y + pugText.transform.localPosition.y) / 8f));

			return RoundToPixelPerfectPosition.RoundFloat(height);
		}
		
		public static float CalculateHeight(Component component) {
			return CalculateHeight(component.gameObject);
		}

		public static Material GetUISpriteColorReplaceMaterial() {
			return new Material(Shader.Find("Amplify/UISpriteColorReplace"));
		}

		public enum MenuSound {
			GenericOpen,
			GenericClose,
			ChangeTabOrCategory,
			AddObjectToInventory,
			Favorite,
			Unfavorite,
			ToggleBrowser,
			NoSourcesOrUsages
		}

		public static void PlaySound(MenuSound sound, MonoBehaviour source) {
			if (Manager.load.IsScreenBlack())
				return;
			
			switch (sound) {
				case MenuSound.GenericOpen:
					AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.6f, false, 1f, 0f);
					break;
				case MenuSound.GenericClose:
					AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f, false, 1f, 0f);
					break;
				case MenuSound.ChangeTabOrCategory:
					AudioManager.Sfx(SfxTableID.inventorySFXCreativeModeCategory, source.transform.position);
					break;
				case MenuSound.AddObjectToInventory:
					AudioManager.Sfx(SfxID.twitch, source.transform.position, 0.1f, 0.55f, 0.1f, true);
					break;
				case MenuSound.Favorite:
					AudioManager.Sfx(SfxTableID.inventorySFXSlotUnlock, source.transform.position);
					break;
				case MenuSound.Unfavorite:
					AudioManager.Sfx(SfxTableID.inventorySFXSlotLock, source.transform.position);
					break;
				case MenuSound.ToggleBrowser:
					AudioManager.Sfx(SfxTableID.inventorySFXInfoTab, Manager.main.player.transform.position);
					break;
				case MenuSound.NoSourcesOrUsages:
					AudioManager.SfxUI(SfxID.menu_denied, 1.15f, false, 0.4f, 0.05f);
					break;
			}
		}
	}
}