using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using PugProperties;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record NaturalSpawnAroundObject : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/NaturalSpawnAroundObject", ObjectID.NatureCicadaSummoningItem, Priorities.NaturalSpawnAroundObject);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public (ObjectID Id, int Variation) Entity { get; set; }
		public float DespawnRadius { get; set; }
		public float SpawnRadius { get; set; }
		public (float Min, float Max) SpawnCooldown { get; set; }
		public int SpawnLimit { get; set; }
		public (float Min, float Max) SpawnLimitReachedCooldown { get; set; }
		public Season? SpawnsInSeason { get; set; }
		public List<Biome> SpawnsInBiomes { get; set; } = new();
		public bool NeedToBeInsideBiome { get; set; }
		public Tileset? SpawnsInTileset { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var allCritters = GetContinuouslySpawningCritters(allObjects);

				foreach (var (objectData, authoring) in allObjects) {
					if (!authoring.TryGetComponent<SpawnAroundObjectAuthoring>(out var spawnAroundObjectAuthoring))
						continue;

					foreach (var spawn in spawnAroundObjectAuthoring.spawnEntries) {
						if (spawn.spawnCrittersInsteadOfObject) {
							foreach (var critter in allCritters) {
								foreach (var tileset in critter.Tilesets) {
									var entry = new NaturalSpawnAroundObject {
										Result = (critter.Id, 0),
										Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
										DespawnRadius = spawn.critterDespawnDistance,
										SpawnRadius = spawn.maxSpawnDistance,
										SpawnCooldown = (spawn.minSpawnCooldown, spawn.maxSpawnCooldown),
										SpawnLimit = spawn.limitNumberSpawned,
										SpawnLimitReachedCooldown = (spawn.minReachedLimitCooldown, spawn.maxReachedLimitCooldown),
										SpawnsInSeason = spawn.onlySpawnsInSeason != Season.None ? spawn.onlySpawnsInSeason : null,
										SpawnsInBiomes = critter.Biomes,
										NeedToBeInsideBiome = spawn.playerNeedsToBeInsideBiome,
										SpawnsInTileset = tileset != Tileset.MAX_VALUE ? tileset : null
									};
									registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
								}
							}
						} else {
							var entry = new NaturalSpawnAroundObject {
								Result = (spawn.objectToSpawn.objectID, ObjectUtils.GetPrimaryVariation(spawn.objectToSpawn.objectID, spawn.objectToSpawn.variation)),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								DespawnRadius = spawn.critterDespawnDistance,
								SpawnRadius = spawn.maxSpawnDistance,
								SpawnCooldown = (spawn.minSpawnCooldown, spawn.maxSpawnCooldown),
								SpawnLimit = spawn.limitNumberSpawned,
								SpawnLimitReachedCooldown = (spawn.minReachedLimitCooldown, spawn.maxReachedLimitCooldown),
								SpawnsInSeason = spawn.onlySpawnsInSeason != Season.None ? spawn.onlySpawnsInSeason : null,
								SpawnsInBiomes = spawn.spawnsInBiome.ToList(),
								NeedToBeInsideBiome = spawn.playerNeedsToBeInsideBiome
							};
							registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
						}
					}
				}
			}

			private static List<(ObjectID Id, List<Biome> Biomes, List<Tileset> Tilesets)> GetContinuouslySpawningCritters(List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				return allObjects
					.Where(entry => PugDatabase.TryGetComponent<ObjectPropertiesCD>(entry.ObjectData, out var propertiesCD) && propertiesCD.Has(PropertyID.Critter.spawnContinuously))
					.GroupBy(entry => entry.ObjectData.objectID)
					.Select(group => {
						var objectData = group.First().ObjectData;
						var propertiesCD = PugDatabase.GetComponent<ObjectPropertiesCD>(objectData);

						var biomesToSpawnIn = propertiesCD.GetManagedList<Biome>(PropertyID.Critter.biomesToSpawnIn);
						var tilesetsToSpawnIn = propertiesCD.Has(PropertyID.Critter.tilesetsToSpawnIn)
							? propertiesCD.GetManagedList<Tileset>(PropertyID.Critter.tilesetsToSpawnIn)
							: new List<Tileset>();
						
						if (tilesetsToSpawnIn.Count == 0)
							tilesetsToSpawnIn.Add(Tileset.MAX_VALUE);
						
						return (
							objectData.objectID,
							biomesToSpawnIn,
							tilesetsToSpawnIn
						);
					})
					.ToList();
			}
		}
	}
}