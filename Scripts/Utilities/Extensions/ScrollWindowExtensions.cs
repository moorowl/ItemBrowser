using Unity.Mathematics;

namespace ItemBrowser.Utilities.Extensions {
	public static class ScrollWindowExtensions {
		public static float GetScrollValue(this UIScrollWindow scrollWindow) {
			return 1f - ((scrollWindow.scrollingContent.localPosition.y - scrollWindow.minScrollPos) / math.max(scrollWindow.ScrollHeight - scrollWindow.minScrollPos, 0.001f));
		}
	}
}