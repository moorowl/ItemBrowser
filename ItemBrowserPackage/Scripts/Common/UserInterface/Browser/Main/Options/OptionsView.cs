using System.Collections.Generic;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class OptionsView : MainSubView, IScrollable {
		public UIScrollWindow scrollWindow;
		
		private readonly OptionsEntryType _cheatMode = new() {
			CanBeClicked = () => ClientWorldStateSystem.IsAdmin && !ClientWorldStateSystem.IsCreativeMode,
			OnLeftClick = () => {
				OptionsManager.Instance.CheatMode = !OptionsManager.Instance.CheatMode;
			},
			UpdateValueText = (valueText, _) => {
				if (ClientWorldStateSystem.IsCreativeMode)
					valueText.Render($"ItemBrowser-Options/Enabled");
				else if (ClientWorldStateSystem.IsAdmin)
					valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.CheatMode ? "Enabled" : "Disabled")}");
				else
					valueText.Render("ItemBrowser-Options/Unavailable");
			},
			GetDescription = (lines, _) => {
				if (!ClientWorldStateSystem.IsAdmin && !ClientWorldStateSystem.IsCreativeMode) {
					lines[^1].paddingBeneath = UserInterfaceUtility.DescriptionPadding;
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-Options/CheatModeUnavailable",
						color = Manager.ui.brokenColor
					});
				}
			}
		};
		private readonly OptionsEntryType _clearFavorites = new() {
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

						OptionsManager.Instance.RemoveTagFromAll(ObjectTagType.Favorited);
					},
					options: new List<string> { "cancelDialogue", "yes" },
					minWidth: 10f,
					backgroundAlpha: 0.9f,
					pauseGame: false
				);
				Manager.ui.DeselectAnySelectedUIElement();
				Manager.ui.mouse.UpdateMouseUIInput(out _, out _);
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
				OptionsManager.Instance.ShowButtonHints = !OptionsManager.Instance.ShowButtonHints;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.ShowButtonHints ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _searchByDescription = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.SearchByDescription = !OptionsManager.Instance.SearchByDescription;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.SearchByDescription ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _searchByEffect = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.SearchByEffect = !OptionsManager.Instance.SearchByEffect;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.SearchByEffect ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _searchById = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.SearchById = !OptionsManager.Instance.SearchById;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.SearchById ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _showSourceMod = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.ShowSourceMod = !OptionsManager.Instance.ShowSourceMod;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.ShowSourceMod ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _showTechnicalInfo = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.AlwaysShowTechnicalInfo = !OptionsManager.Instance.AlwaysShowTechnicalInfo;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.AlwaysShowTechnicalInfo ? "Always" : "KeyHeld")}");
			}
		};
		private readonly OptionsEntryType _hideUndiscovered = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.DiscoveryMode = !OptionsManager.Instance.DiscoveryMode;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.DiscoveryMode ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _showChecklist = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.ShowChecklist = !OptionsManager.Instance.ShowChecklist;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.ShowChecklist ? "Enabled" : "Disabled")}");
			}
		};
		private readonly OptionsEntryType _autoMarkDiscoveredAsCollected = new() {
			OnLeftClick = () => {
				OptionsManager.Instance.AutoMarkDiscoveredAsCollected = !OptionsManager.Instance.AutoMarkDiscoveredAsCollected;
			},
			UpdateValueText = (valueText, _) => {
				valueText.Render($"ItemBrowser-Options/{(OptionsManager.Instance.AutoMarkDiscoveredAsCollected ? "Enabled" : "Disabled")}");
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
			if (_firstEntry != null && !UserInterfaceUtility.IsUsingMouseAndKeyboard)
				UserInterfaceUtility.SelectAndMoveMouseTo(_firstEntry);
		}

		private void AddAllSettings() {
			// General
			AddSection("ItemBrowser-Options/General");
			AddEntry("ItemBrowser-Options/CheatMode", _cheatMode);
			AddEntry("ItemBrowser-Options/HideUndiscovered", _hideUndiscovered);
			AddEntry("ItemBrowser-Options/ClearFavorites", _clearFavorites);

			// Appearance
			AddSection("ItemBrowser-Options/Appearance");
			AddEntry("ItemBrowser-Options/Theme", _theme);
			AddEntry("ItemBrowser-Options/ShowSourceMod", _showSourceMod);
			AddEntry("ItemBrowser-Options/ShowButtonHints", _showButtonHints);
			AddEntry("ItemBrowser-Options/ShowTechnicalInfo", _showTechnicalInfo);

			// Checklist
			// AddSection("ItemBrowser-Options/Checklist");
			// AddEntry("ItemBrowser-Options/ShowChecklist", _showChecklist);
			// AddEntry("ItemBrowser-Options/AutoMarkDiscoveredAsCollected", _autoMarkDiscoveredAsCollected);

			// Search
			AddSection("ItemBrowser-Options/Search");
			AddEntry("ItemBrowser-Options/SearchByDescription", _searchByDescription);
			AddEntry("ItemBrowser-Options/SearchByEffect", _searchByEffect);
			AddEntry("ItemBrowser-Options/SearchById", _searchById);
			
			foreach (var scrollItem in scrollContainer.GetComponentsInChildren<IScrollItem>())
				scrollItem.OnScrollWindowChanged(scrollWindow);
		}

		private void AddSection(string term) {
			var section = Instantiate(sectionTemplate, scrollContainer);
			section.gameObject.SetActive(true);
			section.SetTerm(term);
			var sectionHeight = UserInterfaceUtility.CalculateHeight(section);
			
			_height -= sectionHeight / 2f;
			section.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= sectionHeight / 2f;
			
			AddDivider();
		}

		private void AddEntry(string term, OptionsEntryType type) {
			var entry = Instantiate(entryTemplate, scrollContainer);
			entry.SetNameAndType(term, type);
			entry.gameObject.SetActive(true);
			var entryHeight = UserInterfaceUtility.CalculateHeight(entry);
			
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
			var dividerHeight = UserInterfaceUtility.CalculateHeight(divider);
			
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