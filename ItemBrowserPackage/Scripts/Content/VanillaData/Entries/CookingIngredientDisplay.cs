using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CookingIngredientDisplay : ObjectEntryDisplay<CookingIngredient> {
		public ItemBrowserSlot ingredientSlot;
		public ItemBrowserSlot turnsIntoFoodSlot;
		
		protected override void OnRender(CookingIngredient entry) {
			ingredientSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Ingredient
			});
			turnsIntoFoodSlot.Icon = new CookedFoodSlotIcon(entry.TurnsIntoFood, entry.Ingredient, ObjectID.Egg);
		}
		
		protected override void OnRenderDescription(CookingIngredient entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/CookingIngredient_0",
				color = UserInterfaceUtility.DescriptionColor
			});
			
			description.AddPadding();

			foreach (var givesConditionsWhenConsumedBuffer in PugDatabase.GetBuffer<GivesConditionsWhenConsumedBuffer>(entry.Ingredient)) {
				var conditionData = givesConditionsWhenConsumedBuffer.conditionDataContainer.conditionDataWhenCooked;

				if (conditionData.conditionID != ConditionID.None && conditionData.value != 0) {
					var text = ConditionUI.GetConditionTextAndFormatFields(default, conditionData, false, false, false);
					text.color = UserInterfaceUtility.DescriptionColor;
					text.dontLocalizeFormatFields = true;
					description.AddLine(text);
				}
			}
		}
	}
}