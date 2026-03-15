using System.Collections.Generic;
using ItemBrowser.Utilities;

namespace ItemBrowser.UserInterface.Browser {
	public class OptionsEntry : ItemBrowserButton {
		public PugText nameText;
		public PugText valueText;
		public float defaultTextOpacity;
		public float disabledTextOpacity;

		private OptionsEntryType _type;
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			if (!canBeClicked || _type?.OnLeftClick == null)
				return;
			
			base.OnLeftClicked(mod1, mod2);

			_type.OnLeftClick.Invoke();
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericOpen, this);
			UpdateVisuals();
		}
		
		public override void OnRightClicked(bool mod1, bool mod2) {
			if (!canBeClicked || _type?.OnRightClick == null)
				return;
			
			base.OnRightClicked(mod1, mod2);

			_type.OnRightClick.Invoke();
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.GenericOpen, this);
			UpdateVisuals();
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateVisuals();
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (valueText.displayedTextString.Length > 0) {
				return new TextAndFormatFields {
					text = $"{nameText.displayedTextString}: {valueText.displayedTextString}",
					dontLocalize = true
				};
			}

			return new TextAndFormatFields {
				text = nameText.GetText()
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = nameText.GetText() + "Desc",
					color = UserInterfaceUtils.DescriptionColor
				}
			};

			_type?.GetDescription?.Invoke(lines);

			return lines;
		}

		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR;
		}
		
		public void SetNameAndType(string term, OptionsEntryType type) {
			nameText.Render(term);
			_type = type;
			UpdateValueText();
		}
		
		private void UpdateVisuals() {
			canBeClicked = _type?.CanBeClicked?.Invoke() ?? true;
			UpdateValueText();
			UpdateTextOpacity();
		}
		
		private void UpdateValueText() {
			_type?.UpdateValueText?.Invoke(valueText, canBeClicked);
		}

		private void UpdateTextOpacity() {
			var opacity = canBeClicked ? defaultTextOpacity : disabledTextOpacity;

			nameText.style.color.a = opacity;
			nameText.SetTempColor(nameText.style.color);
			valueText.style.color.a = opacity;
			valueText.SetTempColor(nameText.style.color);
		}
	}
}