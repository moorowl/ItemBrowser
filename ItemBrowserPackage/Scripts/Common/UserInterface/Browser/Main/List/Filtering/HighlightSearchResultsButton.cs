using System.Collections.Generic;
using ItemBrowser.Utilities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class HighlightSearchResultsButton : ItemBrowserButton {
		public ObjectListView objectListView;

		protected override void LateUpdate() {
			IsToggled = objectListView.HighlightSearchResults;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			objectListView.HighlightSearchResults = !objectListView.HighlightSearchResults;
		}
	}
}