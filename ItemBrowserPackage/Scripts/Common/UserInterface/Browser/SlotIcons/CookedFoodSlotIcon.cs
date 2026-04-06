using ItemBrowser.Utilities;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
	public class CookedFoodSlotIcon : SlotIcon {
		public override ContainedObjectsBuffer VisualObject => new() {
			objectData = _objectData
		};

		private readonly string _name;
		private readonly ObjectDataCD _objectData;

		public CookedFoodSlotIcon(ObjectID id, ObjectID primaryIngredient = ObjectID.Egg, ObjectID secondaryIngredient = ObjectID.HeartBerry) {
			_objectData = new ObjectDataCD {
				objectID = id,
				variation = CookedFoodCD.GetFoodVariation(primaryIngredient, secondaryIngredient)
			};
			_name = ObjectUtility.GetInternalName(id).Replace("Rare", "").Replace("Epic", "");
		}

		public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			return new TextAndFormatFields {
				text = $"Items/{_name}"
			};
		}
	}
}