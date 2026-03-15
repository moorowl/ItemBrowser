using PugMod;

namespace ItemBrowser.UserInterface.Browser {
	public class EntriesUnavailableHeader : UIelement {
		public PugText text;
		public string term;

		public void SetAmount(int amount) {
			// text.Render(string.Format(API.Localization.GetLocalizedTerm(term), amount.ToString()));
		}
	}
}