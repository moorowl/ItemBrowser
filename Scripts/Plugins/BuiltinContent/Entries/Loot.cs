using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities.Extensions;
using ItemBrowser.Utilities;
using PugMod;
using PugWorldGen;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record Loot : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Loot", ObjectID.CopperChest, Priorities.Loot);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public (ObjectID Id, int Variation) Entity { get; set; }
		public float Chance { get; set; }
		public Func<float> ChanceForOne { get; set; }
		public Func<(int Min, int Max)> Amount { get; set; }
		public Func<(int Min, int Max)> Rolls { get; set; }
		public Biome OnlyDropsInBiome { get; set; }
		public bool IsFromGuaranteedPool { get; set; }
		public bool IsFromTableWithGuaranteedPool { get; set; }
		public List<(string Name, int Amount)> FoundInScenes { get; set; } = new();
		public List<(string Name, int Amount)> FoundInDungeons { get; set; } = new();
		
		public virtual bool Equals(Loot other) {
			if (other == null)
				return false;
			
			return Entity.Id == other.Entity.Id
			       && Entity.Variation == other.Entity.Variation
			       && Mathf.Approximately(Chance, other.Chance)
			       && Mathf.Approximately(ChanceForOne(), other.ChanceForOne())
			       && Amount() == other.Amount()
			       && Rolls() == other.Rolls()
			       && OnlyDropsInBiome == other.OnlyDropsInBiome
			       && IsFromGuaranteedPool == other.IsFromGuaranteedPool
			       && IsFromTableWithGuaranteedPool == other.IsFromTableWithGuaranteedPool;
		}
		
		public override int GetHashCode() {
			var hashCode = new HashCode();
			hashCode.Add((int) Entity.Id);
			hashCode.Add(Entity.Variation);
			hashCode.Add(Chance);
			hashCode.Add(ChanceForOne());
			hashCode.Add(Amount());
			hashCode.Add(Rolls());
			hashCode.Add((int) OnlyDropsInBiome);
			hashCode.Add(IsFromGuaranteedPool);
			hashCode.Add(IsFromTableWithGuaranteedPool);
			return hashCode.ToHashCode();
		}
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var entriesToAdd = new List<(ObjectID Id, int Variation, Loot Entry)>();
				var pugDatabaseBankBlob = API.Client.GetEntityQuery(typeof(PugDatabase.DatabaseBankCD)).GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob;

				void AddNormalEntry(ObjectID id, int variation, Loot entry) {
					entriesToAdd.Add((id, variation, entry));
				}
				
				void AddEntryFromScene(ObjectID id, int variation, string sceneName, Loot entry) {
					foreach (var existingEntry in entriesToAdd) {
						if (existingEntry.Id == id && existingEntry.Variation == variation && existingEntry.Entry.Equals(entry)) {
							if (existingEntry.Entry.FoundInScenes.Count > 0) {
								var existingScene = existingEntry.Entry.FoundInScenes.FirstOrDefault(x => x.Name == sceneName);
								if (existingScene.Name != null)
									existingScene.Amount += 1;
								else
									existingEntry.Entry.FoundInScenes.Add((sceneName, 1));	
							}
							
							return;
						}
					}
					
					entry.FoundInScenes.Add((sceneName, 1));
					AddNormalEntry(id, variation, entry);
				}
				
				void AddEntryFromDungeon(ObjectID id, int variation, string dungeonName, Loot entry) {
					foreach (var existingEntry in entriesToAdd) {
						if (existingEntry.Id == id && existingEntry.Variation == variation && existingEntry.Entry.Equals(entry) && existingEntry.Entry.FoundInDungeons.Count > 0) {
							var existingDungeon = existingEntry.Entry.FoundInDungeons.FirstOrDefault(x => x.Name == dungeonName);
							if (existingDungeon.Name != null)
								existingDungeon.Amount += 1;
							else
								existingEntry.Entry.FoundInDungeons.Add((dungeonName, 1));
							
							return;
						}
					}
					
					entry.FoundInDungeons.Add((dungeonName, 1));
					AddNormalEntry(id, variation, entry);
				}
				
				void AddNormalOrSceneEntry(ObjectID id, int variation, string sceneName, Loot entry) {
					if (sceneName == null)
						AddNormalEntry(id, variation, entry);
					else
						AddEntryFromScene(id, variation, sceneName, entry);
				}

				void AddEntriesFromPrefab(World world, ObjectDataCD objectData, Entity entity, string optionalSceneName = null) {
					if (EntityUtility.HasComponentData<InventoryBuffer>(entity, world) && EntityUtility.TryGetBuffer<ContainedObjectsBuffer>(entity, world, out var containedObjects)) {
						var groupedContainedObjects = containedObjects.ConvertToList()
							.GroupBy(entry => entry.objectID)
							.Select(group => {
								var entry = group.First();
								return new ObjectDataCD {
									objectID = entry.objectID,
									variation = entry.variation,
									amount = group.Sum(item => item.amount)
								};
							});

						foreach (var containedObject in groupedContainedObjects) {
							var entry = new Loot {
								Result = (containedObject.objectID, ObjectUtils.GetPrimaryVariation(containedObject.objectID, containedObject.variation)),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Amount = () => (containedObject.amount, containedObject.amount),
								Chance = 1f,
								ChanceForOne = () => 1f,
								Rolls = () => (1, 1)
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}	
					}

					if (EntityUtility.TryGetComponentData<AddRandomLootCD>(entity, world, out var addRandomLootCD)) {
						foreach (var drop in LootUtils.GetLootTableContents(addRandomLootCD.lootTableID)) {
							var entry = new Loot {
								Result = (drop.ObjectId, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Chance = drop.Chance,
								ChanceForOne = () => drop.CalculateChanceForOne(),
								Amount = () => drop.ObjectAmount,
								Rolls = () => drop.CalculateRolls(),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}
					}

					if (EntityUtility.TryGetComponentData<ChangeVariationWhenContainingObjectCD>(entity, world, out var changeVariationWhenContainingObjectCD)) {
						foreach (var drop in LootUtils.GetLootTableContents(changeVariationWhenContainingObjectCD.addLootFromTableToNewObject)) {
							var entry = new Loot {
								Result = (drop.ObjectId, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
								Chance = drop.Chance,
								ChanceForOne = () => drop.CalculateChanceForOne(),
								Amount = () => drop.ObjectAmount,
								Rolls = () => drop.CalculateRolls(),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						}
						
						if (EntityUtility.TryGetBuffer<ItemsToAddToNewObjectBuffer>(entity, world, out var itemsToAddToNewObject)) {
							var groupedItemsToAddToNewObject = itemsToAddToNewObject.ConvertToList()
								.GroupBy(entry => entry.objectData.objectID)
								.Select(group => {
									var entry = group.First();
									return new ObjectDataCD {
										objectID = entry.objectData.objectID,
										variation = entry.objectData.variation,
										amount = group.Sum(item => item.objectData.amount)
									};
								});

							foreach (var item in groupedItemsToAddToNewObject) {
								var entry = new Loot {
									Result = (item.objectID, ObjectUtils.GetPrimaryVariation(item.objectID, item.variation)),
									Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation)),
									Amount = () => (item.amount, item.amount),
									Chance = 1f,
									ChanceForOne = () => 1f,
									Rolls = () => (1, 1)
								};
								AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							}
						}
					}
				}
				
				// Normal objects
				foreach (var (objectData, _) in allObjects) {
					var entity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, pugDatabaseBankBlob, objectData.variation);
					if (entity == Unity.Entities.Entity.Null)
						continue;
					
					AddEntriesFromPrefab(API.Client.World, objectData, entity);
				}
				
				// Scene objects
				ref var customScenesBlobArray = ref API.Client.GetEntityQuery(typeof(CustomSceneTableCD)).GetSingleton<CustomSceneTableCD>().Value.Value.scenes;
				for (var sceneIdx = 0; sceneIdx < customScenesBlobArray.Length; sceneIdx++) {
					ref var customSceneBlob = ref customScenesBlobArray[sceneIdx];
					var name = customSceneBlob.sceneName.ToString();
					
					if (!StructureUtils.CanSceneGenerateInAnyWorld(name))
						continue;

					for (var i = 0; i < customSceneBlob.prefabInventoryOverrides.Length; i++) {
						ref var prefabInventoryOverride = ref customSceneBlob.prefabInventoryOverrides[i];
						ref var prefabObjectData = ref customSceneBlob.prefabObjectDatas[i];
						ref var prefab = ref customSceneBlob.prefabs[i];
						
						if (prefabInventoryOverride.hasLootTableOverride) {
							var lootTableOverride = prefabInventoryOverride.lootTableOverride;
							
							foreach (var drop in LootUtils.GetLootTableContents(lootTableOverride)) {
								var entry = new Loot {
									Result = (drop.ObjectId, 0),
									Entity = (prefabObjectData.objectID, ObjectUtils.GetPrimaryVariation(prefabObjectData.objectID, prefabObjectData.variation)),
									Chance = drop.Chance,
									ChanceForOne = () => drop.CalculateChanceForOne(),
									Amount = () => drop.ObjectAmount,
									Rolls = () => drop.CalculateRolls(),
									OnlyDropsInBiome = drop.OnlyDropsInBiome,
									IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
									IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
								};
								AddEntryFromScene(entry.Result.Id, entry.Result.Variation, name, entry);
							}
						}

						if (prefabInventoryOverride.hasItemsOverride) {
							var groupedItemsOverride = prefabInventoryOverride.itemsOverride.ConvertToList()
								.GroupBy(entry => entry.objectID)
								.Select(group => {
									var entry = group.First();
									return new ObjectDataCD {
										objectID = entry.objectID,
										variation = entry.variation,
										amount = group.Sum(item => PugDatabase.GetObjectInfo(entry.objectID) is { isStackable: true } ? Math.Max(item.amount, 1) : 1)
									};
								});
							
							foreach (var item in groupedItemsOverride) {
								var entry = new Loot {
									Result = (item.objectID, ObjectUtils.GetPrimaryVariation(item.objectID, item.variation)),
									Entity = (prefabObjectData.objectID, ObjectUtils.GetPrimaryVariation(prefabObjectData.objectID, prefabObjectData.variation)),
									Amount = () => (item.amount, item.amount),
									Chance = 1f,
									ChanceForOne = () => 1f,
									Rolls = () => (1, 1)
								};
								AddEntryFromScene(entry.Result.Id, entry.Result.Variation, name, entry);
							}
						}
						
						if (prefab != null && EntityUtility.HasComponentData<CustomScenePrefab>(prefab, API.Client.World))
							AddEntriesFromPrefab(API.Client.World, prefabObjectData, prefab, name);
					}
				}
				
				// Dungeon objects
				// Dungeons
				foreach (var dungeon in StructureUtils.GetAllDungeons()) {
					var roomsThatSpawn = new HashSet<RoomFlags>();

					if (EntityUtility.TryGetBuffer<DungeonRoomPlacementBuffer>(dungeon.Entity, API.Client.World, out var dungeonRoomPlacementBuffer)) {
						foreach (var dungeonRoomPlacement in dungeonRoomPlacementBuffer) {
							var room = dungeonRoomPlacement.Value;
							if (room.amount.max <= 0)
								continue;

							roomsThatSpawn.UnionWith(StructureUtils.SeparateFlags(room.roomType));
						}
					}

					if (EntityUtility.TryGetBuffer<DungeonNodeTemplateBuffer>(dungeon.Entity, API.Client.World, out var dungeonNodeTemplateBuffer)) {
						foreach (var dungeonNodeTemplate in dungeonNodeTemplateBuffer) {
							var nodeFlags = StructureUtils.SeparateFlags(dungeonNodeTemplate.flags);
							var nodeEntity = dungeonNodeTemplate.spawnTemplateBufferEntity;

							if (!EntityUtility.TryGetBuffer<DungeonNodeSpawnTemplateBuffer>(nodeEntity, API.Client.World, out var dungeonNodeSpawnTemplateBuffer))
								continue;

							if (!roomsThatSpawn.Any(room => nodeFlags.Contains(room)))
								continue;

							foreach (var dungeonNodeSpawnTemplate in dungeonNodeSpawnTemplateBuffer) {
								ref var spawnTemplate = ref dungeonNodeSpawnTemplate.Value.Value;

								for (var entryIdx = 0; entryIdx < spawnTemplate.entries.Length; entryIdx++) {
									ref var spawnEntry = ref spawnTemplate.entries[entryIdx];
									if (spawnEntry.containLoot == LootTableID.Empty)
										continue;
									
									foreach (var drop in LootUtils.GetLootTableContents(spawnEntry.containLoot)) {
										var entry = new Loot {
											Result = (drop.ObjectId, 0),
											Entity = (spawnEntry.objectToSpawn.objectID, 0),
											Chance = drop.Chance,
											ChanceForOne = () => drop.CalculateChanceForOne(),
											Amount = () => drop.ObjectAmount,
											Rolls = () => drop.CalculateRolls(),
											OnlyDropsInBiome = drop.OnlyDropsInBiome,
											IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
											IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
										};
										AddEntryFromDungeon(entry.Result.Id, entry.Result.Variation, dungeon.Name, entry);
									}
								}
							}
						}
					}
				}

				foreach (var entry in entriesToAdd) {
					registry.Register(ObjectEntryType.Source, entry.Id, entry.Variation, entry.Entry);
					// registry.Register(ObjectEntryType.Usage, entry.Entry.Entity.Id, entry.Entry.Entity.Variation, entry.Entry);
				}
			}
		}
	}
}