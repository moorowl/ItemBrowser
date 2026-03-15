using System;
using System.Collections.Generic;
using ItemBrowser.Api;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ItemBrowser.UserInterface.Browser {
	public class HistoryEntry : UIelement {
		public ItemBrowserSlot itemSlot;
		public PugText typeLabel;
		public PugText categoryLabel;
		public PugText timestampLabel;
		public GameObject selectedBorder;

		private DetailsState _state;
		private bool _isNonObtainable;
		private string _typeString;
		private string _timestampString;

		private void Awake() {
			selectedBorder.SetActive(false);
		}

		public void SetDetailsState(DetailsState state) {
			_state = state;
			
			itemSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = state.ObjectData.objectID,
				variation = state.ObjectData.variation
			});
			
			_typeString = ObjectUtils.IsNonObtainable(state.ObjectData) ? $"ItemBrowser-DetailsTabs/{state.Tab}_NonObtainable" : $"ItemBrowser-DetailsTabs/{state.Tab}";
			_timestampString = DateTime.FromFileTime(state.Timestamp).ToShortTimeString();
			typeLabel.Render(_typeString);
			timestampLabel.Render(_timestampString);
			
			switch (state.Tab) {
				case DetailsTab.Sources:
					categoryLabel.Render(state.EntriesSourceCategoryTerm);
					break;
				case DetailsTab.Usages:
					categoryLabel.Render(state.EntriesUsageCategoryTerm);
					break;
			}
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (_state.ObjectData.objectID == ObjectID.None)
				return;

			ItemBrowserAPI.ItemBrowserUI.ShowDetailsFromHistory(_state);
		}

		public override void OnSelected() {
			base.OnSelected();
			selectedBorder.SetActive(true);
		}

		public override void OnDeselected(bool playEffect = true) {
			base.OnDeselected(playEffect);
			selectedBorder.SetActive(false);
		}

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = "ItemBrowser-General/JumpTo",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(_state.ObjectData)
				},
				dontLocalizeFormatFields = true
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = _typeString,
					color = UserInterfaceUtils.DescriptionColor
				},
				new() {
					text = _state.Tab switch {
						DetailsTab.Sources => _state.EntriesSourceCategoryTerm,
						DetailsTab.Usages => _state.EntriesUsageCategoryTerm,
						_ => ""
					},
					color = UserInterfaceUtils.DescriptionColor,
					paddingBeneath = UserInterfaceUtils.DescriptionPadding
				},
				new() {
					text = "ItemBrowser-General/LastAccessed",
					formatFields = new[] {
						_timestampString
					},
					dontLocalizeFormatFields = true,
					color = Color.white * 0.95f
				}
			};
		}
		
		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return UserInterfaceUtils.IsUsingMouseAndKeyboard ? HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR : HoverWindowAlignment.BOTTOM_RIGHT_OF_SCREEN;
		}
	}
}