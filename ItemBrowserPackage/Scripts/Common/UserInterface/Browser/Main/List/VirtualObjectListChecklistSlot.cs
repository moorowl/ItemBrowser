using System.Collections.Generic;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.Options.DiscoveredObjects;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class VirtualObjectListChecklistSlot : VirtualObjectListSlot {
		public GameObject optionalCollectedMarker;

		protected override bool HasBeenCollected => OptionsManager.Instance.HasTag(Icon.ContainedObject.objectData, ObjectTagType.Collected);

		public void TryToggleCollected() {
			var objectData = Icon.ContainedObject.objectData;
            if (objectData.objectID == ObjectID.None || !HasBeenDiscovered)
            	return;

            if (HasBeenCollected) {
	            // mark as not collected
	            OptionsManager.Instance.RemoveTag(objectData, ObjectTagType.Collected);
	            OptionsManager.Instance.AddTag(objectData, ObjectTagType.Uncollected);
	            
	            ItemBrowserSounds.PlayUnfavorited(this); 
            } else {
	            // mark as collected
	            OptionsManager.Instance.RemoveTag(objectData, ObjectTagType.Uncollected);
	            OptionsManager.Instance.AddTag(objectData, ObjectTagType.Collected);
				
	            ItemBrowserSounds.PlayFavorited(this); 
            }

            PlayBumpAnimation();
            UpdateVisuals();
		}

		public override void UpdateVisuals() {
			base.UpdateVisuals();

			if (optionalCollectedMarker != null)
				optionalCollectedMarker.SetActive(HasBeenCollected);
		}

		private void UpdateCollectedMarker() {
			var objectData = Icon.ContainedObject.objectData;
			if (optionalCollectedMarker != null)
				optionalCollectedMarker.SetActive(objectData.objectID != ObjectID.None && OptionsManager.Instance.HasTag(objectData, ObjectTagType.Collected));
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (HasBeenDiscovered)
				TryShowButtonHint(HasBeenCollected ? ButtonHint.RemoveCollected : ButtonHint.AddCollected);
			
			UpdateCollectedMarker();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			TryToggleCollected();
			
			base.OnLeftClicked(mod1, mod2);
		}

		protected override bool ShouldIconBeHidden() {
			return base.ShouldIconBeHidden() || !HasBeenCollected;
		}
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = base.GetHoverDescription();
			
			var objectData = Icon.ContainedObject.objectData;
			if (objectData.objectID == ObjectID.None)
				return lines;

			var hasBeenCollected = HasBeenCollected;
			var hasBeenDiscovered = DiscoveredTracker.HasBeenDiscovered(objectData);

			if (hasBeenCollected) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-General/Collected",
					color = Color.green
				});
			} else if (hasBeenDiscovered) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-General/Discovered",
					color = new Color(0.56f, 0.7f, 1f) // cornflower blue
				});
			}
			
			return lines;
		}
	}
}