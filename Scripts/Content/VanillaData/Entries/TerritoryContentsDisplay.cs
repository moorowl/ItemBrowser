using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class TerritoryContentsDisplay : ObjectEntryDisplay<TerritoryContents> {
		public ItemBrowserSlot resultSlot;
		public PugText territoryText;
		
		protected override void OnRender(TerritoryContents entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			});
			territoryText.Render($"ItemBrowser-TerritoryNames/{entry.Territory}");
		}

		protected override void OnRenderDescription(TerritoryContents entry, EntryDescriptionButton description) { }
	}
}