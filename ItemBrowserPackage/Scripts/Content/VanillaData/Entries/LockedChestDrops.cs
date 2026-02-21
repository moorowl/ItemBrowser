using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record LockedChestDrops : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/LockedChestDrops", ObjectID.WallStoneBlock, VanillaPriorities.LockedChestDrops);
		
		public ObjectID Result { get; set; }
		public Biome RequiredBiome { get; set; }
		public Tileset RequiredTileset { get; set; }
		public float Chance { get; set; }
		
		public class Provider : ObjectEntryProvider {
			private static readonly MemberInfo MiBiomeAndTilesetToChest = typeof(DropLootSystem).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "BiomeAndTilesetToChest");
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var biome in Enum.GetValues(typeof(Biome)).Cast<Biome>()) {
					foreach (var tileset in Enum.GetValues(typeof(Tileset)).Cast<Tileset>()) {
						var chest = (ObjectID) API.Reflection.Invoke(MiBiomeAndTilesetToChest, null, biome, tileset);
						if (chest == ObjectID.None)
							continue;
						
						registry.Register(ObjectEntryType.Source, chest, 0, new LockedChestDrops {
							Result = chest,
							RequiredBiome = biome,
							RequiredTileset = tileset,
							// from DropLootSystem, this isn't a const anywhere?
							Chance = 0.0027027028f
						});
					}
				}
			}
		}
	}
}