using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Api.Entries.Requirements.Types;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record NaturalSpawnAroundObject : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/NaturalSpawnAroundObject", ObjectID.NatureCicadaSummoningItem, VanillaPriorities.NaturalSpawnAroundObject);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public (ObjectID Id, int Variation) Entity { get; set; }
		public float DespawnRadius { get; set; }
		public float SpawnRadius { get; set; }
		public (float Min, float Max) SpawnCooldown { get; set; }
		public int SpawnLimit { get; set; }
		public (float Min, float Max) SpawnLimitReachedCooldown { get; set; }
		public List<Biome> SpawnsInBiomes { get; set; } = new();
		public bool NeedToBeInsideBiome { get; set; }
		public Tileset? SpawnsOnTileset { get; set; }
		public bool OnlySpawnsInCombat { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var allCritters = ObjectUtils.GetAllCritterSpawnAreas(allObjects);

				foreach (var (objectData, authoring) in allObjects) {
					// Have to use authoring because season-specific entries aren't converted
					if (!authoring.TryGetComponent<SpawnAroundObjectAuthoring>(out var spawnAroundObjectAuthoring))
						continue;

					foreach (var spawn in spawnAroundObjectAuthoring.spawnEntries) {
						if (spawn.spawnCrittersInsteadOfObject) {
							foreach (var critter in allCritters) {
								var entry = new NaturalSpawnAroundObject {
									Result = (critter.Id, 0),
									Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
									DespawnRadius = spawn.critterDespawnDistance,
									SpawnRadius = spawn.maxSpawnDistance,
									SpawnCooldown = (spawn.minSpawnCooldown, spawn.maxSpawnCooldown),
									SpawnLimit = spawn.limitNumberSpawned,
									SpawnLimitReachedCooldown = (spawn.minReachedLimitCooldown, spawn.maxReachedLimitCooldown),
									NeedToBeInsideBiome = spawn.playerNeedsToBeInsideBiome,
									OnlySpawnsInCombat = spawn.onlySpawnIfInCombat
								};

								if (spawn.onlySpawnsInSeason != Season.None)
									entry.AddRequirement(new SeasonActive(spawn.onlySpawnsInSeason));
								
								if (critter.Biomes.Count > 0) {
									registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry with {
										SpawnsInBiomes = critter.Biomes
									});
									registry.Register(ObjectEntryType.Usage, entry.Entity.Id, entry.Entity.Variation, entry with {
										SpawnsInBiomes = critter.Biomes
									});
								}

								foreach (var tileset in critter.Tilesets) {
									registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry with {
										SpawnsOnTileset = tileset
									});
									registry.Register(ObjectEntryType.Usage, entry.Entity.Id, entry.Entity.Variation, entry with {
										SpawnsOnTileset = tileset
									});
								}
							}
						} else {
							var entry = new NaturalSpawnAroundObject {
								Result = (spawn.objectToSpawn.objectID, ObjectUtils.GetPrimaryVariation(spawn.objectToSpawn)),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
								DespawnRadius = spawn.critterDespawnDistance,
								SpawnRadius = spawn.maxSpawnDistance,
								SpawnCooldown = (spawn.minSpawnCooldown, spawn.maxSpawnCooldown),
								SpawnLimit = spawn.limitNumberSpawned,
								SpawnLimitReachedCooldown = (spawn.minReachedLimitCooldown, spawn.maxReachedLimitCooldown),
								SpawnsInBiomes = spawn.spawnsInBiome.Where(biome => biome != Biome.None).ToList(),
								NeedToBeInsideBiome = spawn.playerNeedsToBeInsideBiome,
								OnlySpawnsInCombat = spawn.onlySpawnIfInCombat
							};
							
							if (spawn.onlySpawnsInSeason != Season.None)
								entry.AddRequirement(new SeasonActive(spawn.onlySpawnsInSeason));
							
							registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
							registry.Register(ObjectEntryType.Usage, entry.Entity.Id, entry.Entity.Variation, entry);
						}
					}
				}
			}
		}
	}
}