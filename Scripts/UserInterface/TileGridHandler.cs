using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.UserInterface {
	public class TileGridHandler : MonoBehaviour {
		[SerializeField]
		private GameObject root;
		[SerializeField]
		private float yRenderOffset = 5f;

		public bool IsShowing { get; private set; }

		private void Awake() {
			IsShowing = false;
			root.SetActive(false);
		}

		private void LateUpdate() {
			if (InputHandler.IsToggleTileGridPressed)
				IsShowing = !IsShowing;
			
			root.SetActive(IsShowing && !Manager.prefs.hideInGameUI);
			if (!root.activeSelf)
				return;
			
			var tilePosition = Manager.camera.smoothedCameraPosition.RoundToInt2() / 16;
			transform.localPosition = new Vector3(tilePosition.x * 16f - 0.5f, yRenderOffset, tilePosition.y * 16f - yRenderOffset - 0.5f);
		}
	}
}