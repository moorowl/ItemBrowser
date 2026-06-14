using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class EntriesList : UIelement, IScrollable {
		public float dividerPadding = 2f / 16f;
		public UIScrollWindow scrollWindow;
		public Transform container;
		
		private EntriesListRenderer _renderer;

		private void OnDestroy() {
			ClearList();
		}
		
		protected override void OnDisable() {
			ClearList();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			_renderer?.Update();
		}

		public void SetEntries(ObjectDataCD objectData, List<ObjectEntry> entries, float scrollProgress = 1f) {
			ClearList();

			if (entries.Count == 0)
				return;

			var firstEntry = entries[0];
			_renderer = firstEntry.CreateRenderer();

			if (!_renderer.SetEntries(this, objectData, entries))
				return;

			RenderList();

			scrollWindow.SetScrollValueImmediately(scrollProgress, this);
		}

		private void ClearList() {
			_renderer?.ClearList();
		}

		private void RenderList() {
			_renderer?.RenderList();
		}
		
		public void UpdateContainingElements(float scroll) { }

		public bool IsBottomElementSelected() {
			return false;
		}

		public bool IsTopElementSelected() {
			return false;
		}

		public float GetCurrentWindowHeight() {
			return _renderer == null ? 0f : Mathf.Abs(_renderer.TotalHeight) - 1f / 16f;
		}
	}
}