using System.Collections.Generic;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
	public abstract class SlotIcon {
		public virtual ContainedObjectsBuffer ContainedObject => default;
		public virtual ContainedObjectsBuffer VisualObject => ContainedObject;
		public virtual (int Min, int Max) Amount => ContainedObject.objectID == ObjectID.None ? (VisualObject.amount, VisualObject.amount) : (ContainedObject.amount, ContainedObject.amount);

		public virtual void Update(SlotUIBase slot) { }

		public virtual bool ShowDetails(SlotUIBase slot, DetailsTab initialTab) {
			return false;
		}

		public virtual TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			return null;
		}

		public virtual List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
			return new List<TextAndFormatFields>();
		}

		public virtual List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
			return null;
		}
	}
}