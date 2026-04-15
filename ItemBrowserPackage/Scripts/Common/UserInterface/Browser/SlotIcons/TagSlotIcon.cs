using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Input;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
	public class TagSlotIcon : SlotIcon {
		public override ContainedObjectsBuffer VisualObject => new() {
			objectData = _objectsToDisplay.CurrentObjectData
		};

		private readonly ObjectCategoryTag _tag;
		private readonly int _amount;
		private readonly CyclingObjectData _objectsToDisplay;

		public TagSlotIcon(ObjectCategoryTag tag, int amount = 1) {
			_tag = tag;
			_objectsToDisplay = new CyclingObjectData(GetObjectsToDisplay(tag).Select(objectData => new ObjectDataCD {
				objectID = objectData.objectID,
				variation = objectData.variation,
				amount = amount
			}));
		}

		public override void Update(SlotUIBase slot) {
			_objectsToDisplay.Update(slot);
		}

		public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			return new TextAndFormatFields {
				text = $"ItemBrowser-ObjectCategoryNames/{_tag}"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
			var lines = new List<TextAndFormatFields>();

			if (InputHelper.IsShowTechnicalInfoHeld) {
				lines.Add(new TextAndFormatFields {
					text = _tag.ToString(),
					dontLocalize = true
				});
			}

			ObjectUtility.GetTotalAmountInInventoryAndNearbyChests(Manager.main.player, _tag, out var inInventory, out var inNearbyChests);

			if (inInventory > 0 || inNearbyChests > 0) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-General/AmountInInventory",
					formatFields = new[] {
						(inInventory + inNearbyChests).ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.AlmostWhiteColor
				});
			}

			return lines;
		}

		private static IEnumerable<ObjectDataCD> GetObjectsToDisplay(ObjectCategoryTag tag) {
			var allObjectsWithTag = ObjectUtility.GetAllObjectsWithTag(tag);
			return tag switch {
				ObjectCategoryTag.UncommonOrLowerCookedFood or ObjectCategoryTag.RareOrHigherCookedFood => allObjectsWithTag.Select(objectData => new ObjectDataCD {
					objectID = objectData.objectID,
					variation = CookedFoodCD.GetFoodVariation(ObjectID.HeartBerry, ObjectID.GlowingTulipFlower)
				}),
				ObjectCategoryTag.CattlePlantFood => allObjectsWithTag.Where(objectData => !PugDatabase.HasComponent<PlantCD>(objectData)),
				_ => allObjectsWithTag
			};
		}
	}
}