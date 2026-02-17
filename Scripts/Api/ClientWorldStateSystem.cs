using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace ItemBrowser.Api {
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
	public partial struct ClientWorldStateSystem : ISystem {
		public static WorldInfoCD WorldInfo;
		public static WorldGenerationType WorldGenerationType;
		public static BlobAssetReference<CustomSceneTableBlob> CustomSceneTable;
		public static NativeHashSet<DataBlockAddress> ActivatedContentBundles;
		public static BlobAssetReference<LootTableBankBlob> LootTableBank;
		public static int PlayerCount;
		public static bool HasRunAtLeastOnce;
		
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
		}

		public void OnUpdate(ref SystemState state) {
			var networkTime = SystemAPI.GetSingleton<NetworkTime>();

			if (!HasRunAtLeastOnce || ShouldUpdate(ref state, networkTime, 23, 0.2f)) {
				HasRunAtLeastOnce = true;

				// Update WorldInfo
				WorldInfo = SystemAPI.GetSingleton<WorldInfoCD>();
				
				// Update WorldGenerationType
				WorldGenerationType = SystemAPI.GetSingleton<WorldGenerationTypeCD>().Value;
				
				// Update CustomSceneTableCD
				CustomSceneTable = SystemAPI.GetSingleton<CustomSceneTableCD>().Value;
				
				// Update ActivatedContentBundles
				ActivatedContentBundles.Clear();
				var activatedContentBundlesBuffer = SystemAPI.GetSingletonBuffer<ActivatedContentBundlesBuffer>();
				foreach (var activatedContentBundles in activatedContentBundlesBuffer)
					ActivatedContentBundles.Add(activatedContentBundles.ContentBundle);

				LootTableBank = SystemAPI.GetSingleton<LootTableBankCD>().Value;

				// Update PlayerCount
				PlayerCount = _playerCountQuery.CalculateEntityCount();
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
		
		private static bool ShouldUpdate(ref SystemState state, NetworkTime networkTime, int offset, float minimumUpdateRate) {
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