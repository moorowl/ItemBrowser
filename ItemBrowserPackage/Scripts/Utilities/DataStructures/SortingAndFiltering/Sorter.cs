namespace ItemBrowser.Utilities.DataStructures.SortingAndFiltering {
	public class Sorter<T> {
		public delegate int SorterDelegate(T item);

		public readonly string Name;
		public bool Localize { get; set; } = true;
		public SorterDelegate Function { get; set; }

		public Sorter(string name) {
			Name = name;
		}
	}
}