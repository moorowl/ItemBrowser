using System.Linq;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ItemBrowserView : UIelement {
		private bool _hasBeenShownBefore;
		private bool _isShowing;
		public bool IsShowing {
			get => gameObject.activeSelf;
			set {
				gameObject.SetActive(value);
				if (_isShowing == value)
					return;
				
				if (value)
					Show();
				else
					Hide();
			}
		}

		private ItemBrowserView[] _subViews;
		private ItemBrowserView[] SubViews {
			get {
				return _subViews ??= GetComponentsInChildren<ItemBrowserView>(true).Where(subView => subView != this).ToArray();
			}
		}

		private void Awake() {
			_subViews = GetComponentsInChildren<ItemBrowserView>(true).Where(subView => subView != this).ToArray();
		}

		private void Show() {
			OnShow(!_hasBeenShownBefore);
			
			foreach (var subView in SubViews) {
				if (subView.gameObject.activeSelf && subView.IsShowing)
					subView.Show();
			}

			_isShowing = true;
			_hasBeenShownBefore = true;
		}

		private void Hide() {
			OnHide();
			
			foreach (var subView in SubViews) {
				if (subView.gameObject.activeSelf && !subView.IsShowing)
					subView.Hide();
			}

			_isShowing = false;
		}
		
		protected virtual void OnShow(bool isFirstTimeShowing) { }

		protected virtual void OnHide() { }

		public static bool IsInsideView(MonoBehaviour monoBehaviour) {
			return monoBehaviour.GetComponentInParent<ItemBrowserView>() != null;
		}
	}
}