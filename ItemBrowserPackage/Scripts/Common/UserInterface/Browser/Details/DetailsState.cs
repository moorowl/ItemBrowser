using System;

namespace ItemBrowser.Common.UserInterface.Browser {
	public record DetailsState {
		public ObjectDataCD ObjectData { get; set; }
		public DetailsTab Tab { get; set; }
		public int EntriesSourceCategory { get; set; }
		public string EntriesSourceCategoryTerm { get; set; }
		public float EntriesSourceScrollProgress { get; set; } = 1f;
		public int EntriesUsageCategory { get; set; }
		public string EntriesUsageCategoryTerm { get; set; }
		public float EntriesUsageScrollProgress { get; set; } = 1f;
		public long Timestamp { get; set; } = DateTime.Now.ToFileTimeUtc();

		public bool EqualsForHistory(DetailsState other) {
			if (ObjectData.Equals(other.ObjectData) && Tab == other.Tab) {
				return Tab switch {
					DetailsTab.Sources => EntriesSourceCategory == other.EntriesSourceCategory,
					DetailsTab.Usages => EntriesUsageCategory == other.EntriesUsageCategory,
					_ => false
				};
			}

			return false;
		}
	}
}