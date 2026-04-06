using System.Collections.Generic;
using System.Linq;

namespace ItemBrowser.Common.Api.Themes {
	public class ItemBrowserTheme {
		private readonly ItemBrowserThemeDataBlock _data;
		public readonly IReadOnlyList<ItemBrowserThemeSpriteReplacementDataBlock> SpriteReplacements;
		public readonly IReadOnlyList<ItemBrowserThemeColorReplacementDataBlock> ColorReplacements;

		public DataBlockAddress Address => _data.address;
		public string Term => _data.term;
		public int DisplayOrder => _data.displayOrder;

		private ItemBrowserTheme(ItemBrowserThemeDataBlock data, List<ItemBrowserThemeSpriteReplacementDataBlock> spriteReplacements, List<ItemBrowserThemeColorReplacementDataBlock> colorReplacements) {
			_data = data;
			SpriteReplacements = spriteReplacements;
			ColorReplacements = colorReplacements;
		}

		public static IEnumerable<ItemBrowserTheme> GetAllThemesFromDataBlocks() {
			return ScriptableData.GetDataBlocks<ItemBrowserThemeDataBlock>().Where(theme => theme.isEnabled).Select(theme => {
				var spriteReplacements = ScriptableData.GetDataBlocks<ItemBrowserThemeSpriteReplacementDataBlock>()
					.Where(replacement => replacement.theme.address == theme.address)
					.ToList();
			
				var colorReplacements = ScriptableData.GetDataBlocks<ItemBrowserThemeColorReplacementDataBlock>()
					.Where(replacement => replacement.theme.address == theme.address)
					.ToList();
			
				return new ItemBrowserTheme(theme, spriteReplacements, colorReplacements);
			});
		}
	}
}