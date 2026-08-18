using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CraftingDisplay : ObjectEntryDisplay<Crafting> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot stationSlot;
		public ItemBrowserSlot[] ingredientSlots;
		public ItemBrowserSlot tagIngredientSlot;

		public override IEnumerable<Crafting> OnSort(IEnumerable<Crafting> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Result.Id, entry.Result.Variation))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.UsesStation ? entry.Station : entry.Recipe));
		}

		protected override void OnRender(Crafting entry) {
			GetCraftingInfo(entry.Result, out var materials, out var usesMaterialsWithTag);
			
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation,
				amount = entry.Amount
			});
			stationSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.UsesStation ? entry.Station : entry.Recipe
			}); 
			
			tagIngredientSlot.gameObject.SetActive(false);
			foreach (var slot in ingredientSlots)
				slot.gameObject.SetActive(false);

			if (usesMaterialsWithTag != ObjectCategoryTag.None) {
				tagIngredientSlot.gameObject.SetActive(true);
				tagIngredientSlot.Icon = new TagSlotIcon(usesMaterialsWithTag);
			} else {
				for (var i = 0; i < materials.Count; i++) {
					if (i >= ingredientSlots.Length)
						break;
					
					var craftingObject = materials[i];
					var slot = ingredientSlots[i];
					slot.gameObject.SetActive(true);

					slot.Icon = new BasicSlotIcon(new ObjectDataCD {
						objectID = craftingObject.objectID,
						amount = craftingObject.amount
					});
				}
			}
		}

		protected override void OnRenderDescription(Crafting entry, EntryDescriptionButton description) {
			GetCraftingInfo(entry.Result, out var materials, out var usesMaterialsWithTag);
			
			// Crafted at
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Crafting_0_" + (entry.UsesStation ? "Station" : "Recipe"),
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.UsesStation ? entry.Station : entry.Recipe)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			
			description.AddMaterialsFor(entry.Result.Id);

			// Crafting time
			if (entry.CraftingTime > 0) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Crafting_3",
					formatFields = new[] {
						entry.CraftingTime.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});		
			}
			
			// Crafted at nearby object
			if (entry.RequiresObjectNearby != ObjectID.None) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Crafting_4",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.RequiresObjectNearby)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
		
		private static void GetCraftingInfo((ObjectID Id, int Variation) item, out List<CraftingObject> materials, out ObjectCategoryTag usesMaterialsWithTag) {
			var objectInfo = PugDatabase.GetObjectInfo(item.Id, item.Variation);
			materials = objectInfo.requiredObjectsToCraft.Where(craftingObject => craftingObject.objectID != ObjectID.None)
				.GroupBy(craftingObject => craftingObject.objectID)
				.Select(group => new CraftingObject {
					objectID = group.Key,
					amount = group.Sum(craftingObject => craftingObject.amount)
				})
				.ToList();
			usesMaterialsWithTag = objectInfo.craftingSettings.canOnlyUseAnyMaterialsWithTag;
		}
	}
}