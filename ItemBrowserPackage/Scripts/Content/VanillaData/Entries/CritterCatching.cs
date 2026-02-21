using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using Pug.Automation;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record CritterCatching : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/CritterCatching", ObjectID.CritterCatcher, VanillaPriorities.CritterCatching);
		
		public (ObjectID Id, int Variation) Critter { get; set; }
		public List<Biome> SpawnsInBiomes { get; set; } = new();
		public Tileset? SpawnsOnTileset { get; set; }
		public (float Min, float Max) TimeToCatch { get; set; }
		public (ObjectID Id, int Variation) CritterCatcher { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var allCritters = ObjectUtils.GetAllCritterSpawnAreas(allObjects)
					.Where(critter => PugDatabase.HasComponent<CritterCatcherCatchableCD>(critter.Id))
					.ToList();

				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CritterCatchingCD>(objectData, out var critterCatchingCD))
						continue;

					foreach (var critter in allCritters) {
						var entry = new CritterCatching {
							Critter = (critter.Id, 0),
							TimeToCatch = (critterCatchingCD.minMaxRandomDefaultCraftingTime.x, critterCatchingCD.minMaxRandomDefaultCraftingTime.y),
							CritterCatcher = (objectData.objectID, objectData.variation)
						};

						if (critter.Biomes.Count > 0) {
							registry.Register(ObjectEntryType.Source, entry.Critter.Id, entry.Critter.Variation, entry with {
								SpawnsInBiomes = critter.Biomes
							});
							registry.Register(ObjectEntryType.Usage, objectData, entry with {
								SpawnsInBiomes = critter.Biomes
							});
						}

						foreach (var tileset in critter.Tilesets) {
							registry.Register(ObjectEntryType.Source, entry.Critter.Id, entry.Critter.Variation, entry with {
								SpawnsOnTileset = tileset
							});
							registry.Register(ObjectEntryType.Usage, objectData, entry with {
								SpawnsOnTileset = tileset
							});
						}
					}
				}
			}
		}
	}
}