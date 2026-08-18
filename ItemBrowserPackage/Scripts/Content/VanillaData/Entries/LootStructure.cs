using ItemBrowser.Common.Api.Entries;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record LootStructure : Loot {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/LootStructure", ObjectID.IronChest, VanillaPriorities.Loot - 1);
	}
}