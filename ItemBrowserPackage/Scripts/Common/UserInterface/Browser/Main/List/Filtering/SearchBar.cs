using System.Collections;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SearchBar : TextInputField {
		private const float DoubleClickThreshold = 0.5f;
		
		public ObjectListView objectListView;
		public GameObject highlightBorder;
		public SpriteMask mask;
		
		private float _lastLeftClicked;
		private bool _oldHighlightSearchResults;
		private string _lastSearchTerm;

		private bool CanHighlightSearchResults => !string.IsNullOrWhiteSpace(GetInputText());
		private bool CanClearSearchResults => GetInputText().Length > 0;

		public override void OnLeftClicked(bool mod1, bool mod2) {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard)
				base.OnLeftClicked(mod1, mod2);
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard && CanClearSearchResults) {
				ResetText();
				StartCoroutine(ReselectInputField());
			}
		}

		private IEnumerator ReselectInputField() {
			yield return new WaitForSeconds(0.05f);

			if (Manager.ui.currentSelectedUIElement == this)
				OnLeftClicked(false, false);	
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			DeselectIfUsingController();
			UpdateHighlightSearchResultsInput();
			UpdateVisuals();

			if (selectedMarker != null && selectedMarker.activeSelf) {
				if (CanClearSearchResults)
					ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(ButtonHint.SearchClear);
				
				if (CanHighlightSearchResults)
					ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(ButtonHint.SearchHighlight);
			}

			if (_lastSearchTerm != GetInputText()) {
				AdjustSearchFieldPosition();
				_lastSearchTerm = GetInputText();
			}
		}

		private void UpdateVisuals() {
			if (_oldHighlightSearchResults != objectListView.HighlightSearchResults) {
				highlightBorder.SetActive(objectListView.HighlightSearchResults);
				_oldHighlightSearchResults = objectListView.HighlightSearchResults;
			}
		}
		
		private void AdjustSearchFieldPosition() {
			var maskUnitWidth = mask.transform.localScale.x / 16f;
			var searchInputPosition = pugText.transform.localPosition;
			searchInputPosition.x = -1f * Mathf.Max(0f, pugText.dimensions.width - maskUnitWidth);
			pugText.transform.localPosition = searchInputPosition;

			var member = typeof(TextInputField).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "Update");
			API.Reflection.Invoke(member, this);
		}

		private void UpdateHighlightSearchResultsInput() {
			if (!CanHighlightSearchResults) {
				objectListView.HighlightSearchResults = false;
				return;
			}

			var input = Manager.input.singleplayerInputModule;
			if (!selectedMarker.activeSelf || !input.WasButtonPressedDownThisFrame(PlayerInput.InputType.UI_INTERACT, true))
				return;

			if (Time.time <= _lastLeftClicked + DoubleClickThreshold) {
				objectListView.HighlightSearchResults = !objectListView.HighlightSearchResults;
				_lastLeftClicked = 0f;
			} else {
				_lastLeftClicked = Time.time;
			}
		}

		private void DeselectIfUsingController() {
			if (!UserInterfaceUtility.IsUsingMouseAndKeyboard && inputIsActive)
				Deactivate(true);
		}
		
		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}
	}
}