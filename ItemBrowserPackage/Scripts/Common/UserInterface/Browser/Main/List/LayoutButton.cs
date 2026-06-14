using System;
using System.Collections.Generic;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class LayoutButton : ItemBrowserButton {
		public SpriteRenderer[] iconsToUpdate;
		public Sprite gridLayoutIcon;
		public Sprite listLayoutIcon;

		private VirtualObjectListLayout _previousLayout;

		protected override void Awake() {
			base.Awake();

			UpdateVisuals();
		}
		
		private void OnEnable() {
			UpdateVisuals();
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (_previousLayout != OptionsManager.Instance.ListLayout) {
				UpdateVisuals();
				_previousLayout = OptionsManager.Instance.ListLayout;
			}
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			OptionsManager.Instance.ListLayout = OptionsManager.Instance.ListLayout switch {
				VirtualObjectListLayout.Grid => VirtualObjectListLayout.List,
				VirtualObjectListLayout.List => VirtualObjectListLayout.Grid,
				_ => throw new ArgumentOutOfRangeException()
			};
			UpdateVisuals();
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = $"ItemBrowser-General/Layout{OptionsManager.Instance.ListLayout}",
					color = UserInterfaceUtility.DescriptionColor
				}
			};
		}

		private void UpdateVisuals() {
			var iconToUse = OptionsManager.Instance.ListLayout switch {
				VirtualObjectListLayout.Grid => gridLayoutIcon,
				VirtualObjectListLayout.List => listLayoutIcon,
				_ => throw new ArgumentOutOfRangeException()
			};

			foreach (var icon in iconsToUpdate)
				icon.sprite = iconToUse;
		}
	}
}