using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record NaturalSpawnInitial : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/NaturalSpawnInitial", ObjectID.Bush, VanillaPriorities.NaturalSpawnInitial);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public EnvironmentSpawnType Type { get; set; }
		public int AmountToSpawn { get; set; }
		public Tileset? TilesetToSpawnOn { get; set; }
		public float ClusterSpawnChance { get; set; }
		public float ClusterSpreadChance { get; set; }
		public bool ClusterSpreadFourWayOnly { get; set; }
		public EnvironmentSpawnData.SpawnCheck SpawnCheck { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var spawnTable = Manager.mod.SpawnTable;
				
				foreach (var spawnData in spawnTable.spawnObjects) {
					var tilesets = spawnData.spawnCheck.tilesets;
					if (tilesets.Count == 0)
						tilesets.Add(Tileset.MAX_VALUE);
					
					foreach (var spawn in spawnData.spawns) {
						var variations = new List<ValueWithWeight<int>>();
						if (spawn.advancedVariationControl) {
							variations.AddRange(spawn.weightedVariations.value);
						} else {
							for (var i = spawn.variation.min; i <= spawn.variation.max; i++)
								variations.Add(new ValueWithWeight<int>(i, 1f));
						}

						variations.RemoveAll(variation => !ObjectUtils.IsPrimaryVariation(spawn.objectID, variation.value));
						if (variations.Count == 0)
							variations.Add(new ValueWithWeight<int>(0, 1f));

						foreach (var variation in variations) {
							foreach (var tileset in tilesets) {
								var entry = new NaturalSpawnInitial {
									Result = (TryReplaceResultingObject(spawn.objectID), variation.value),
									Type = spawn.spawnType,
									AmountToSpawn = spawn.amount,
									TilesetToSpawnOn = tileset == Tileset.MAX_VALUE ? null : tileset,
									ClusterSpawnChance = spawn.clusterSpawnChance,
									ClusterSpreadChance = spawn.clusterSpreadChance,
									ClusterSpreadFourWayOnly = spawn.clusterSpreadFourWayOnly,
									SpawnCheck = spawnData.spawnCheck
								};
								registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
							}
						}
					}
				}
			}

			private static ObjectID TryReplaceResultingObject(ObjectID id) {
				if (PugDatabase.TryGetComponent<PlantCD>(id, out var plantCD))
					return plantCD.objectToDropWhenHarvested;

				return id;
			}
		}
	}
}