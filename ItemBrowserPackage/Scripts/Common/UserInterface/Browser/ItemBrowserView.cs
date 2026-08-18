using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ItemBrowserView : UIelement {
		private bool _hasBeenShownBefore;

		public bool IsShowing {
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		protected virtual void OnEnable() {
			OnShow(!_hasBeenShownBefore);
			_hasBeenShownBefore = true;
		}

		protected override void OnDisable() {
			base.OnDisable();

			OnHide();
		}
		
		protected virtual void OnShow(bool isFirstTimeShowing) { }

		protected virtual void OnHide() { }

		public static bool IsInsideView(MonoBehaviour monoBehaviour) {
			return monoBehaviour.GetComponentInParent<ItemBrowserView>() != null;
		}
	}
}