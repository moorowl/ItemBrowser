using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class UserInterfaceUtils {
		public static bool IsUsingMouse => Manager.input.SystemIsUsingMouse();
		public static bool IsUsingKeyboard => Manager.input.SystemIsUsingKeyboard();
		public static bool IsUsingMouseAndKeyboard => Manager.input.SystemPrefersKeyboardAndMouse();
		public static bool IsUsingMouseOrKeyboard => IsUsingMouse || IsUsingKeyboard;
		
		public static Color DescriptionColor => Manager.text.GetRarityColor(Rarity.Poor);
		
		public static string GetInputGlyph(string binding) {
			var prefersJoystick = Manager.input.IsAnyGamepadConnected() && !IsUsingMouseOrKeyboard;
			return prefersJoystick ? Manager.ui.GetShortCutString(binding, true, true) : Manager.ui.GetShortCutString(binding, false);
		}

		public static void AppendButtonHint(List<TextAndFormatFields> lines, string term, string binding) {
			var glyph = GetInputGlyph(binding);
			if (glyph == null)
				return;
			
			if (lines.Count > 0)
				lines[^1].paddingBeneath = 0.125f;
			
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
			if (chance < 0.001f)
				return (chance * 100f).ToString("0.###");
				
			return (chance * 100f).ToString("0.##");
		}

		public static string FormatAmountOrRollsRange((int Min, int Max) amount) {
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

		public enum MenuSound {
			GenericOpen,
			GenericClose,
			ChangeTypeOrCategory,
			AddObjectToInventory,
			Favorite,
			Unfavorite,
			ToggleBrowser,
			NoSourcesOrUsages
		}

		public static void PlaySound(MenuSound sound, MonoBehaviour source) {
			switch (sound) {
				case MenuSound.GenericOpen:
					AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.6f, false, 1f, 0f);
					break;
				case MenuSound.GenericClose:
					AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f, false, 1f, 0f);
					break;
				case MenuSound.ChangeTypeOrCategory:
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