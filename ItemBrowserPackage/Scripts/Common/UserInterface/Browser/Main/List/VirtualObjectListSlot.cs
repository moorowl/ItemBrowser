using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;
using UnityEngine.Serialization;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class VirtualObjectListSlot : ItemBrowserSlot {
		public FiltersPanel filtersPanel;
		public ObjectListView objectListView;
		public PugText nameText;
		public float discoveredNameOpacity = 0.75f;
		public float undiscoveredNameOpacity = 0.33f;
		public int maxNameTextLength = 15;
		
		private ObjectDataCD _previousObjectData;
		private bool _wasDiscovered;
		
		public void SetObject(ObjectData objectData) {
			if (_previousObjectData.EqualsExact(objectData))
				return;

			Icon = new BasicSlotIcon(objectData);
			UpdateNameText(HasBeenDiscovered);
				
			_previousObjectData = objectData;
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			if (nameText != null) {
				var isDiscovered = HasBeenDiscovered;
				if (isDiscovered != _wasDiscovered) {
					UpdateNameText(isDiscovered);
					_wasDiscovered = isDiscovered;
				}
			}
		}

		private void UpdateNameText(bool isDiscovered) {
			if (nameText == null)
				return;

			var containedObjectData = Icon.ContainedObject.objectData;
			var nameColor = Manager.text.GetRarityColor(PugDatabase.GetObjectInfo(containedObjectData.objectID, containedObjectData.variation)?.rarity ?? Rarity.Common);

			nameText.style.color = nameColor.ColorWithNewAlpha(isDiscovered ? discoveredNameOpacity : undiscoveredNameOpacity);
			nameText.Render(isDiscovered
				? UserInterfaceUtility.TruncateToFit(ObjectUtility.GetLocalizedDisplayNameOrDefault(containedObjectData), maxNameTextLength)
				: API.Localization.GetLocalizedTerm("ItemBrowser-General/Undiscovered")
			);
		}

		public override void OnFavoritedStateChanged() {
			objectListView.RequestListRefresh(true);
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