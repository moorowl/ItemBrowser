using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class VirtualObjectListHeader : ItemBrowserButton {
		public PugText nameText;
		public PugText amountText;
		public SpriteRenderer icon;
		public ObjectListView objectListView;
		public float unselectedOpacity = 0.5f;
		public float selectedOpacity = 1f;
		public Sprite collapsedSprite;
		public Sprite uncollapsedSprite;
		
		private int _index;
		private bool _wasCollapsed;
		private bool _wasSelected;

		public void SetGroup(int index, string term, int amountInGroup) {
			_index = index;
			nameText.Render(term);
			amountText.Render($"x{amountInGroup}");

			amountText.transform.localPosition = new Vector3(
				nameText.transform.localPosition.x + nameText.dimensions.width + 0.25f,
				amountText.transform.localPosition.y,
				amountText.transform.localPosition.z
			);
		}

		private void UpdateVisuals(bool force) {
			var isSelected = IsSelected;
			if (force || isSelected != _wasSelected) {
				var opacityToUse = IsSelected ? selectedOpacity : unselectedOpacity;
				
				nameText.style.color = nameText.style.color.ColorWithNewAlpha(opacityToUse);
				nameText.SetTempColor(nameText.style.color);
				amountText.style.color = amountText.style.color.ColorWithNewAlpha(opacityToUse);
				amountText.SetTempColor(amountText.style.color);
				icon.color = icon.color.ColorWithNewAlpha(opacityToUse);
				
				_wasSelected = isSelected;
			}

			var isCollapsed = objectListView.IsIndexInGroupCollapsed(_index);
			if (force || isCollapsed != _wasCollapsed) {
				icon.sprite = isCollapsed ? collapsedSprite : uncollapsedSprite;
				
				_wasCollapsed = isCollapsed;
			}
		}

		protected override void OnEnable() {
			base.OnEnable();
			
			UpdateVisuals(true);
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateVisuals(false);
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			objectListView.SetIndexInGroupCollapsed(_index, !objectListView.IsIndexInGroupCollapsed(_index));
			UpdateVisuals(false);
		}
	}
}