using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ContextView : ItemBrowserView {
		public Transform buttonContainer;
		public ContextButton buttonPrefab;
		public SpriteRenderer background;
		public Vector2 backgroundPadding = new(0.5f, 0.5f);

		private readonly List<ContextButton> _buttons = new();

		protected override void OnHide() {
			IsShowing = false;
			
			foreach (var button in _buttons)
				button.gameObject.SetActive(false);
		}

		public void ShowWithOptions(List<ContextOption> options) {
			for (var i = 0; i < options.Count - _buttons.Count; i++)
				_buttons.Add(Instantiate(buttonPrefab, buttonContainer));

			var totalHeight = 0f;
			var totalWidth = 0f;
			for (var i = 0; i < _buttons.Count; i++) {
				var button = _buttons[i];
				
				if (i >= options.Count) {
					button.gameObject.SetActive(false);
				} else {
					button.gameObject.SetActive(true);
					button.SetOption(options[i]);

					totalWidth = Mathf.Max(totalWidth, button.TextWidth);
					totalHeight += button.TotalHeight / 2f;
					if (i > 0)
						totalHeight += 1f / 16f;

					button.transform.localPosition = new Vector3(button.transform.localPosition.x, -totalHeight, button.transform.localPosition.z);

					totalHeight += button.TotalHeight / 2f;
				}
			}
			
			for (var i = 0; i < options.Count; i++)
				_buttons[i].SetWidth(totalWidth);

			background.size = new Vector3(totalWidth + backgroundPadding.x, totalHeight + backgroundPadding.y);
			background.transform.localPosition = new Vector3(totalWidth / 2f, -(totalHeight / 2f), 0f);
			transform.position = new Vector3(Manager.ui.mouse.pointer.position.x + backgroundPadding.x, Manager.ui.mouse.pointer.position.y - (background.size.y / 2f), transform.position.z);
			
			IsShowing = true;
			
			Manager.ui.DeselectAnySelectedUIElement();
			Manager.ui.mouse.UpdateMouseUIInput(out _, out _);
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			IsShowing = false;
		}
	}
}