using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class VirtualObjectListSlot : ItemBrowserSlot {
		public FiltersPanel filtersPanel;
		public ObjectListView objectListView;
		public PugText nameText;
		public PugText sortingValueText;
		public float discoveredNameOpacity = 0.75f;
		public float undiscoveredNameOpacity = 0.33f;
		public int maxNameTextLength = 15;
		public SpriteRenderer plusIcon;

		private ObjectDataCD _previousObjectData;
		private bool _wasDiscovered;
		private bool _wasCollected;
		private int _amount;

		protected virtual bool HasBeenCollected => true;

		public void SetObject(ObjectData objectData) {
			if (_previousObjectData.EqualsExact(objectData))
				return;

			Icon = new BasicSlotIcon(objectData);
			_amount = 0;
			_previousObjectData = objectData;

			UpdateVisuals();
		}

		public void SetExtraAmount(int amount) {
			_amount = amount;

			_previousObjectData = default;
			Icon = new BasicSlotIcon(_previousObjectData);

			UpdateVisuals();
		}

		public override void UpdateVisuals() {
			base.UpdateVisuals();

			if (nameText != null) {
				if (_amount > 0) {
					nameText.style.color = Manager.text.GetRarityColor(Rarity.Common).ColorWithNewAlpha(undiscoveredNameOpacity);
					nameText.SetTempColor(nameText.style.color);
					nameText.Render(string.Format(API.Localization.GetLocalizedTerm("ItemBrowser-General/PlusHiddenByFiltersShort"), _amount.ToString()));
				}
				else {
					var isDiscovered = HasBeenDiscovered;
					var isCollected = HasBeenCollected;

					var containedObjectData = Icon.ContainedObject.objectData;
					var nameColor = Manager.text.GetRarityColor(PugDatabase.GetObjectInfo(containedObjectData.objectID, containedObjectData.variation)?.rarity ?? Rarity.Common);

					nameText.style.color = nameColor.ColorWithNewAlpha((isDiscovered && isCollected) ? discoveredNameOpacity : undiscoveredNameOpacity);
					nameText.SetTempColor(nameText.style.color);
					nameText.Render(isDiscovered
						? UserInterfaceUtility.TruncateToFit(ObjectUtility.GetLocalizedDisplayNameOrDefault(containedObjectData), maxNameTextLength)
						: API.Localization.GetLocalizedTerm("ItemBrowser-General/Undiscovered")
					);
				}
			}

			if (sortingValueText != null) {
				var currentSorter = objectListView.CurrentSorter;
				if (nameText != null) {
					sortingValueText.style.color = nameText.style.color;
					sortingValueText.SetTempColor(sortingValueText.style.color);
				}

				if (currentSorter?.AdditionalInfoFunction != null)
					sortingValueText.Render(currentSorter.AdditionalInfoFunction(Icon.ContainedObject.objectData) ?? "");
				else
					sortingValueText.Render("");
			}

			if (plusIcon != null) {
				if (_amount > 0) {
					icon.gameObject.SetActive(false);
					missingIcon.gameObject.SetActive(false);
					plusIcon.gameObject.SetActive(true);
				}
				else {
					plusIcon.gameObject.SetActive(false);
				}
			}
		}

		public override List<PugDatabase.MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing) {
			var slotObjectData = GetSlotObject().objectData;

			if (!filtersPanel.DisplayItemCraftingRequirements && !PugDatabase.HasComponent<CookedFoodCD>(slotObjectData))
				return null;

			return MaterialInfoUtility.GetMaterialInfosForRecipe(slotObjectData);
		}

		public override CraftingSettings GetCraftingSettings() {
			var slotObjectData = GetSlotObject().objectData;

			if (!filtersPanel.DisplayItemCraftingRequirements && !PugDatabase.HasComponent<CookedFoodCD>(slotObjectData))
				return base.GetCraftingSettings();

			if (!PugDatabase.TryGetObjectInfo(slotObjectData.objectID, out var objectInfo, slotObjectData.variation))
				return base.GetCraftingSettings();

			return objectInfo.craftingSettings;
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (_amount > 0) {
				return new TextAndFormatFields {
					text = "ItemBrowser-General/PlusHiddenByFilters",
					formatFields = new[] {
						_amount.ToString()
					},
					dontLocalizeFormatFields = true
				};
			}

			return base.GetHoverTitle();
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return _amount > 0 ? GetDescriptionForExtraAmount() : GetDescriptionForObject();
		}

		private List<TextAndFormatFields> GetDescriptionForObject() {
			var lines = new List<TextAndFormatFields>();
			var slotObject = GetSlotObject();
			
			if (_amount == 0 && objectListView is CookingListView && PugDatabase.HasComponent<CookingIngredientCD>(slotObject.objectID)) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/CookingIngredient_0",
					color = Manager.ui.hoverTextSettings.canBeCookedColor
				});
			
				foreach (var givesConditionsWhenConsumedBuffer in PugDatabase.GetBuffer<GivesConditionsWhenConsumedBuffer>(slotObject.objectData)) {
					var conditionData = givesConditionsWhenConsumedBuffer.conditionDataContainer.conditionDataWhenCooked;

					if (conditionData.conditionID != ConditionID.None && conditionData.value != 0) {
						var text = ConditionUI.GetConditionTextAndFormatFields(default, conditionData, false, false, false);
						text.color = UserInterfaceUtility.AlmostWhiteColor;
						text.dontLocalizeFormatFields = true;

						lines.Add(text);
					}
				}
			}
			
			lines.AddRange(base.GetHoverDescription());

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

		private List<TextAndFormatFields> GetDescriptionForExtraAmount() {
			var lines = new List<TextAndFormatFields>();

			if (filtersPanel.FiltersToInclude.Any()) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-General/ActiveFiltersIncluding",
					color = UserInterfaceUtility.AlmostWhiteColor
				});

				foreach (var group in filtersPanel.FiltersToInclude) {
					lines.Add(new TextAndFormatFields {
						text = GetFilterGroupNamesTextLocalized(group.ToList()),
						dontLocalize = true,
						color = UserInterfaceUtility.DescriptionColor
					});
				}
			}

			if (filtersPanel.FiltersToExclude.Any()) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-General/ActiveFiltersExcluding",
					color = UserInterfaceUtility.AlmostWhiteColor
				});

				foreach (var group in filtersPanel.FiltersToExclude) {
					lines.Add(new TextAndFormatFields {
						text = GetFilterGroupNamesTextLocalized(group.ToList()),
						dontLocalize = true,
						color = UserInterfaceUtility.DescriptionColor
					});
				}
			}

			return lines;
		}

		private static string GetFilterGroupNamesTextLocalized(List<Filter> group) {
			var result = new StringBuilder();
			result.Append("- ");

			for (var i = 0; i < group.Count; i++) {
				var filter = group[i];

				result.Append(PugText.ProcessText(filter.Name, filter.NameFormatFields, true, filter.LocalizeNameFormatFields));
				if (i < group.Count - 1)
					result.Append(", ");
			}

			return result.ToString();
		}
	}
}