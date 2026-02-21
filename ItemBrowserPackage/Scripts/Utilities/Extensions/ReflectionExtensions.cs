using System.Linq;
using PugMod;

namespace ItemBrowser.Utilities.Extensions {
	public static class ReflectionExtensions {
		public static T GetValue<T>(this object obj, string memberName) {
			var member = obj.GetType().GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == memberName);
			if (member == null)
				return default;
			
			return (T) API.Reflection.GetValue(member, obj);
		}
		
		public static void SetValue<T>(this object obj, string memberName, T value) {
			var member = obj.GetType().GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == memberName);
			if (member == null)
				return;
			
			API.Reflection.SetValue(member, obj, value);
		}
	}
}