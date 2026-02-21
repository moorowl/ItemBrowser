using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record CookingIngredient : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/CookingIngredient", ObjectID.HeartBerry, VanillaPriorities.CookingIngredient);
		
		public ObjectID Ingredient { get; set; }
		public IngredientType IngredientType { get; set; }
		public ObjectID TurnsIntoFood { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CookingIngredientCD>(objectData, out var cookingIngredientCD))
						continue;

					var entry = new CookingIngredient {
						Ingredient = objectData.objectID,
						IngredientType = cookingIngredientCD.ingredientType,
						TurnsIntoFood = cookingIngredientCD.turnsIntoFood
					};
					registry.Register(ObjectEntryType.Usage, entry.Ingredient, 0, entry);
				}
			}
		}
	}
}