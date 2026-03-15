using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
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
		
		private void Show() {
			OnShow(!_hasBeenShownBefore);
			
			foreach (var subView in GetComponentsInChildren<ItemBrowserView>()) {
				if (subView != this && subView.IsShowing)
					subView.Show();
			}

			_isShowing = true;
			_hasBeenShownBefore = true;
		}

		private void Hide() {
			OnHide();
			
			foreach (var subView in GetComponentsInChildren<ItemBrowserView>()) {
				if (subView != this && !subView.IsShowing)
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