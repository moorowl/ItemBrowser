using ItemBrowser.Common.Api.Entries;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record DropsStructure : Drops {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/DropsStructure", ObjectID.PoisonSlime, VanillaPriorities.Drops - 1);
	}
}