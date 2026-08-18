using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ContextButton : ItemBrowserButton {
		public ContextView contextView;
		public BoxCollider boxCollider;
		public SpriteRenderer background;
		public SpriteRenderer backgroundSelected;
		
		private ContextOption _option;

		public float TotalHeight => boxCollider.size.y;
		public float TextWidth => text.dimensions.width + text.transform.localPosition.x * 2f;
		
		public void SetOption(ContextOption option) {
			_option = option;
			text.Render(option.Term ?? "");
		}

		public void SetWidth(float width) {
			boxCollider.size = new Vector3(width, boxCollider.size.y, boxCollider.size.z);
			boxCollider.center = new Vector3(boxCollider.size.x / 2f, boxCollider.center.y, boxCollider.center.z);

			background.transform.localPosition = new Vector3(boxCollider.center.x, background.transform.localPosition.y, background.transform.localPosition.z);
			background.size = new Vector2(boxCollider.size.x, boxCollider.size.y);
			
			backgroundSelected.size = background.size;
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			_option?.Function?.Invoke();
			contextView.IsShowing = false;
		}
	}
}