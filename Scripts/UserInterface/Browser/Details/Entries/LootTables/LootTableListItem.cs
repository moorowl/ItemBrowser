using System.Linq;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class LootTableListItem : UIelement {
		public ItemBrowserSlot itemSlot;
		public PugText chancePerRollText;
		public SpriteRenderer chancePerRollStrike;
		public PugText chanceForOneText;
		public SpriteRenderer chanceForOneStrike;
		public Transform strikeContainer;
		public float strikePadding;
		
		public void SetItem(PrimaryLootTable.Entry entry) {
			itemSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			}, entry.Amount.Get());

			var isAvailable = entry.Requirements.All(requirement => requirement.IsFulfilled());
			chancePerRollText.Render(UserInterfaceUtils.FormatChance(entry.Chance) + "%" + (isAvailable ? "" : "*"));
			chanceForOneText.Render(UserInterfaceUtils.FormatChance(entry.ChanceForOne.Get()) + "%" + (isAvailable ? "" : "*"));

			strikeContainer.gameObject.SetActive(false);
			/*strikeContainer.gameObject.SetActive(entry.Requirements.Any(requirement => !requirement.IsFulfilled()));
			if (strikeContainer.gameObject.activeSelf) {
				chancePerRollStrike.size = new Vector2(chancePerRollText.dimensions.width + strikePadding, chancePerRollStrike.size.y);
				chanceForOneStrike.size = new Vector2(chanceForOneText.dimensions.width + strikePadding, chanceForOneStrike.size.y);
			}*/
		}
	}
}