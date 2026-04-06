using ItemBrowser.Content.VanillaData.Entries;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class LootTablePoolHeader : UIelement {
		public PugText text;
		
		public bool Render(PrimaryLootTable.Pool pool) {
			var description = pool.Header.GetLocalizedDescription(pool);

			if (description != null) {
				text.gameObject.SetActive(true);
				text.Render(description);
				return true;
			}

			text.gameObject.SetActive(false);
			return false;
		}
	}
}