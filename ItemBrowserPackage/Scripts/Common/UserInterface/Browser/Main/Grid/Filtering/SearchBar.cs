using System.Collections;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SearchBar : TextInputField {
		private const float DoubleClickThreshold = 0.5f;

		public GameObject highlightBorder;
		
		public bool HighlightSearchResults { get; private set; }

		private float _lastLeftClicked;
		private bool _oldHighlightSearchResults;

		public override void OnLeftClicked(bool mod1, bool mod2) {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard)
				base.OnLeftClicked(mod1, mod2);
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard) {
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
		}

		private void UpdateVisuals() {
			if (_oldHighlightSearchResults != HighlightSearchResults) {
				highlightBorder.SetActive(HighlightSearchResults);
				_oldHighlightSearchResults = HighlightSearchResults;
			}
		}

		private void UpdateHighlightSearchResultsInput() {
			if (string.IsNullOrWhiteSpace(GetInputText())) {
				HighlightSearchResults = false;
				return;
			}
			
			var input = Manager.input.singleplayerInputModule;
			if (!selectedMarker.activeSelf || !input.WasButtonPressedDownThisFrame(PlayerInput.InputType.UI_INTERACT, true))
				return;

			if (Time.time <= _lastLeftClicked + DoubleClickThreshold) {
				HighlightSearchResults = !HighlightSearchResults;
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