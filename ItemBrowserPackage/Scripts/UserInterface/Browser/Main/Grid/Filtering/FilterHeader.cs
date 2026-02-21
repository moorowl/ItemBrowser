namespace ItemBrowser.UserInterface.Browser {
	public class FilterHeader : UIelement {
		public PugText text;

		public void SetTerm(string term) {
			text.Render(term);
		}
	}
}