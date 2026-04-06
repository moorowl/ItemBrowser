using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Common.Api.Themes {
	public abstract class ThemedRenderer : MonoBehaviour {
		public static event Action<ThemedRenderer> OnEnabled;
		public static event Action<ThemedRenderer> OnDisabled;

		private DataBlockAddress _lastTheme;
		
		private void OnEnable() {
			OnEnabled?.Invoke(this);
		}

		private void OnDisable() {
			OnDisabled?.Invoke(this);
		}
		
		public void Apply(DataBlockAddress theme, IReadOnlyList<ItemBrowserThemeSpriteReplacementDataBlock> sprites, IReadOnlyList<ItemBrowserThemeColorReplacementDataBlock> colors) {
			if (_lastTheme == theme)
				return;
			
			OnClear();
			OnApply(sprites, colors);
			
			_lastTheme = theme;
		}

		protected abstract void OnClear();
		
		protected abstract void OnApply(IReadOnlyList<ItemBrowserThemeSpriteReplacementDataBlock> sprites, IReadOnlyList<ItemBrowserThemeColorReplacementDataBlock> colors);
	}
}