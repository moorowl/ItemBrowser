using ItemBrowser.Utilities;

namespace ItemBrowser.UserInterface.Browser {
	public class SwapTabButton : ItemBrowserButton {
		public DetailsView detailsView;

		private DetailsTab _tab;

		public void SetTab(DetailsTab tab) {
			_tab = tab;
		}
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			if (!canBeClicked)
				return;
			
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTabOrCategory, this);
			detailsView.SwapSelectedTab(_tab);
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return null;

			return new TextAndFormatFields {
				text = detailsView.IsSelectedObjectNonObtainable
					? $"ItemBrowser-DetailsTabs/{_tab}_Show_NonObtainable"
					: $"ItemBrowser-DetailsTabs/{_tab}_Show"
			};
		}
	}
}