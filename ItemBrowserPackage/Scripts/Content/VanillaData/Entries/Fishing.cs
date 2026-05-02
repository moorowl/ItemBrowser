using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Fishing : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Fishing", ObjectID.IronFishingRod, VanillaPriorities.Fishing);
		
		public ObjectID Result { get; set; }
		public Biome Biome { get; set; }
		public Tileset Tileset { get; set; }
		public CatchType Type { get; set; }
		public float Chance { get; set; }
		
		public enum CatchType {
			Fish,
			Loot
		}

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var fishingTable = Manager.mod.FishingTable;
				var biomeLoot = fishingTable.fishingInfos.Where(info => info.biomes.Count > 0);
				var liquidLoot = fishingTable.fishingInfos.Where(info => info.waterTilesets.Count > 0);
				
				foreach (var info in liquidLoot) {
					var tilesets = info.waterTilesets;
					if (tilesets.Count == 0 || tilesets[0] == Tileset.Dirt)
						continue;
					
					AddEntriesFromTable(LootUtility.GetLootTableHelper(info.lootTableID), CatchType.Loot, new List<Biome>(), tilesets);
					AddEntriesFromTable(LootUtility.GetLootTableHelper(info.fishLootTableID), CatchType.Fish, new List<Biome>(), tilesets);
				}
				
				foreach (var info in biomeLoot) {
					var biomes = info.biomes;
					if (biomes.Count == 0 || biomes.Contains(Biome.None))
						continue;

					AddEntriesFromTable(LootUtility.GetLootTableHelper(info.lootTableID), CatchType.Loot, biomes, new List<Tileset>());
					AddEntriesFromTable(LootUtility.GetLootTableHelper(info.fishLootTableID), CatchType.Fish, biomes, new List<Tileset>());
				}

				void AddEntriesFromTable(LootUtility.LootTableHelper helper, CatchType catchType, List<Biome> biomes, List<Tileset> tilesets) {
					foreach (var drop in helper.RandomPool) {
						foreach (var tileset in tilesets) {
							var entry = new Fishing {
								Result = drop.Item,
								Tileset = tileset,
								Type = catchType,
								Chance = drop.Chance
							};
							registry.Register(ObjectEntryType.Source, drop.Item, 0, entry);
							
							if (TileUtility.TryGetAssociatedObject(TileType.water, entry.Tileset, out var associatedTilesetObject))
								registry.Register(ObjectEntryType.Usage, associatedTilesetObject, entry);
						}
						
						foreach (var biome in biomes) {
							var entry = new Fishing {
								Result = drop.Item,
								Biome = biome,
								Type = catchType,
								Chance = drop.Chance
							};
							registry.Register(ObjectEntryType.Source, drop.Item, 0, entry);
							registry.Register(ObjectEntryType.Usage, ObjectID.Water, 0, entry);
						}
					}
				}
			}
		}
	}
}