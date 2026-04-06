using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using PugMod;
using PugTilemap;
using PugWorldGen;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record StructureContents : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/StructureContents", "ItemBrowser-ObjectEntryNames/StructureContents_NonObtainable", ObjectID.RuinsCavelingPillar, VanillaPriorities.StructureContents);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; set; }
		public string Scene { get; set; }
		public string Dungeon { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				ref var customSceneTable = ref ClientWorldStateSystem.CustomSceneTable.Value;

				foreach (var scene in StructureUtility.AllCustomScenes) {
					ref var sceneBlob = ref customSceneTable.scenes[scene.IndexInCustomSceneTable];
					
					var allObjectDatas = new List<ObjectDataCD>();
					
					// Add normal objects
					for (var i = 0; i < sceneBlob.prefabObjectDatas.Length; i++) {
						var objectData = sceneBlob.prefabObjectDatas[i];
						var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
						
						// Some scenes contain objects that can't be placed for some reason
						if (objectInfo == null || objectInfo.prefabInfos[0].prefab == null)
							continue;
						
						allObjectDatas.Add(new ObjectDataCD {
							objectID = objectData.objectID,
							variation = ObjectUtility.GetPrimaryVariation(objectData),
							amount = 1
						});
					}

					// Add tiles
					for (var i = 0; i < sceneBlob.tiles.Length; i++) {
						var tile = sceneBlob.tiles[i];
						
						var tileType = tile.tileType;
						if (tileType == TileType.ground)
							tileType = TileType.wall;
						var tileset = tile.tileset;
						if (tileType == TileType.bigRoot && tileset == (int) Tileset.Nature)
							tileset = (int) Tileset.Dirt;
						
						var objectInfo = PugDatabase.TryGetTileItemInfo(tileType, tileset);
						if (objectInfo != null) {
							allObjectDatas.Add(TryReplaceResultingObject(new ObjectDataCD {
								objectID = objectInfo.objectID,
								variation = objectInfo.variation,
								amount = 1
							}));
						}
					}

					var combinedObjectDatas = ObjectUtility.GroupAndSumObjects(allObjectDatas, false);

					foreach (var objectData in combinedObjectDatas) {
						var entry = new StructureContents {
							Result = (objectData.objectID, objectData.variation, objectData.amount),
							Scene = scene.Name
						};
						
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					}
				}

				var allDungeons = StructureUtility.AllRandomDungeons.Select(x => (x.Name, x.Entity))
					.Union(StructureUtility.AllUniqueDungeons.Select(x => (x.Name, x.Entity)))
					.ToList();

				// Dungeons
				foreach (var dungeon in allDungeons) {
					var objectsThatSpawn = new HashSet<ObjectDataCD>();
					var objectsThatCouldHaveSpawned = new HashSet<ObjectID>();
					var roomsThatSpawn = new HashSet<RoomFlags>();
					
					if (EntityUtility.TryGetBuffer<DungeonRoomPlacementBuffer>(dungeon.Entity, API.Client.World, out var dungeonRoomPlacementBuffer)) {
						foreach (var dungeonRoomPlacement in dungeonRoomPlacementBuffer) {
							var room = dungeonRoomPlacement.Value;
							if (room.amount.max <= 0)
								continue;

							roomsThatSpawn.UnionWith(StructureUtility.SeparateFlags(room.roomType));
						}
					}
					
					if (EntityUtility.TryGetBuffer<DungeonNodeTemplateBuffer>(dungeon.Entity, API.Client.World, out var dungeonNodeTemplateBuffer)) {
						foreach (var dungeonNodeTemplate in dungeonNodeTemplateBuffer) {
							var nodeFlags = StructureUtility.SeparateFlags(dungeonNodeTemplate.flags);
							var nodeEntity = dungeonNodeTemplate.spawnTemplateBufferEntity;

							if (!EntityUtility.TryGetBuffer<DungeonNodeSpawnTemplateBuffer>(nodeEntity, API.Client.World, out var dungeonNodeSpawnTemplateBuffer))
								continue;

							if (!roomsThatSpawn.Any(room => nodeFlags.Contains(room)))
								continue;

							foreach (var dungeonNodeSpawnTemplate in dungeonNodeSpawnTemplateBuffer)
								TryAddObjectsFromSpawnTemplate(ref dungeonNodeSpawnTemplate.Value.Value);
						}
					}

					if (EntityUtility.TryGetBuffer<DungeonPathTemplateBuffer>(dungeon.Entity, API.Client.World, out var dungeonPathTemplateBuffer)) {
						foreach (var dungeonPathTemplate in dungeonPathTemplateBuffer) {
							var pathFlags = StructureUtility.SeparateFlags(dungeonPathTemplate.flags);
							var pathEntity = dungeonPathTemplate.spawnTemplateBufferEntity;

							if (!EntityUtility.TryGetBuffer<DungeonPathSpawnTemplateBuffer>(pathEntity, API.Client.World, out var dungeonPathSpawnTemplateBuffer))
								continue;

							if (!roomsThatSpawn.Any(room => pathFlags.Contains(room)))
								continue;

							foreach (var dungeonPathSpawnTemplate in dungeonPathSpawnTemplateBuffer)
								TryAddObjectsFromSpawnTemplate(ref dungeonPathSpawnTemplate.Value.Value);
						}
					}

					foreach (var objectData in objectsThatSpawn) {
						var replacedObjectData = TryReplaceResultingObject(objectData);
						var entry = new StructureContents {
							Result = (replacedObjectData.objectID, objectData.variation, 0),
							Dungeon = dungeon.Name
						};
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					}

					continue;

					void TryAddObjectsFromSpawnTemplate(ref SpawnTemplateBlob spawnTemplate) {
						for (var entryIdx = 0; entryIdx < spawnTemplate.entries.Length; entryIdx++) {
							ref var entry = ref spawnTemplate.entries[entryIdx];

							if (entry.disabled || entry.chanceToAppearAtAll <= 0f)
								continue;

							if (entry.canSpawnOn.Length > 0 && !entry.canSpawnOn.ToArray().Any(canSpawnOn => objectsThatCouldHaveSpawned.Contains(canSpawnOn)))
								continue;

							if (entry.canNotSpawnOn.Length > 0 && !entry.canNotSpawnOn.ToArray().All(canNotSpawnOn => objectsThatCouldHaveSpawned.Contains(canNotSpawnOn)))
								continue;

							if (entry.canSpawnNextTo.Length > 0 && !entry.canSpawnNextTo.ToArray().Any(canSpawnNextTo => objectsThatCouldHaveSpawned.Contains(canSpawnNextTo)))
								continue;

							for (var variationIdx = 0; variationIdx < entry.objectToSpawn.variations.Length; variationIdx++) {
								var variation = entry.objectToSpawn.variations[variationIdx];

								objectsThatCouldHaveSpawned.Add(entry.objectToSpawn.objectID);
								if (PugDatabase.TryGetComponent<TileCD>(entry.objectToSpawn.objectID, out var tileCD) && TileUtility.IsBlock(tileCD.tileType, (Tileset) tileCD.tileset, out var wallObjectId, out _)) {
									objectsThatSpawn.Add(new ObjectDataCD {
										objectID = wallObjectId,
										variation = variation
									});
								} else {
									objectsThatSpawn.Add(new ObjectDataCD {
										objectID = entry.objectToSpawn.objectID,
										variation = variation
									});
								}
							}
						}
					}
				}
			}
			
			private static ObjectDataCD TryReplaceResultingObject(ObjectDataCD objectData) {
				if (PugDatabase.TryGetComponent<PlantCD>(objectData, out var plantCD)) {
					return new ObjectDataCD {
						objectID = plantCD.objectToDropWhenHarvested,
						amount = objectData.amount
					};
				}

				return new ObjectDataCD {
					objectID = objectData.objectID,
					variation = ObjectUtility.GetPrimaryVariation(objectData),
					amount = objectData.amount
				};
			}
		}
	}
}