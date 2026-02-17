using System;
using ItemBrowser.Api;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class LootUtils {
		public readonly struct LootTableHelper {
			public readonly Drop[] GuaranteedPool;
			public readonly Drop[] RandomPool;
			public readonly (int Min, int Max) BaseRolls;
			
			public LootTableHelper(ref EntityLootTable entityLootTable) {
				ref var guaranteedLootInfos = ref entityLootTable.guaranteedDropsLootTable;
				ref var normalLootInfos = ref entityLootTable.lootTable;
				
				var minRolls = entityLootTable.minUniqueDrops;
				var maxRolls = entityLootTable.maxUniqueDrops;
				if (guaranteedLootInfos.Length > 0) {
					minRolls--;
					maxRolls--;
				}

				GuaranteedPool = new Drop[guaranteedLootInfos.Length];
				RandomPool = new Drop[normalLootInfos.Length];
				BaseRolls = (minRolls, maxRolls);

				var guaranteedTotalWeight = 0f;
				var normalTotalWeight = 0f;
				for (var i = 0; i < guaranteedLootInfos.Length; i++) {
					var entry = guaranteedLootInfos[i];
					guaranteedTotalWeight += entry.weight;
				}
				for (var i = 0; i < normalLootInfos.Length; i++) {
					var entry = normalLootInfos[i];
					normalTotalWeight += entry.weight;
				}

				for (var i = 0; i < guaranteedLootInfos.Length; i++) {
					var entry = guaranteedLootInfos[i];
					GuaranteedPool[i] = new Drop(
						this,
						entry.objectID,
						(entry.amount.min, entry.amount.max),
						(1, 1),
						entry.weight / guaranteedTotalWeight,
						entry.onlyDropsInBiome,
						true
					);
				}
				
				for (var i = 0; i < normalLootInfos.Length; i++) {
					var entry = normalLootInfos[i];
					RandomPool[i] = new Drop(
						this,
						entry.objectID,
						(entry.amount.min, entry.amount.max),
						(minRolls, maxRolls),
						entry.weight / normalTotalWeight,
						entry.onlyDropsInBiome,
						false
					);
				}
			}
			
			public readonly struct Drop {
				public readonly LootTableHelper BelongsToLootTable;
				public readonly ObjectID Item;
				public readonly (int Min, int Max) Amount;
				public readonly (int Min, int Max) BaseRolls;
				public readonly float Chance;
				public readonly Biome OnlyDropsInBiome;
				public readonly bool IsFromGuaranteedPool;

				public bool IsFromLootTableWithGuaranteedPool => BelongsToLootTable.GuaranteedPool.Length > 0;

				public Drop(LootTableHelper lootTable, ObjectID item, (int, int) amount, (int, int) baseRolls, float chance, Biome onlyDropsInBiome, bool isFromGuaranteedPool) {
					BelongsToLootTable = lootTable;
					Item = item;
					Amount = amount;
					BaseRolls = baseRolls;
					Chance = chance;
					OnlyDropsInBiome = onlyDropsInBiome;
					IsFromGuaranteedPool = isFromGuaranteedPool;
				}
			}
		}

		public static LootTableHelper GetLootTableHelper(LootTableID id) {
			ref var entityLootTable = ref ClientWorldStateSystem.TryGetEntityLootTable(id, out var exists);
			return exists ? new LootTableHelper(ref entityLootTable) : default;
		}
		
		public static float GetChanceForActiveWorld(EnvironmentalSpawnChance chance) {
			var worldGenSettings = Manager.saves.GetWorldInfo().worldGenerationSettings;
			
			return chance.source switch {
				EnvironmentalSpawnChance.Source.Constant => chance.constantValue.GetValueForCurrentPlatform(),
				EnvironmentalSpawnChance.Source.WorldGenSetting => worldGenSettings.Count == 0
					? chance.worldGenDependentValue.GetValue(WorldGenerationSettingLevel.Normal)
					: chance.worldGenDependentValue.GetValue(worldGenSettings[(int) chance.worldGenDependentValue.worldGenSetting].level),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		
		public static float CalculateChanceForOneForBosses(float baseChance, (int Min, int Max) baseRolls, int? playerCountOverride = null) {
			var (minRolls, maxRolls) = CalculateRollsForBosses(baseRolls, playerCountOverride);
			var averageRolls = (minRolls + maxRolls) / 2f;

			return 1f - Mathf.Pow(1f - baseChance, averageRolls);
		}
			
		public static float CalculateChanceForOne(float baseChance, (int Min, int Max) baseRolls) {
			return CalculateChanceForOneForBosses(baseChance, baseRolls, 1);
		}

		public static (int Min, int Max) CalculateRollsForBosses((int Min, int Max) baseRolls, int? playerCountOverride = null) {
			if (baseRolls.Min == 1 && baseRolls.Max == 1)
				return baseRolls;
				
			var playerCount = Math.Max(playerCountOverride ?? ClientWorldStateSystem.PlayerCount, 1);

			var rollsMultiplier = 1f + Math.Max(0, playerCount - 1) * 0.5f;
			if (ClientWorldStateSystem.WorldInfo.IsWorldModeEnabled(WorldMode.Hard))
				rollsMultiplier *= Constants.hardModeBossLootAmountMultiplier;
				
			return (
				(int) Math.Round(baseRolls.Min * rollsMultiplier),
				(int) Math.Round(baseRolls.Max * rollsMultiplier)
			);
		}
			
		public static (int Min, int Max) CalculateRolls((int Min, int Max) baseRolls) {
			return CalculateRollsForBosses(baseRolls, 1);
		}
		
		public static int GetMultiplayerScaledAmount(int baseAmount, float multiplayerAmountAdditionScaling, int? playerCountOverride = null) {
			var playerCount = playerCountOverride ?? ClientWorldStateSystem.PlayerCount;
			return (int) math.round(baseAmount * (1f + math.max(0, playerCount - 1) * multiplayerAmountAdditionScaling));
		}
	}
}