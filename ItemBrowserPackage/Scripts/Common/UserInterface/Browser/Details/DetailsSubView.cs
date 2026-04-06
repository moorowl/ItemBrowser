namespace ItemBrowser.Common.UserInterface.Browser {
	public abstract class DetailsSubView : ItemBrowserView {
		public abstract void OnApplyState(DetailsState currentState, DetailsState previousState);
	}
}