using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace ItemBrowser.Common.Api {
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
	public partial struct ClientWorldStateSystem : ISystem {
		public static WorldInfoCD WorldInfo;
		public static WorldGenerationType WorldGenerationType;
		public static BlobAssetReference<CustomSceneTableBlob> CustomSceneTable;
		public static NativeHashSet<DataBlockAddress> ActivatedContentBundles;
		public static BlobAssetReference<LootTableBankBlob> LootTableBank;
		public static BlobAssetReference<PugDatabase.PugDatabaseBank> PugDatabaseBank;
		public static List<Entity> NearbyChests;
		public static bool LarvaBossStatueIsActivated;
		public static bool HiveBossStatueIsActivated;
		public static int PlayerCount;
		public static bool HasRunAtLeastOnce;

		public static bool IsAdminOrInCreative => Manager.saves.IsCreativeModeCharacter() || Manager.main.player.adminPrivileges >= 1;
		
		private EntityQuery _playerCountQuery;
		
		public void OnCreate(ref SystemState state) {
			ActivatedContentBundles = new NativeHashSet<DataBlockAddress>(16, Allocator.Persistent);
			HasRunAtLeastOnce = false;

			_playerCountQuery = state.GetEntityQuery(typeof(PlayerGhost));
			
			state.RequireForUpdate<NetworkTime>();
			state.RequireForUpdate<WorldGenerationTypeCD>();
			state.RequireForUpdate<CustomSceneTableCD>();
			state.RequireForUpdate<WorldInfoCD>();
			state.RequireForUpdate<ActivatedContentBundlesBuffer>();
			state.RequireForUpdate<LootTableBankCD>();
			state.RequireForUpdate<PugDatabase.DatabaseBankCD>();
		}

		public void OnUpdate(ref SystemState state) {
			var networkTime = SystemAPI.GetSingleton<NetworkTime>();

			if (!HasRunAtLeastOnce || ShouldUpdate(ref state, networkTime, 23, 0.2f)) {
				HasRunAtLeastOnce = true;

				WorldInfo = SystemAPI.GetSingleton<WorldInfoCD>();
				WorldGenerationType = SystemAPI.GetSingleton<WorldGenerationTypeCD>().Value;
				CustomSceneTable = SystemAPI.GetSingleton<CustomSceneTableCD>().Value;
				LootTableBank = SystemAPI.GetSingleton<LootTableBankCD>().Value;
				PugDatabaseBank = SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob;
				PlayerCount = _playerCountQuery.CalculateEntityCount();
				NearbyChests = Manager.main.player?.playerCraftingHandler?.GetNearbyChests() ?? new List<Entity>();

				// we can't just use the fields in WorldInfoCD because they aren't synced to clients
				LarvaBossStatueIsActivated = false;
				HiveBossStatueIsActivated = false;
				foreach (var bossStatue in SystemAPI.Query<RefRO<BossStatueCD>>()) {
					switch (bossStatue.ValueRO.acceptsCrystalID) {
						case ObjectID.LarvaBossCrystal:
							LarvaBossStatueIsActivated = true;
							break;
						case ObjectID.HiveBossCrystal:
							HiveBossStatueIsActivated = true;
							break;
					}
				}
				
				ActivatedContentBundles.Clear();
				var activatedContentBundlesBuffer = SystemAPI.GetSingletonBuffer<ActivatedContentBundlesBuffer>();
				foreach (var activatedContentBundles in activatedContentBundlesBuffer)
					ActivatedContentBundles.Add(activatedContentBundles.ContentBundle);
			}
		}

		public void OnDestroy(ref SystemState state) {
			WorldInfo = default;
			WorldGenerationType = default;
			CustomSceneTable = default;
			ActivatedContentBundles.Dispose();
			PlayerCount = default;
			HasRunAtLeastOnce = default;
		}
		
		public static bool ShouldUpdate(ref SystemState state, NetworkTime networkTime, int offset, float minimumUpdateRate) {
			var num = math.max(1, (int) math.round(20f / minimumUpdateRate));
			return !networkTime.ServerTick.IsValid || networkTime.ServerTick.TickIndexForValidTick % num == offset % num;
		}
		
		private static EntityLootTable _defaultLootTable;
		
		public static ref EntityLootTable TryGetEntityLootTable(LootTableID id, out bool exists) {
			ref var lootTables = ref LootTableBank.Value.lootTables;

			for (var i = 0; i < lootTables.Length; i++) {
				if (lootTables[i].lootTableID != id)
					continue;
				
				exists = true;
				return ref lootTables[i];
			}

			exists = false;
			return ref _defaultLootTable;
		}
	}
}