using System.Collections.Generic;
using ItemBrowser.Utilities;

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
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = optionalTitle.mTerm
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = optionalHoverDesc.mTerm,
					color = UserInterfaceUtility.DescriptionColor
				}
			};

			return lines;
		}
	}
}