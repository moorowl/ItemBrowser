using System.Collections.Generic;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.Common.Api.Themes {
	[RequireComponent(typeof(PugText))]
	public class ThemedPugText : ThemedRenderer {
		public PugText pugText;
		public DataBlockRef<ItemBrowserThemeColorVariableDataBlock> colorVariable;

		private void OnValidate() {
			pugText = gameObject.GetComponent<PugText>();
			if (pugText == null)
				return;

			OnClear();
		}

		protected override void OnClear() {
			if (colorVariable != null) {
				var alpha = pugText.style.color.a;
				pugText.style.color = colorVariable.Get().defaultColor.ColorWithNewAlpha(alpha);
				pugText.SetTempColor(pugText.style.color);
			}
		}

		protected override void OnApply(IReadOnlyList<ItemBrowserThemeSpriteReplacementDataBlock> sprites, IReadOnlyList<ItemBrowserThemeColorReplacementDataBlock> colors) {
			foreach (var replacement in colors) {
				if (replacement.colorVariable.hasAddress && replacement.colorVariable.address == colorVariable.address) {
					var alpha = pugText.style.color.a;
					pugText.style.color = replacement.colorReplacement.ColorWithNewAlpha(alpha);
					pugText.SetTempColor(pugText.style.color);
				}
			}
		}
	}
}