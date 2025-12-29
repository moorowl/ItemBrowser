using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class TerritoryContentsDisplay : ObjectEntryDisplay<TerritoryContents> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText territoryText;
		
		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			});
			territoryText.Render($"ItemBrowser:TerritoryType/{Entry.Territory}");
		}

		private void RenderMoreInfo() { }
	}
}