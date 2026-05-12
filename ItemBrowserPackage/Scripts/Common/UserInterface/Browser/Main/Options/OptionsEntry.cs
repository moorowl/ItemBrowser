using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugMod;
using Unity.Mathematics;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class OptionsEntry : ItemBrowserButton {
		private const int MaxNameLength = 33;
		
		public PugText nameText;
		public PugText valueText;
		public float defaultTextOpacity;
		public float disabledTextOpacity;

		private string _nameTerm;
		private OptionsEntryType _type;
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			if (!canBeClicked || _type?.OnLeftClick == null)
				return;
			
			base.OnLeftClicked(mod1, mod2);

			_type.OnLeftClick.Invoke();
			UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.GenericOpen, this);
			UpdateVisuals();
		}
		
		public override void OnRightClicked(bool mod1, bool mod2) {
			if (!canBeClicked || _type?.OnRightClick == null)
				return;
			
			base.OnRightClicked(mod1, mod2);

			_type.OnRightClick.Invoke();
			UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.GenericOpen, this);
			UpdateVisuals();
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateVisuals();
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (valueText.displayedTextString.Length > 0) {
				var localizedName = API.Localization.GetLocalizedTerm(_nameTerm) ?? _nameTerm;
				return new TextAndFormatFields {
					text = $"{localizedName}: {valueText.displayedTextString}",
					dontLocalize = true
				};
			}

			return new TextAndFormatFields {
				text = _nameTerm
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = _nameTerm + "Desc",
					color = UserInterfaceUtility.DescriptionColor
				}
			};

			_type?.GetDescription?.Invoke(lines);

			return lines;
		}

		public void SetNameAndType(string term, OptionsEntryType type) {
			_nameTerm = term;
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

			var localizedName = API.Localization.GetLocalizedTerm(_nameTerm) ?? _nameTerm;
			var allowedLeftChars = MaxNameLength - valueText.displayedTextString.Length;
			if (allowedLeftChars < localizedName.Length)
				localizedName = localizedName[..math.max(0, allowedLeftChars - 3)].TrimEnd() + "...";

			nameText.localize = false;
			nameText.Render(localizedName);
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