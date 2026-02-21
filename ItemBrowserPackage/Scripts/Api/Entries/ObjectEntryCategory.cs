namespace ItemBrowser.Api.Entries {
	public readonly struct ObjectEntryCategory {
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
	}
}