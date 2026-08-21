using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class UserInterfaceUtility {
		public static bool IsUsingMouse => Manager.input.SystemIsUsingMouse();
		public static bool IsUsingKeyboard => Manager.input.SystemIsUsingKeyboard();
		public static bool IsUsingMouseAndKeyboard => Manager.input.SystemPrefersKeyboardAndMouse();
		public static bool IsUsingMouseOrKeyboard => IsUsingMouse || IsUsingKeyboard;

		public const float DescriptionPadding = 0.125f;
		public static Color DescriptionColor => Manager.text.GetRarityColor(Rarity.Poor);
		public static Color AlmostWhiteColor = Color.white * 0.99f;

		private static readonly MemberInfo RuntimeGradientCacheMember = typeof(UIManager).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "s_runtimeGradientCache");
		private static readonly int GradientMap = Shader.PropertyToID("_GradientMap");
		
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
				height = Mathf.Max(height, pugText.GetUIComponentRenderHeight());

			return RoundToPixelPerfectPosition.RoundFloat(height);
		}
		
		public static float CalculateHeight(Component component) {
			return CalculateHeight(component.gameObject);
		}

		public static Material GetUISpriteColorReplaceMaterial() {
			return new Material(Shader.Find("Amplify/UISpriteColorReplace"));
		}

		public static void ApplyObjectIconTransform(SpriteRenderer sr, ObjectInfo objectInfo, float desiredScale) {
			sr.transform.localPosition = objectInfo.iconOffset;
			
			var iconSize = new Vector3(desiredScale, desiredScale, 0f);
			if (sr.sprite.bounds.size is { x: > 1f, y: > 1f } && !ObjectUtility.IsCarriedObject(objectInfo.objectType))
				iconSize = sr.sprite.bounds.size;

			var scaleMin = Mathf.Min(desiredScale / iconSize.x, desiredScale / iconSize.y);
			sr.transform.localScale = new Vector3(scaleMin, scaleMin, 1f);
		}

		public static string TruncateToFit(string text, int maxCharacters) {
			if (text.Length <= maxCharacters)
				return text;
			
			return text[..(maxCharacters - 3)].TrimEnd() + "...";
		}

		public static void ApplyPetGradientMapBasedOnVariation(ContainedObjectsBuffer containedObject, SpriteRenderer sr) {
			if (!PugDatabase.HasComponent<PetCD>(containedObject.objectID))
				return;
			
			var petSkinInfo = Manager.ui.petInfosTable.GetPetSkinInfo(containedObject.objectID);
			var skinToUse = containedObject.variation;

			if (petSkinInfo == null || skinToUse < 0 || skinToUse >= petSkinInfo.skins.Count)
				return;

			var primaryGradientMap = petSkinInfo.skins[skinToUse].primaryGradientMap;
			var runtimeGradientCache = (Dictionary<GradientMapDataBlock, Texture2D>) API.Reflection.GetValue(RuntimeGradientCacheMember, Manager.ui);

			if (primaryGradientMap == null || !primaryGradientMap.hasData)
				return;

			if (!runtimeGradientCache.TryGetValue(primaryGradientMap, out var gradientMapTexture) || gradientMapTexture == null) {
				gradientMapTexture = new Texture2D(primaryGradientMap.textureWidth, 1, TextureFormat.ARGB32, mipChain: false);
				var array = new Color32[gradientMapTexture.width];
				for (var i = 0; i < gradientMapTexture.width; i++)
					array[i] = primaryGradientMap.GetPixel(i);
	
				gradientMapTexture.SetPixels32(array);
				gradientMapTexture.Apply();
							
				runtimeGradientCache[primaryGradientMap] = gradientMapTexture;
			}
						
			if (gradientMapTexture != null) {
				sr.material.EnableKeyword("USE_GRADIENT_MAP");
				sr.material.SetTexture(GradientMap, gradientMapTexture);
			}
		}

		public static void OpenSkillPage(SkillID skill) {
			ItemBrowserAPI.ItemBrowserUI.IsShowing = false;

			if (!Manager.ui.characterWindow.isShowing) {
				Manager.ui.HideAllInventoryAndCraftingUI();
				Manager.ui.OnPlayerInventoryOpen();
			}
			
			if (!Manager.ui.characterWindow.isShowing)
				Manager.ui.characterWindow.Show();
			Manager.ui.characterWindow.ShowSkillsWindow();

			var talentTreeUI = Manager.ui.characterWindow.skillsWindow.GetComponentInChildren<SkillTalentTreeUI>();
			talentTreeUI.root.SetActive(false);
			talentTreeUI.ToggleTalentTree(skill);
		}
	}
}