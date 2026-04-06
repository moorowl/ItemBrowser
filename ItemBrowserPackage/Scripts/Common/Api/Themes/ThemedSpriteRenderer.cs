using System.Collections.Generic;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.Common.Api.Themes {
	[RequireComponent(typeof(SpriteRenderer))]
	public class ThemedSpriteRenderer : ThemedRenderer {
		public SpriteRenderer spriteRenderer;
		public DataBlockRef<ItemBrowserThemeSpriteVariableDataBlock> spriteVariable;
		public DataBlockRef<ItemBrowserThemeColorVariableDataBlock> colorVariable;

		private void OnValidate() {
			spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
			if (spriteRenderer == null)
				return;

			OnClear();
		}

		protected override void OnClear() {
			if (spriteVariable != null)
				spriteRenderer.sprite = spriteVariable.Get().defaultSprite;
			
			if (colorVariable != null) {
				var alpha = spriteRenderer.color.a;
				spriteRenderer.color = colorVariable.Get().defaultColor.ColorWithNewAlpha(alpha);
			}
		}

		protected override void OnApply(IReadOnlyList<ItemBrowserThemeSpriteReplacementDataBlock> sprites, IReadOnlyList<ItemBrowserThemeColorReplacementDataBlock> colors) {
			foreach (var replacement in sprites) {
				if (replacement.spriteReplacement != null && replacement.spriteVariable.hasAddress && replacement.spriteVariable.address == spriteVariable.address)
					spriteRenderer.sprite = replacement.spriteReplacement;
			}
			
			foreach (var replacement in colors) {
				if (replacement.colorVariable.hasAddress && replacement.colorVariable.address == colorVariable.address) {
					var alpha = spriteRenderer.color.a;
					spriteRenderer.color = replacement.colorReplacement.ColorWithNewAlpha(alpha);
				}
			}
		}
	}
}