using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class CraftingDisplay : ObjectEntryDisplay<Crafting> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot stationSlot;
		public ItemBrowserSlot[] ingredientSlots;
		public ItemBrowserSlot tagIngredientSlot;

		public override IEnumerable<Crafting> OnSort(IEnumerable<Crafting> entries) {
			return entries
				.OrderBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Result.Id, entry.Result.Variation))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.UsesStation ? entry.Station : entry.Recipe));
		}

		protected override void OnRender(Crafting entry) {
			GetCraftingInfo(entry.Result, out var materials, out var usesMaterialsWithTag);
			
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation,
				amount = entry.Amount
			});
			stationSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.UsesStation ? entry.Station : entry.Recipe
			}); 
			
			tagIngredientSlot.gameObject.SetActive(false);
			foreach (var slot in ingredientSlots)
				slot.gameObject.SetActive(false);

			if (usesMaterialsWithTag != ObjectCategoryTag.None) {
				tagIngredientSlot.gameObject.SetActive(true);
				tagIngredientSlot.DisplayedObject = new DisplayedObject.Tag(usesMaterialsWithTag);
			} else {
				for (var i = 0; i < materials.Count; i++) {
					if (i >= ingredientSlots.Length)
						break;
					
					var craftingObject = materials[i];
					var slot = ingredientSlots[i];
					slot.gameObject.SetActive(true);

					slot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
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
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.UsesStation ? entry.Station : entry.Recipe)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			
			// "Materials" header
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Crafting_1",
				color = UserInterfaceUtils.DescriptionColor
			});
			
			// Materials list
			if (usesMaterialsWithTag != ObjectCategoryTag.None) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Crafting_2",
					formatFields = new[] {
						API.Localization.GetLocalizedTerm($"ItemBrowser-ObjectCategoryNames/{usesMaterialsWithTag}"),
						"1"
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				foreach (var craftingObject in materials) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/Crafting_2",
						formatFields = new[] {
							ObjectUtils.GetLocalizedDisplayNameOrDefault(craftingObject.objectID),
							craftingObject.amount.ToString()
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}	
			}
			
			// Crafting time
			if (entry.CraftingTime > 0) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Crafting_3",
					formatFields = new[] {
						entry.CraftingTime.ToString(LocalizationManager.CurrentCulture)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});		
			}
			
			// Crafted at nearby object
			if (entry.RequiresObjectNearby != ObjectID.None) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Crafting_4",
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.RequiresObjectNearby)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
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