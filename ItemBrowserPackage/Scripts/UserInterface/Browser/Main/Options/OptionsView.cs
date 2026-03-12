using System.Collections.Generic;
using ItemBrowser.Api;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class OptionsView : MainSubView, IScrollable {
		private readonly OptionsEntryType _cheatMode = new() {
			CanBeClicked = () => ClientWorldStateSystem.IsAdminOrInCreative,
			OnLeftClick = () => {
				Options.Instance.CheatMode = !Options.Instance.CheatMode;
			},
			UpdateValueText = (valueText, canBeClicked) => {
				if (!canBeClicked)
					valueText.Render("ItemBrowser-Options/Unavailable");
				else
					valueText.Render($"ItemBrowser-Options/{(Options.Instance.CheatMode ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _clearFavorites = new() {
			CanBeClicked = () => Options.Instance.FavoritesCount >= 1,
			OnLeftClick = () => {
				Manager.menu.centerPopUpText.StartNewDisplaySequence(
					"ItemBrowser-Options/ClearFavoritesAreYouSure",
					null,
					menuInputCooldown: true,
					fadeTime: 0f,
					staticTime: 1.5f,
					useUnscaledTime: true,
					yPosition: 0f,
					textBackgroundAlpha: 1f,
					localize: true,
					TextManager.FontFace.boldMedium,
					response => {
						if (response.IsCancel)
							return;

						Options.Instance.RemoveAllFavorites();
			
						foreach (var itemSlot in API.Rendering.UICamera.transform.GetComponentsInChildren<ItemBrowserSlot>(true))
							itemSlot.OnFavoritedStateChanged();
					},
					options: new List<string> { "cancelDialogue", "yes" },
					minWidth: 10f,
					backgroundAlpha: 0.9f,
					pauseGame: false
				);
			}
		};
		private readonly OptionsEntryType _theme = new() {
			OnLeftClick = () => {
				ItemBrowserAPI.ItemBrowserUI.SwapToNextTheme();
			},
			OnRightClick = () => {
				ItemBrowserAPI.ItemBrowserUI.SwapToPreviousTheme();
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render(ItemBrowserAPI.ItemBrowserUI.CurrentTheme.Term);
			}
		};
		private readonly OptionsEntryType _showButtonHints = new() {
			OnLeftClick = () => {
				Options.Instance.ShowButtonHints = !Options.Instance.ShowButtonHints;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(Options.Instance.ShowButtonHints ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _showSourceMod = new() {
			OnLeftClick = () => {
				Options.Instance.ShowSourceMod = !Options.Instance.ShowSourceMod;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(Options.Instance.ShowSourceMod ? "Enabled" : "Disabled")}");
			}
		};
		
		public Transform scrollContainer;
		public OptionsEntry entryTemplate;
		public OptionsSection sectionTemplate;
		public GameObject dividerTemplate;

		private float _height;

		protected override void OnShow(bool isFirstTimeShowing) {
			if (isFirstTimeShowing)
				AddAllSettings();
		}

		private void AddAllSettings() {
			// General
			AddSection("ItemBrowser-Options/General");
			AddEntry("ItemBrowser-Options/CheatMode", _cheatMode);
			AddEntry("ItemBrowser-Options/ClearFavorites", _clearFavorites);

			// Appearance
			AddSection("ItemBrowser-Options/Appearance");
			AddEntry("ItemBrowser-Options/Theme", _theme);
			AddEntry("ItemBrowser-Options/ShowSourceMod", _showSourceMod);
			AddEntry("ItemBrowser-Options/ShowButtonHints", _showButtonHints);
		}

		private void AddSection(string term) {
			var section = Instantiate(sectionTemplate, scrollContainer);
			section.gameObject.SetActive(true);
			section.SetTerm(term);
			var sectionHeight = UserInterfaceUtils.CalculateHeight(section);
			
			_height -= sectionHeight / 2f;
			section.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= sectionHeight / 2f;
			
			AddDivider();
		}

		private void AddEntry(string term, OptionsEntryType type) {
			var entry = Instantiate(entryTemplate, scrollContainer);
			entry.SetNameAndType(term, type);
			entry.gameObject.SetActive(true);
			var entryHeight = UserInterfaceUtils.CalculateHeight(entry);
			
			_height -= entryHeight / 2f;
			entry.transform.localPosition = new Vector3(entry.transform.localPosition.x, _height, 0f);
			_height -= entryHeight / 2f;
			
			AddDivider();
		}
		
		private void AddDivider() {
			var divider = Instantiate(dividerTemplate, scrollContainer);
			divider.gameObject.SetActive(true);
			var dividerHeight = UserInterfaceUtils.CalculateHeight(divider);
			
			_height -= dividerHeight / 2f;
			divider.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= dividerHeight / 2f;
		}
		
		public void UpdateContainingElements(float scroll) { }

		public bool IsBottomElementSelected() {
			return false;
		}

		public bool IsTopElementSelected() {
			return false;
		}

		public float GetCurrentWindowHeight() {
			return Mathf.Abs(_height);
		}
	}
}