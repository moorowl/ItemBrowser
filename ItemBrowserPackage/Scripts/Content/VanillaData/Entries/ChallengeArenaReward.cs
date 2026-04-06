using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record ChallengeArenaReward : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/ChallengeArenaReward", ObjectID.AlienChest, VanillaPriorities.ChallengeArenaReward);
		
		public ObjectID Result { get; set; }
		public float Chance { get; set; }
		public float ChanceForOne { get; set; }
		public (int Min, int Max) Amount { get; set; }
		public (int Min, int Max) Rolls { get; set; }
		public Biome OnlyDropsInBiome { get; set; }
		public bool IsFromGuaranteedPool { get; set; }
		public bool IsFromTableWithGuaranteedPool { get; set; }
		public float? ChanceWhenBraveMerchantAlive { get; set; }

		public class Provider : ObjectEntryProvider {
			private static readonly (ObjectID Id, Biome Biome)[] GemstoneTypes = {
				(ObjectID.NatureGemstone, Biome.Nature),
				(ObjectID.SeaGemstone, Biome.Sea),
				(ObjectID.DesertGemstone, Biome.Desert)
			};
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// All of these are hardcoded in EventTerminalSystem
				registry.Register(ObjectEntryType.Source, ObjectID.AlienChest, 0, new ChallengeArenaReward {
					Result = ObjectID.AlienChest,
					Amount = (1, 1),
					Chance = 1f,
					ChanceForOne = 1f,
					Rolls = (1, 1)
				});
				registry.Register(ObjectEntryType.Source, ObjectID.CrystalMerchantSpawnItem, 0, new ChallengeArenaReward {
					Result = ObjectID.CrystalMerchantSpawnItem,
					Amount = (1, 1),
					Chance = 1f,
					ChanceForOne = 1f,
					Rolls = (1, 1)
				});

				foreach (var gem in GemstoneTypes) {
					registry.Register(ObjectEntryType.Source, gem.Id, 0, new ChallengeArenaReward {
						Result = gem.Id,
						Amount = (2, 3),
						Chance = 1f,
						ChanceForOne = 1f,
						Rolls = (1, 1),
						OnlyDropsInBiome = gem.Biome
					});
				}

				var eventTerminalLootTableHelper = LootUtility.GetLootTableHelper(LootTableID.AlienEventTerminal);
				
				foreach (var drop in eventTerminalLootTableHelper.GuaranteedPool) {
					registry.Register(ObjectEntryType.Source, drop.Item, 0, new ChallengeArenaReward {
						Result = drop.Item,
						Chance = drop.Chance,
						ChanceForOne = LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls),
						Amount = drop.Amount,
						Rolls = LootUtility.CalculateRolls(drop.BaseRolls),
						OnlyDropsInBiome = drop.OnlyDropsInBiome,
						IsFromGuaranteedPool = true,
						IsFromTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
					});
				}
				
				foreach (var drop in eventTerminalLootTableHelper.RandomPool) {
					registry.Register(ObjectEntryType.Source, drop.Item, 0, new ChallengeArenaReward {
						Result = drop.Item,
						Chance = drop.Chance,
						ChanceForOne = LootUtility.CalculateChanceForOne(drop.Chance, drop.BaseRolls),
						Amount = drop.Amount,
						Rolls = LootUtility.CalculateRolls(drop.BaseRolls),
						OnlyDropsInBiome = drop.OnlyDropsInBiome,
						IsFromTableWithGuaranteedPool = drop.IsFromLootTableWithGuaranteedPool
					});
				}
			}
		}
	}
}