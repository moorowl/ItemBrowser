using UnityEngine;

namespace ItemBrowser.Api.Themes {
	public class ItemBrowserThemeSpriteReplacementDataBlock : ScriptableDataBlock {
		public DataBlockRef<ItemBrowserThemeDataBlock> theme;
		public DataBlockRef<ItemBrowserThemeSpriteVariableDataBlock> spriteVariable;
		public Sprite spriteReplacement;
	}
}