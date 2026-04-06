using UnityEngine;

namespace ItemBrowser.Common.Api.Themes {
	public class ItemBrowserThemeColorReplacementDataBlock : ScriptableDataBlock {
		public DataBlockRef<ItemBrowserThemeDataBlock> theme;
		public DataBlockRef<ItemBrowserThemeColorVariableDataBlock> colorVariable;
		public Color colorReplacement;
	}
}