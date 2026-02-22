using Unity.Mathematics;

namespace ItemBrowser.Utilities.Extensions {
	public static class ScrollWindowExtensions {
		public static float GetScrollValue(this UIScrollWindow scrollWindow) {
			var scrollValue = 1f - (scrollWindow.scrollingContent.localPosition.y - scrollWindow.minScrollPos) / math.max(scrollWindow.ScrollHeight - scrollWindow.minScrollPos, 0.001f);
			if (float.IsInfinity(scrollValue) || float.IsNaN(scrollValue))
				scrollValue = 1f;
			
			return scrollValue;
		}
	}
}