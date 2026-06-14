using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class HistoryView : MainSubView, IScrollable {
		public Transform scrollContainer;
		public UIScrollWindow scrollWindow;
		public DetailsView detailsView;
		public PugText nothingToShowText;
		public float dividerPadding = 2f / 16f;

		private float _height;
		private readonly List<UIelement> _activePooledElements = new();
		private HistoryEntry _firstEntry;
		
		protected override void OnShow(bool isFirstTimeShowing) {
			RenderList();
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

		protected override void OnHide() {
			ClearList();
		}

		private void RenderList() {
			ClearList();

			var states = detailsView.History.OrderByDescending(state => state.Timestamp).ToList();
			foreach (var state in states)
				AddEntry(state);
			
			foreach (var scrollItem in scrollContainer.GetComponentsInChildren<IScrollItem>())
				scrollItem.OnScrollWindowChanged(scrollWindow);
			
			nothingToShowText.gameObject.SetActive(states.Count == 0);
		}
		
		private void ClearList() {
			scrollWindow.ResetScrollValueImmediately(this);
			
			_height = 0f;
			_firstEntry = null;
			
			for (var i = _activePooledElements.Count - 1; i >= 0; i--) {
				var element = _activePooledElements[i];

				foreach (var pugText in element.GetComponentsInChildren<PugText>(true)) {
					var wasActive = pugText.gameObject.activeSelf;
					pugText.Clear();
					pugText.gameObject.SetActive(wasActive);
				}

				ItemBrowserAPI.FreePooledElement(element);
			}

			_activePooledElements.Clear();
		}
		
		private void AddEntry(DetailsState state) {
			var entry = ItemBrowserAPI.GetPooledElement<HistoryEntry>();
			_activePooledElements.Add(entry);
			entry.gameObject.SetActive(true);
			entry.SetDetailsState(state);
			var entryHeight = UserInterfaceUtility.CalculateHeight(entry);
			
			_height -= 1.25f / 2f;
			entry.transform.SetParent(scrollContainer);
			entry.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= 1.25f / 2f;
			
			AddDivider();

			if (_firstEntry == null)
				_firstEntry = entry;
		}
		
		private void AddDivider() {
			var divider = ItemBrowserAPI.GetPooledElement<EntriesDivider>();
			_activePooledElements.Add(divider);
			divider.gameObject.SetActive(true);
			var dividerHeight = UserInterfaceUtility.CalculateHeight(divider);
			
			_height -= dividerPadding;
			_height -= dividerHeight / 2f;
			divider.transform.SetParent(scrollContainer);
			divider.transform.localPosition = new Vector3(0f, _height, 0f);
			_height -= dividerHeight / 2f;
			_height -= dividerPadding;
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