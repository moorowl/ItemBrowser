using System;

namespace ItemBrowser.Common.Api.Entries {
	public readonly struct ObjectEntryCategory : IEquatable<ObjectEntryCategory> {
		public readonly string Title;
		public readonly string TitleForNonObtainable;
		public readonly ObjectID Icon;
		public readonly int Priority;
		
		public ObjectEntryCategory(string title, string titleForNonObtainable, ObjectID icon, int priority = 0) {
			Title = title;
			TitleForNonObtainable = titleForNonObtainable;
			Icon = icon;
			Priority = priority;
		}
		
		public ObjectEntryCategory(string title, ObjectID icon, int priority = 0) : this(title, title, icon, priority) { }

		public string GetTitle(bool isNonObtainable) {
			return isNonObtainable ? TitleForNonObtainable : Title;
		}

		public bool Equals(ObjectEntryCategory other) {
			return Title == other.Title && TitleForNonObtainable == other.TitleForNonObtainable && Icon == other.Icon && Priority == other.Priority;
		}

		public override bool Equals(object obj) {
			return obj is ObjectEntryCategory other && Equals(other);
		}

		public override int GetHashCode() {
			return HashCode.Combine(Title, TitleForNonObtainable, (int)Icon, Priority);
		}
	}
}