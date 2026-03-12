namespace ItemBrowser.UserInterface.Browser {
	public class OptionsSection : UIelement {
		public PugText labelText;

		public void SetTerm(string term) {
			labelText.Render(term);
		}
	}
}