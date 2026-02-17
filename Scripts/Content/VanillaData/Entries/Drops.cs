using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Api.Entries.Requirements.Types;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using Pug.Properties;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Drops : PrimaryLootTable.Entry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Drops", ObjectID.Slime, VanillaPriorities.Drops);

		public (ObjectID Id, int Variation) Entity { get; set; }
		public ValueBasedOnWorldState<(int Min, int Max)> Rolls { get; set; }
		public float MaxAmountCheckRadius { get; set; }
		public int MaxAmountAllowedWithinRadius { get; set; }
		public bool IsFromGuaranteedPool { get; set; }
		public bool IsFromLootTableWithGuaranteedPool { get; set; }
		public List<(string Name, int Amount)> FoundInScenes { get; set; } = new();

		public virtual bool Equals(Drops other) {
			if (other == null)
				return false;
			
			return Entity.Id == other.Entity.Id
			       && Entity.Variation == other.Entity.Variation
			       && Mathf.Approximately(Chance, other.Chance)
			       && Mathf.Approximately(ChanceForOne.Get(), other.ChanceForOne.Get())
			       && Amount.Get() == other.Amount.Get()
			       && Rolls.Get() == other.Rolls.Get()
			       && Mathf.Approximately(MaxAmountCheckRadius, other.MaxAmountCheckRadius)
			       && MaxAmountAllowedWithinRadius == other.MaxAmountAllowedWithinRadius
			       && OnlyDropsInBiome == other.OnlyDropsInBiome
			       && IsFromGuaranteedPool == other.IsFromGuaranteedPool
			       && IsFromLootTableWithGuaranteedPool == other.IsFromLootTableWithGuaranteedPool
			       && IsAffectedByPlayerCount == other.IsAffectedByPlayerCount
			       && IsAffectedByWorldMode == other.IsAffectedByWorldMode;
		}
		
		public override int GetHashCode() {
			var hashCode = new HashCode();
			hashCode.Add((int) Entity.Id);
			hashCode.Add(Entity.Variation);
			hashCode.Add(Chance);
			hashCode.Add(ChanceForOne.Get());
			hashCode.Add(Amount.Get());
			hashCode.Add(Rolls.Get());
			hashCode.Add(MaxAmountCheckRadius);
			hashCode.Add(MaxAmountAllowedWithinRadius);
			hashCode.Add((int) OnlyDropsInBiome);
			hashCode.Add(IsFromGuaranteedPool);
			hashCode.Add(IsFromLootTableWithGuaranteedPool);
			hashCode.Add(IsAffectedByPlayerCount);
			hashCode.Add(IsAffectedByWorldMode);
			return hashCode.ToHashCode();
		}

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var entriesToAdd = new List<(ObjectID Id, int Variation, Drops Entry)>();
				var pugDatabaseBankBlob = API.Client.GetEntityQuery(typeof(PugDatabase.DatabaseBankCD)).GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob;

				void AddNormalEntry(ObjectID id, int variation, Drops entry) {
					entriesToAdd.Add((id, variation, entry));
				}
				
				void AddEntryFromScene(ObjectID id, int variation, string sceneName, Drops entry) {
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
				
				void AddNormalOrSceneEntry(ObjectID id, int variation, string sceneName, Drops entry) {
					if (sceneName == null)
						AddNormalEntry(id, variation, entry);
					else
						AddEntryFromScene(id, variation, sceneName, entry);
				}

				void AddEntriesFromPrefab(World world, ObjectDataCD objectData, Entity entity, PrimaryLootTable lootTable, PrimaryLootTable.Pool genericPool, string optionalSceneName = null) {
					if (EntityUtility.TryGetComponentData<SnakeMovementStateCD>(entity, world, out var snakeMovementStateCD) && snakeMovementStateCD.tailObjectId == objectData.objectID)
						return;
					
					if (EntityUtility.HasComponentData<ProjectileCD>(entity, world) || EntityUtility.HasComponentData<MortarProjectileCD>(entity, world))
						return;

					// Spawns on death
					if (EntityUtility.TryGetComponentData<SpawnEntityOnDeathCD>(entity, world, out var spawnEntityOnDeathCD)) {
						var entry = new Drops {
							Result = (spawnEntityOnDeathCD.objectToSpawn, ObjectUtils.GetPrimaryVariation(spawnEntityOnDeathCD.objectToSpawn, spawnEntityOnDeathCD.objectVariation)),
							Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
							Chance = spawnEntityOnDeathCD.spawnChance,
							ChanceForOne = spawnEntityOnDeathCD.spawnChance,
							Amount = (1, 1),
							Rolls = (1, 1),
							MaxAmountCheckRadius = spawnEntityOnDeathCD.maxAmountCheckRadius,
							MaxAmountAllowedWithinRadius = spawnEntityOnDeathCD.maxAmountAllowedWithinRadius
						};
						AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						
						genericPool.AddEntry(entry);
					}
					
					// Boss chest
					if (EntityUtility.TryGetComponentData<BossCD>(entity, world, out var bossCD)) {
						var chestObjectData = UsesOptionalChest(objectData) ? bossCD.optionalChestVersion : bossCD.chestToSpawn;
						var entry = new Drops {
							Result = (chestObjectData.objectID, 0),
							Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
							Chance = 1f,
							ChanceForOne = 1f,
							Amount = (1, 1),
							Rolls = (1, 1)
						};
						AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
						
						genericPool.AddEntry(entry);
					}
					
					// Drops
					if (EntityUtility.TryGetComponentData<ChanceToDropLootCD>(entity, world, out var chanceToDropLootCD) && EntityUtility.TryGetBuffer<DropsLootBuffer>(entity, world, out var dropsLootBuffer)) {
						var groupedDrops = dropsLootBuffer.ConvertToList().GroupBy(entry => entry.lootDropID).Select(group => new DropsLootBuffer {
							lootDropID = group.First().lootDropID,
							amount = group.Sum(entry => entry.amount),
							multiplayerAmountAdditionScaling = group.First().multiplayerAmountAdditionScaling,
							requiredContentBundle = group.First().requiredContentBundle,
							skipIfScanned = group.First().skipIfScanned
						});
						
						foreach (var drop in groupedDrops) {
							if (drop.lootDropID == objectData.objectID && EntityUtility.HasComponentData<DontDropSelfCD>(entity, world) && !EntityUtility.HasComponentData<TileCD>(entity, world))
								continue;

							var entityObjectData = objectData;
							if (PugDatabase.HasComponent<PlantCD>(entityObjectData) && TryGetSeedFromPlant(allObjects, objectData, out var associatedSeed)) {
								if (entityObjectData.variation != 0)
									continue;
								
								entityObjectData = associatedSeed;
							}

							var entry = new Drops {
								Result = (drop.lootDropID, 0),
								Entity = (entityObjectData.objectID, ObjectUtils.GetPrimaryVariation(entityObjectData)),
								Chance = chanceToDropLootCD.chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => chanceToDropLootCD.chance
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(() => {
									var scaledAmount = LootUtils.GetMultiplayerScaledAmount(drop.amount, drop.multiplayerAmountAdditionScaling);
									return (scaledAmount, scaledAmount);
								}),
								Rolls = (1, 1),
								IsAffectedByPlayerCount = drop.multiplayerAmountAdditionScaling > 0
							};
							
							if (drop.requiredContentBundle.hasValue)
								entry.AddRequirement(new ContentBundlePresent(drop.requiredContentBundle.value));
							
							if (drop.skipIfScanned.hasValue)
								entry.AddRequirement(new HasNotBeenScanned(drop.skipIfScanned.value));
							
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							genericPool.AddEntry(entry);
						}
					}
					
					// Drops from table
					if (EntityUtility.TryGetComponentData<DropsLootFromLootTableCD>(entity, world, out var dropsLootFromLootTableCD)) {
						var isBoss = EntityUtility.HasComponentData<BossCD>(entity, world);
						var lootTableHelper = LootUtils.GetLootTableHelper(dropsLootFromLootTableCD.lootTableID);
						
						var lootTableGuaranteedPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader((1, 1)));
						var lootTableRandomPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader(
							new ValueBasedOnWorldState<(int, int)>(
								() => isBoss ? LootUtils.CalculateRollsForBosses(lootTableHelper.BaseRolls) : LootUtils.CalculateRolls(lootTableHelper.BaseRolls)
							)
						));

						foreach (var drop in lootTableHelper.GuaranteedPool) {
							var entry = new Drops {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => isBoss ? LootUtils.CalculateChanceForOneForBosses(drop.Chance, drop.BaseRolls) : LootUtils.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => isBoss ? LootUtils.CalculateRollsForBosses(drop.BaseRolls) : LootUtils.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							lootTableGuaranteedPool.AddEntry(entry);
						}
						
						foreach (var drop in lootTableHelper.RandomPool) {
							var entry = new Drops {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => isBoss ? LootUtils.CalculateChanceForOneForBosses(drop.Chance, drop.BaseRolls) : LootUtils.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => isBoss ? LootUtils.CalculateRollsForBosses(drop.BaseRolls) : LootUtils.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool,
								IsAffectedByPlayerCount = isBoss,
								IsAffectedByWorldMode = isBoss
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							lootTableRandomPool.AddEntry(entry);
						}
					}

					// Spawns on use (containers)
					if (EntityUtility.TryGetComponentData<SpawnsItemsOnUseCD>(entity, world, out var spawnsItemsOnUseCD) && EntityUtility.TryGetBuffer<OnUseLootBuffer>(entity, world, out var onUseLootBuffer)) {
						foreach (var onUseLoot in onUseLootBuffer) {
							var entry = new Drops {
								Result = (onUseLoot.lootDropID, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
								Chance = onUseLoot.chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => onUseLoot.chance
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => (onUseLoot.amount, onUseLoot.amount)
								),
								Rolls = (1, 1)
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							genericPool.AddEntry(entry);
						}
						
						var spawnsOnUseLootTableHelper = LootUtils.GetLootTableHelper(spawnsItemsOnUseCD.lootTable);
						
						var spawnsOnUseGuaranteedPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader((1, 1)));
						var spawnsOnUseRandomPool = lootTable.CreateAndAddPool(new PrimaryLootTable.RollsPoolHeader(
							new ValueBasedOnWorldState<(int, int)>(
								() => LootUtils.CalculateRolls(spawnsOnUseLootTableHelper.BaseRolls)
							)
						));
						
						foreach (var drop in spawnsOnUseLootTableHelper.GuaranteedPool) {
							var entry = new Drops {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => LootUtils.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => LootUtils.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);
							
							spawnsOnUseGuaranteedPool.AddEntry(entry);
						}
						
						foreach (var drop in spawnsOnUseLootTableHelper.RandomPool) {
							var entry = new Drops {
								Result = (drop.Item, 0),
								Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
								Chance = drop.Chance,
								ChanceForOne = new ValueBasedOnWorldState<float>(
									() => LootUtils.CalculateChanceForOne(drop.Chance, drop.BaseRolls)
								),
								Amount = new ValueBasedOnWorldState<(int, int)>(
									() => drop.Amount
								),
								Rolls = new ValueBasedOnWorldState<(int, int)>(
									() => LootUtils.CalculateRolls(drop.BaseRolls)
								),
								OnlyDropsInBiome = drop.OnlyDropsInBiome,
								IsFromLootTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
							};
							AddNormalOrSceneEntry(entry.Result.Id, entry.Result.Variation, optionalSceneName, entry);

							spawnsOnUseRandomPool.AddEntry(entry);
						}
					}
				}
				
				// Normal objects
				foreach (var (objectData, authoring) in allObjects) {
					var entity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, pugDatabaseBankBlob, objectData.variation);
					if (entity == Unity.Entities.Entity.Null || !ObjectUtils.IsPrimaryVariation(objectData))
						continue;

					if (ItemBrowserAPI.IsDeprecatedObject(objectData))
						continue;
					
					var primaryLootTable = new PrimaryLootTable();
					var genericPool = primaryLootTable.CreateAndAddPool();
					
					AddEntriesFromPrefab(API.Client.World, objectData, entity, primaryLootTable, genericPool);
					
					// Seasonal drops
					// Have to use DropLootAuthoring because drops from a season that isn't active aren't converted
					if (authoring.TryGetComponent<DropLootAuthoring>(out var dropLootAuthoring) && dropLootAuthoring.hasSeasonalLoot && IsSeasonalDropRequirementFulfilled(objectData)) {
						var seasonalLoot = dropLootAuthoring.seasonalLootDrops;
						
						foreach (var group in seasonalLoot.lootDrops) {
							var seasonalLootPool = group.season == Season.None ? genericPool : primaryLootTable.CreateAndAddPool(new PrimaryLootTable.SeasonPoolHeader(group.season));

							foreach (var drop in group.lootDrops) {
								var entry = new Drops {
									Result = (drop.lootDropID, 0),
									Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
									Chance = drop.chance,
									ChanceForOne = new ValueBasedOnWorldState<float>(() => drop.chance),
									Amount = new ValueBasedOnWorldState<(int, int)>(() => {
										var scaledAmount = LootUtils.GetMultiplayerScaledAmount(drop.amount, drop.multiplayerAmountAdditionScaling);
										return (scaledAmount, scaledAmount);
									}),
									Rolls = new ValueBasedOnWorldState<(int, int)>(() => (1, 1)),
									IsAffectedByPlayerCount = drop.multiplayerAmountAdditionScaling > 0
								};
								entry.AddRequirement(new SeasonActive(group.season));
								
								AddNormalEntry(entry.Result.Id, entry.Result.Variation, entry);
								
								seasonalLootPool.AddEntry(entry);
							}
						}
					}
					
					if (primaryLootTable.Pools.Any(pool => pool.Entries.Any()))
						registry.Register(ObjectEntryType.Usage, objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData), primaryLootTable);
				}
				
				// Scene objects
				ref var customSceneTable = ref ClientWorldStateSystem.CustomSceneTable.Value;
				foreach (var scene in StructureUtils.AllCustomScenes) {
					ref var sceneBlob = ref customSceneTable.scenes[scene.IndexInCustomSceneTable];

					for (var i = 0; i < sceneBlob.prefabInventoryOverrides.Length; i++) {
						ref var prefabObjectData = ref sceneBlob.prefabObjectDatas[i];
						ref var prefab = ref sceneBlob.prefabs[i];

						if (prefab != null && EntityUtility.HasComponentData<CustomScenePrefab>(prefab, API.Client.World)) {
							var primaryLootTable = new PrimaryLootTable();
							var genericPool = primaryLootTable.CreateAndAddPool();
							
							AddEntriesFromPrefab(API.Client.World, prefabObjectData, prefab, new PrimaryLootTable(), genericPool, scene.Name);
						}
					}
				}

				foreach (var entry in entriesToAdd) {
					if (entry.Id == ObjectID.None)
						continue;
					
					registry.Register(ObjectEntryType.Source, entry.Id, entry.Variation, entry.Entry);
				}
			}

			private static bool TryGetSeedFromPlant(List<(ObjectData ObjectData, GameObject Authoring)> allObjects, ObjectDataCD plant, out ObjectDataCD seed) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var objectPropertiesCD) || !objectPropertiesCD.Has(PropertyID.isSeed))
						continue;

					var turnsIntoPlant = objectPropertiesCD.Get<ObjectID>(PropertyID.Seed.turnsIntoPlantID);
					if (turnsIntoPlant != plant.objectID)
						continue;
					
					seed = objectData;
					return true;
				}

				seed = default;
				return false;
			}
			
			private static bool IsSeasonalDropRequirementFulfilled(ObjectData objectData) {
				// this is hardcoded in BirdBossSystem sadly
				if (objectData.objectID == ObjectID.BirdBoss)
					return objectData.variation == 1;

				return true;
			}

			private static bool UsesOptionalChest(ObjectData objectData) {
				// this is hardcoded in BirdBossSystem sadly
				return objectData is { objectID: ObjectID.BirdBoss, variation: 1 };
			}
		}
	}
}