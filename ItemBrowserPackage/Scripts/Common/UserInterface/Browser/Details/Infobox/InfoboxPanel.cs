using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class InfoboxPanel : DetailsSubView, IScrollable {
		public PugText testObjectName;
		public SpriteRenderer background;
		
		public float WindowWidth => background.size.x;

		public override void OnApplyState(DetailsState currentState, DetailsState previousState) {
			Render(currentState);
		}
		
		private void Render(DetailsState state) {
			testObjectName.Render(ObjectUtility.GetLocalizedDisplayNameOrDefault(state.ObjectData.objectID, state.ObjectData.variation));
		}
		
		public void UpdateContainingElements(float scroll) { }

		public bool IsBottomElementSelected() {
			return false;
		}

		public bool IsTopElementSelected() {
			return false;
		}

		public float GetCurrentWindowHeight() {
			return 0f;
		}
	}
}