using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Content.VanillaData.Entries.Requirements;
using ItemBrowser.Utilities.Extensions;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using PugWorldGen;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Loot : PrimaryLootTable.Entry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Loot", ObjectID.CopperChest, VanillaPriorities.Loot);

		public (ObjectID Id, int Variation) Entity { get; set; }
		public ValueBasedOnWorldState<(int Min, int Max)> Rolls { get; set; }
		public bool IsFromGuaranteedPool { get; set; }
		public bool IsFromLootTableWithGuaranteedPool { get; set; }
		public List<(string Name, int Amount)> FoundInScenes { get; set; } = new();
		public List<(string Name, int Amount)> FoundInDungeons { get; set; } = new();
		
		public virtual bool Equals(Loot other) {
			if (other == null)
				return false;

			if (Requirements.Any() || other.Requirements.Any())
				return false;
			
			return Entity.Id == other.Entity.Id
			       && Entity.Variation == other.Entity.Variation
			       && Mathf.Approximately(Chance, other.Chance)
			       && Mathf.Approximately(ChanceForOne.Get(), other.ChanceForOne.Get())
			       && Amount.Get() == other.Amount.Get()
			       && Rolls.Get() == other.Rolls.Get()
			       && OnlyDropsInBiome == other.OnlyDropsInBiome
			       && IsFromGuaranteedPool == other.IsFromGuaranteedPool
			       && IsFromLootTableWithGuaranteedPool == other.IsFromLootTableWithGuaranteedPool;
		}
		
		public override int GetHashCode() {
			var hashCode = new HashCode();
			hashCode.Add((int) Entity.Id);
			hashCode.Add(Entity.Variation);
			hashCode.Add(Chance);
			hashCode.Add(ChanceForOne.Get());
			hashCode.Add(Amount.Get());
			hashCode.Add(Rolls.Get());
			hashCode.Add((int) OnlyDropsInBiome);
			hashCode.Add(IsFromGuaranteedPool);
			hashCode.Add(IsFromLootTableWithGuaranteedPool);
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

				void AddEntriesFromPrefab(World world, ObjectDataCD objectData, Entity entity, PrimaryLootTable lootTable, PrimaryLootTable.Pool genericPool, string optionalSceneName = null) {
					if (EntityUtility.HasComponentData<InventoryBuffer>(entity, world) && EntityUtility.TryGetBuffer<ContainedObjectsBuffer>(entity, world, out var containedObjects)) {
						var groupedContainedObjects = containedObjects.ConvertToList()
							.Where(entry => entry.objectID != ObjectID.None)
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
								Result = (containedObject.objectID, ObjectUtility.GetPrimaryVariation(containedObject)),
								Entity = (objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData)),
								Amount = (containedObject.amount, containedObject.amount),
								Chance = 1f,
								ChanceForOne = 1f,
								Rolls = (1, 1)
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							genericPool.AddEntry(entry);
						}	
					}

					if (EntityUtility.TryGetComponentData<AddRandomLootCD>(entity, world, out var addRandomLootCD)) {
						var addRandomLootTableHelper = LootUtility.GetLootTableHelper(addRandomLootCD.lootTableID);
						
						var addRandomLootTableGuaranteedPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader((1, 1)));
						var addRandomLootTableRandomPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader(
							new ValueBasedOnWorldState<(int, int)>(
								() => LootUtility.CalculateRolls(addRandomLootTableHelper.BaseRolls)
							)
						));
						
						foreach (var drop in addRandomLootTableHelper.GuaranteedPool) {
							var entry = new Loot {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => LootUtility.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = true,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							addRandomLootTableGuaranteedPool.AddEntry(entry);
						}
						
						foreach (var drop in addRandomLootTableHelper.RandomPool) {
							var entry = new Loot {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => LootUtility.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							addRandomLootTableRandomPool.AddEntry(entry);
						}
					}

					if (EntityUtility.TryGetComponentData<ChangeVariationWhenContainingObjectCD>(entity, world, out var changeVariationWhenContainingObjectCD)) {
						var changeVariationWhenContainingObjectLootTableHelper = LootUtility.GetLootTableHelper(changeVariationWhenContainingObjectCD.addLootFromTableToNewObject);
						
						var changeVariationWhenContainingObjectLootTableGuaranteedPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader((1, 1)));
						var changeVariationWhenContainingObjectLootTableRandomPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader(
							new ValueBasedOnWorldState<(int, int)>(
								() => LootUtility.CalculateRolls(changeVariationWhenContainingObjectLootTableHelper.BaseRolls)
							)
						));
						
						foreach (var drop in changeVariationWhenContainingObjectLootTableHelper.GuaranteedPool) {
							var entry = new Loot {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => LootUtility.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = true,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							changeVariationWhenContainingObjectLootTableGuaranteedPool.AddEntry(entry);
						}
						
						foreach (var drop in changeVariationWhenContainingObjectLootTableHelper.RandomPool) {
							var entry = new Loot {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => LootUtility.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							changeVariationWhenContainingObjectLootTableRandomPool.AddEntry(entry);
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
									Result = (item.objectID, ObjectUtility.GetPrimaryVariation(item.objectID, item.variation)),
									Entity = (objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData)),
									Amount = (item.amount, item.amount),
									Chance = 1f,
									ChanceForOne = 1f,
									Rolls = (1, 1)
								};
								AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
								
								genericPool.AddEntry(entry);
							}
						}
					}
				}
				
				// Normal objects
				foreach (var (objectData, _) in allObjects) {
					var entity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, pugDatabaseBankBlob, objectData.variation);
					if (entity == Unity.Entities.Entity.Null)
						continue;
					
					var primaryLootTable = new PrimaryLootTable();
					var genericPool = primaryLootTable.CreateAndAddPool();
					
					AddEntriesFromPrefab(API.Client.World, objectData, entity, primaryLootTable, genericPool);
					
					if (primaryLootTable.Pools.Any(pool => pool.Entries.Any()))
						registry.Register(ObjectEntryType.Usage, objectData.objectID, ObjectUtility.GetPrimaryVariation(objectData), primaryLootTable);
				}
				
				// Scene objects
				ref var customSceneTable = ref ClientWorldStateSystem.CustomSceneTable.Value;
				foreach (var scene in StructureUtility.AllCustomScenes) {
					ref var sceneBlob = ref customSceneTable.scenes[scene.IndexInCustomSceneTable];

					for (var i = 0; i < sceneBlob.prefabInventoryOverrides.Length; i++) {
						ref var prefabInventoryOverride = ref sceneBlob.prefabInventoryOverrides[i];
						ref var prefabObjectData = ref sceneBlob.prefabObjectDatas[i];
						ref var prefab = ref sceneBlob.prefabs[i];
						
						if (prefabInventoryOverride.hasLootTableOverride) {
							var overrideLootTableHelper = LootUtility.GetLootTableHelper(prefabInventoryOverride.lootTableOverride);

							foreach (var drop in overrideLootTableHelper.GuaranteedPool) {
								var entry = new Loot {
									Result = (drop.Item, 0),
									Entity = (prefabObjectData.objectID, ObjectUtility.GetPrimaryVariation(prefabObjectData)),
									Chance = drop.Chance,
									ChanceForOne = new ValueBasedOnWorldState<float>(
										() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
									),
									Amount = new ValueBasedOnWorldState<(int, int)>(
										() => drop.Amount
									),
									Rolls = new ValueBasedOnWorldState<(int, int)>(
										() => LootUtility.CalculateRolls(drop.BaseRolls)
									),
									OnlyDropsInBiome = drop.OnlyDropsInBiome,
									IsFromGuaranteedPool = true,
									IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
								};
								AddEntryFromScene(entry.Result.Id, entry.Result.Variation, scene.Name, entry);
							}
							
							foreach (var drop in overrideLootTableHelper.RandomPool) {
								var entry = new Loot {
									Result = (drop.Item, 0),
									Entity = (prefabObjectData.objectID, ObjectUtility.GetPrimaryVariation(prefabObjectData)),
									Chance = drop.Chance,
									ChanceForOne = new ValueBasedOnWorldState<float>(
										() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
									),
									Amount = new ValueBasedOnWorldState<(int, int)>(
										() => drop.Amount
									),
									Rolls = new ValueBasedOnWorldState<(int, int)>(
										() => LootUtility.CalculateRolls(drop.BaseRolls)
									),
									OnlyDropsInBiome = drop.OnlyDropsInBiome,
									IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
								};
								AddEntryFromScene(entry.Result.Id, entry.Result.Variation, scene.Name, entry);
							}
						}

						if (prefabInventoryOverride.hasItemsOverride) {
							var groupedInitialInventoryItems = prefabInventoryOverride.itemsOverride.ConvertToList()
								.GroupBy(entry => entry.item.objectID)
								.Select(group => {
									var entry = group.First();
									return new InitialInventoryItem {
										item = new ObjectData {
											objectID = entry.item.objectID,
											variation = entry.item.variation,
											amount = group.Sum(inventoryItem => PugDatabase.GetObjectInfo(entry.item.objectID) is { isStackable: true } ? Math.Max(inventoryItem.item.amount, 1) : 1)
										},
										requiredContentBundle = entry.requiredContentBundle
									};
								});
							
							foreach (var initialInventoryItem in groupedInitialInventoryItems) {
								var entry = new Loot {
									Result = (initialInventoryItem.item.objectID, ObjectUtility.GetPrimaryVariation(initialInventoryItem.item.objectID, initialInventoryItem.item.variation)),
									Entity = (prefabObjectData.objectID, ObjectUtility.GetPrimaryVariation(prefabObjectData)),
									Amount = (initialInventoryItem.item.amount, initialInventoryItem.item.amount),
									Chance = 1f,
									ChanceForOne = 1f,
									Rolls = (1, 1)
								};
								
								if (initialInventoryItem.requiredContentBundle.hasValue)
									entry.AddRequirement(new ContentBundlePresentRequirement(initialInventoryItem.requiredContentBundle.value));
								
								AddEntryFromScene(entry.Result.Id, entry.Result.Variation, scene.Name, entry);
							}
						}

						var primaryLootTable = new PrimaryLootTable();
						var genericPool = primaryLootTable.CreateAndAddPool();
						
						if (prefab != null && EntityUtility.HasComponentData<CustomScenePrefab>(prefab, API.Client.World))
							AddEntriesFromPrefab(API.Client.World, prefabObjectData, prefab, primaryLootTable, genericPool, scene.Name);
					}
				}
				
				// Dungeon objects
				// Dungeons
				var allDungeons = StructureUtility.AllRandomDungeons.Select(x => (x.Name, x.Entity))
					.Union(StructureUtility.AllUniqueDungeons.Select(x => (x.Name, x.Entity)))
					.ToList();
				
				foreach (var dungeon in allDungeons) {
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

							foreach (var dungeonNodeSpawnTemplate in dungeonNodeSpawnTemplateBuffer) {
								ref var spawnTemplate = ref dungeonNodeSpawnTemplate.Value.Value;

								for (var entryIdx = 0; entryIdx < spawnTemplate.entries.Length; entryIdx++) {
									ref var spawnEntry = ref spawnTemplate.entries[entryIdx];
									if (spawnEntry.containLoot == LootTableID.Empty)
										continue;

									var dungeonLootTableHelper = LootUtility.GetLootTableHelper(spawnEntry.containLoot);
									
									foreach (var drop in dungeonLootTableHelper.GuaranteedPool) {
										var entry = new Loot {
											Result = (drop.Item, 0),
											Entity = (spawnEntry.objectToSpawn.objectID, 0),
											Chance = drop.Chance,
											ChanceForOne = new ValueBasedOnWorldState<float>(
												() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
											),
											Amount = new ValueBasedOnWorldState<(int, int)>(
												() => drop.Amount
											),
											Rolls = new ValueBasedOnWorldState<(int, int)>(
												() => LootUtility.CalculateRolls(drop.BaseRolls)
											),
											OnlyDropsInBiome = drop.OnlyDropsInBiome,
											IsFromGuaranteedPool = true,
											IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
										};
										AddEntryFromDungeon(entry.Result.Id, entry.Result.Variation, dungeon.Name, entry);
									}
									
									foreach (var drop in dungeonLootTableHelper.RandomPool) {
										var entry = new Loot {
											Result = (drop.Item, 0),
											Entity = (spawnEntry.objectToSpawn.objectID, 0),
											Chance = drop.Chance,
											ChanceForOne = new ValueBasedOnWorldState<float>(
												() => LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
											),
											Amount = new ValueBasedOnWorldState<(int, int)>(
												() => drop.Amount
											),
											Rolls = new ValueBasedOnWorldState<(int, int)>(
												() => LootUtility.CalculateRolls(drop.BaseRolls)
											),
											OnlyDropsInBiome = drop.OnlyDropsInBiome,
											IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
										};
										AddEntryFromDungeon(entry.Result.Id, entry.Result.Variation, dungeon.Name, entry);
									}
								}
							}
						}
					}
				}

				foreach (var entry in entriesToAdd) {
					if (entry.Id == ObjectID.None)
						continue;
					
					registry.Register(ObjectEntryType.Source, entry.Id, entry.Variation, entry.Entry);
				}
			}
		}
	}
}