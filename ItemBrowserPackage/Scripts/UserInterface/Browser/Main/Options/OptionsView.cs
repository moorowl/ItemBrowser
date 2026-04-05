using System.Collections.Generic;
using ItemBrowser.Api;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class OptionsView : MainSubView, IScrollable {
		public UIScrollWindow scrollWindow;
		
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
		private readonly OptionsEntryType _panelsShiftLayout = new() {
			OnLeftClick = () => {
				Options.Instance.PanelsShiftLayout = !Options.Instance.PanelsShiftLayout;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(Options.Instance.PanelsShiftLayout ? "Enabled" : "Disabled")}");
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
		private readonly OptionsEntryType _showTechnicalInfo = new() {
			OnLeftClick = () => {
				Options.Instance.AlwaysShowTechnicalInfo = !Options.Instance.AlwaysShowTechnicalInfo;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(Options.Instance.AlwaysShowTechnicalInfo ? "Always" : "KeyHeld")}");
			}
		};
		private readonly OptionsEntryType _showTileGrid = new() {
			OnLeftClick = () => {
				ItemBrowserAPI.ItemBrowserUI.TileGrid.IsShowing = !ItemBrowserAPI.ItemBrowserUI.TileGrid.IsShowing;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(ItemBrowserAPI.ItemBrowserUI.TileGrid.IsShowing ? "Enabled" : "Disabled")}");
			}
		};
		
		public Transform scrollContainer;
		public OptionsEntry entryTemplate;
		public OptionsSection sectionTemplate;
		public GameObject dividerTemplate;

		private float _height;
		private OptionsEntry _firstEntry;

		protected override void OnShow(bool isFirstTimeShowing) {
			if (isFirstTimeShowing)
				AddAllSettings();
			
			TrySelectFirstEntry();
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			if (Manager.ui.currentSelectedUIElement == null || Manager.ui.currentSelectedUIElement is BlockingUIElement)
				TrySelectFirstEntry();
		}

		private void TrySelectFirstEntry() {
			if (_firstEntry != null && !UserInterfaceUtils.IsUsingMouseAndKeyboard)
				UserInterfaceUtils.SelectAndMoveMouseTo(_firstEntry);
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
			AddEntry("ItemBrowser-Options/ShowTechnicalInfo", _showTechnicalInfo);
			AddEntry("ItemBrowser-Options/PanelsShiftLayout", _panelsShiftLayout);
			
			// Extras
			AddSection("ItemBrowser-Options/Extras");
			AddEntry("ItemBrowser-Options/ShowTileGrid", _showTileGrid);
			
			foreach (var scrollItem in scrollContainer.GetComponentsInChildren<IScrollItem>())
				scrollItem.OnScrollWindowChanged(scrollWindow);
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

			if (_firstEntry == null)
				_firstEntry = entry;
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