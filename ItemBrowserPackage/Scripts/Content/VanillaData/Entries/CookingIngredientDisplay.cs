using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CookingIngredientDisplay : ObjectEntryDisplay<CookingIngredient> {
		public ItemBrowserSlot ingredientSlot;
		public ItemBrowserSlot turnsIntoFoodSlot;
		
		protected override void OnRender(CookingIngredient entry) {
			ingredientSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Ingredient
			});
			turnsIntoFoodSlot.DisplayedObject = new DisplayedObject.CookedFood(entry.TurnsIntoFood, entry.Ingredient, ObjectID.Egg);
		}
		
		protected override void OnRenderDescription(CookingIngredient entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/CookingIngredient_0",
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}