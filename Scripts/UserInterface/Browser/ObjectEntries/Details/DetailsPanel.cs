using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class DetailsPanel : UIelement, IScrollable {
		[SerializeField]
		private PugText testObjectName;
		[SerializeField]
		private SpriteRenderer background;

		private ObjectDataCD _objectData;
		
		public bool IsShowing {
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}
		public float WindowWidth => background.size.x;
		
		public void SetObjectData(ObjectDataCD objectData) {
			_objectData = objectData;
			Render();
		}

		private void Render() {
			testObjectName.Render(ObjectUtils.GetLocalizedDisplayNameOrDefault(_objectData.objectID, _objectData.variation));
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