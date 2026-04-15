using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ObjectGridSlot : ItemBrowserSlot {
		public FiltersPanel filtersPanel;
		public GridView gridView;

		private ObjectDataCD _previousObjectData;
		
		public void SetObject(ObjectData objectData, ObjectGrid craftingSelectorUI) {
			if (!_previousObjectData.EqualsExact(objectData))
				Icon = new BasicSlotIcon(objectData);

			//slotsUIContainer = craftingSelectorUI;
		}

		public override void OnFavoritedStateChanged() {
			gridView.RequestListRefresh(true);
		}

		public override List<PugDatabase.MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing) {
			if (!filtersPanel.DisplayItemCraftingRequirements)
				return null;
			
			var craftingHandler = Manager.main.player?.playerCraftingHandler;
			if (craftingHandler == null)
				return null;

			var slotObject = GetSlotObject();
			var objectInfo = PugDatabase.GetObjectInfo(slotObject.objectID, slotObject.variation);
			if (objectInfo == null)
				return null;
					
			var nearbyChests = craftingHandler.GetNearbyChests();
			var recipeInfo = new CraftingHandler.RecipeInfo(objectInfo, 1);

			return craftingHandler.GetCraftingMaterialInfosForRecipe(recipeInfo, nearbyChests, false, false, PugDatabase.HasComponent<CookedFoodCD>(slotObject.objectData));
		}

		public override CraftingSettings GetCraftingSettings() {
			var slotObject = GetSlotObject();
			if (!filtersPanel.DisplayItemCraftingRequirements || !PugDatabase.TryGetObjectInfo(slotObject.objectID, out var objectInfo, slotObject.variation))
				return base.GetCraftingSettings();
			
			return objectInfo.craftingSettings;
		}
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = base.GetHoverDescription();
			
			var slotObject = GetSlotObject();
			if (filtersPanel.DisplayItemCraftingRequirements) {
				var craftingSources = ItemBrowserAPI.ObjectEntryRegistry.GetEntries<Crafting>(ObjectEntryType.Source, slotObject.objectID, slotObject.variation).ToList();
				if (craftingSources.Count > 0) {
					lines[^1].paddingBeneath = UserInterfaceUtility.DescriptionPadding;
					foreach (var craftingSource in craftingSources) {
						lines.Add(new TextAndFormatFields {
							text = craftingSource.UsesStation ? "ItemBrowser-ObjectEntryDescriptions/Crafting_0_Station" : "ItemBrowser-ObjectEntryDescriptions/Crafting_0_Recipe",
							formatFields = new[] {
								ObjectUtility.GetLocalizedDisplayNameOrDefault(craftingSource.UsesStation ? craftingSource.Station : craftingSource.Recipe)
							},
							dontLocalizeFormatFields = true,
							color = UserInterfaceUtility.AlmostWhiteColor
						});
					}
				}
			}
			
			return lines;
		}
	}
}