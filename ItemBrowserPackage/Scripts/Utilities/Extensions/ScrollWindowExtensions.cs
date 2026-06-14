using System.Linq;
using PugMod;
using Unity.Mathematics;

namespace ItemBrowser.Utilities.Extensions {
	public static class ScrollWindowExtensions {
		private static readonly MemberInfo MiScrollable = typeof(UIScrollWindow).GetMembersChecked().First(x => x.GetNameChecked() == "_scrollable");
		private static readonly MemberInfo MiUpdateScrollHeight = typeof(UIScrollWindow).GetMembersChecked().First(x => x.GetNameChecked() == "UpdateScrollHeight");
		
		public static void SetScrollValueImmediately(this UIScrollWindow scrollWindow, float value, IScrollable scrollable) {
			// Update scroll height immediately, since it only happens normally every LateUpdate
			// Assign scrollable in case Awake hasn't been called on the scroll window yet
			API.Reflection.SetValue(MiScrollable, scrollWindow, scrollable);
			API.Reflection.Invoke(MiUpdateScrollHeight, scrollWindow);
			scrollWindow.SetScrollValue(value);
		}
		
		public static void ResetScrollValueImmediately(this UIScrollWindow scrollWindow, IScrollable scrollable) {
			scrollWindow.SetScrollValueImmediately(1f, scrollable);
		}
		
		public static float GetScrollValue(this UIScrollWindow scrollWindow) {
			var scrollValue = 1f - (scrollWindow.scrollingContent.localPosition.y - scrollWindow.minScrollPos) / math.max(scrollWindow.ScrollHeight - scrollWindow.minScrollPos, 0.001f);
			if (float.IsInfinity(scrollValue) || float.IsNaN(scrollValue))
				scrollValue = 1f;
			
			return scrollValue;
		}
	}
}