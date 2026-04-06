namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class Group {
		public delegate (string Group, int Priority, bool Localize) GroupDelegate(ObjectDataCD item);
		
		public readonly string Name;

		public bool Localize { get; set; } = true;
		public GroupDelegate Function { get; set; }

		public Group(string name) {
			Name = name;
		}
	}
}