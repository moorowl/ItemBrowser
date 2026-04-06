using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using PugWorldGen;
using Unity.Collections;
using Unity.Entities;
using SceneReference = Pug.UnityExtensions.SceneReference;

namespace ItemBrowser.Utilities {
	public static class StructureUtility {
		private static readonly DataBlockAddress ClassicContentBundle = new("7507d88e-fd7a-7444-1b18-3816c6fbe382");
		private static readonly DataBlockAddress FullReleaseContentBundle = new("46418d34-550b-7504-7970-e202973b089b");
		
		public static HashSet<RandomDungeonRef> AllRandomDungeons = new();
		public static HashSet<UniqueDungeonRef> AllUniqueDungeons = new();
		public static HashSet<CustomSceneRef> AllCustomScenes = new();
		
		private static readonly Dictionary<string, CustomSceneRef> AllCustomScenesLookup = new();

		internal static void Bake() {
			AllRandomDungeons = GetAllRandomDungeons();
			AllUniqueDungeons = GetAllUniqueDungeons();
			AllCustomScenes.Clear();
			AllCustomScenesLookup.Clear();

			var allCustomScenesInRandomDungeonsLookup = new Dictionary<string, HashSet<RandomDungeonRef>>();
			var allCustomScenesInUniqueDungeonsLookup = new Dictionary<string, HashSet<UniqueDungeonRef>>();

			foreach (var dungeon in AllRandomDungeons) {
				foreach (var scene in GetAllScenesInDungeon(dungeon.Entity)) {
					if (!allCustomScenesInRandomDungeonsLookup.ContainsKey(scene))
						allCustomScenesInRandomDungeonsLookup[scene] = new HashSet<RandomDungeonRef>();

					allCustomScenesInRandomDungeonsLookup[scene].Add(dungeon);
				}
			}
			foreach (var dungeon in AllUniqueDungeons) {
				foreach (var scene in GetAllScenesInDungeon(dungeon.Entity)) {
					if (!allCustomScenesInUniqueDungeonsLookup.ContainsKey(scene))
						allCustomScenesInUniqueDungeonsLookup[scene] = new HashSet<UniqueDungeonRef>();

					allCustomScenesInUniqueDungeonsLookup[scene].Add(dungeon);
				}
			}
			
			ref var customScenesTable = ref ClientWorldStateSystem.CustomSceneTable.Value;
			for (var i = 0; i < customScenesTable.scenes.Length; i++) {
				ref var customScene = ref customScenesTable.scenes[i];
				var name = GetPersistentSceneName(customScene.sceneName.ToString());

				var canSpawnAtAll = false;
				var requiredBundlesPresent = new HashSet<DataBlockAddress>();
				var requiredBundlesAbsent = new HashSet<DataBlockAddress>();

				if (customScene.maxOccurrences > 0 && !customScene.replacedByContentBundle.hasValue) {
					var classicBiomes = customScene.biomesToSpawnIn.classic.ConvertToList();
					var fullReleaseBiomes = customScene.biomesToSpawnIn.fullRelease.ConvertToList();

					var availableInClassic = classicBiomes.Count == 0 || classicBiomes.Any(biome => CanBiomeGenerate(WorldGenerationType.Classic, biome));
					var availableInFullRelease = fullReleaseBiomes.Count == 0 || fullReleaseBiomes.Any(biome => CanBiomeGenerate(WorldGenerationType.FullRelease, biome));

					canSpawnAtAll = availableInClassic || availableInFullRelease;
					
					if (!availableInClassic && availableInFullRelease)
						requiredBundlesPresent.Add(FullReleaseContentBundle);
					else if (availableInClassic && !availableInFullRelease)
						requiredBundlesPresent.Add(ClassicContentBundle);
				} else {
					if (allCustomScenesInUniqueDungeonsLookup.TryGetValue(name, out var inUniqueDungeons)) {
						canSpawnAtAll = true;
						requiredBundlesPresent = inUniqueDungeons.ElementAt(0).RequiredBundlesPresent;
						requiredBundlesAbsent = inUniqueDungeons.ElementAt(0).RequiredBundlesAbsent;
					}
					
					if (allCustomScenesInRandomDungeonsLookup.TryGetValue(name, out var inRandomDungeons)) {
						canSpawnAtAll = true;
						requiredBundlesPresent = inRandomDungeons.ElementAt(0).RequiredBundlesPresent;
						requiredBundlesAbsent = inRandomDungeons.ElementAt(0).RequiredBundlesAbsent;
					}
				}
				
				if (!canSpawnAtAll)
					continue;

				var customSceneRef = new CustomSceneRef {
					Name = name,
					IndexInCustomSceneTable = i,
					RequiredBundlesPresent = requiredBundlesPresent,
					RequiredBundlesAbsent = requiredBundlesAbsent
				};
				AllCustomScenes.Add(customSceneRef);
				AllCustomScenesLookup[name] = customSceneRef;
			}

			for (var i = 0; i < customScenesTable.scenes.Length; i++) {
				ref var customScene = ref customScenesTable.scenes[i];
				var name = GetPersistentSceneName(customScene.sceneName.ToString());

				if (!AllCustomScenesLookup.ContainsKey(name))
					Main.Log(nameof(StructureUtility), $"Scene {name} doesn't spawn in any world");
			}
		}
		
		public static string GetPersistentSceneName(string sceneName) {
			// This is to turn SceneBuilder's runtime names (e.g. SB/318923147) into its identifier
			return new SceneReference {
				ScenePath = sceneName
			}.SceneName;
		}

		public static bool CanBiomeGenerate(WorldGenerationType worldGenerationType, Biome biome) {
			return worldGenerationType switch {
				WorldGenerationType.Classic => biome is Biome.None or Biome.Slime or Biome.Larva or Biome.Stone or Biome.Nature or Biome.Sea or Biome.Desert,
				WorldGenerationType.FullRelease => true,
				_ => false
			};
		}

		private static HashSet<string> GetAllScenesInDungeon(Entity dungeon) {
			var roomsThatSpawn = new HashSet<RoomFlags>();
			var scenes = new HashSet<string>();
					
			if (EntityUtility.TryGetBuffer<DungeonRoomPlacementBuffer>(dungeon, API.Client.World, out var dungeonRoomPlacementBuffer)) {
				foreach (var dungeonRoomPlacement in dungeonRoomPlacementBuffer) {
					var room = dungeonRoomPlacement.Value;
					if (room.amount.max <= 0)
						continue;

					roomsThatSpawn.UnionWith(SeparateFlags(room.roomType));
				}
			}

			if (EntityUtility.TryGetBuffer<DungeonCustomSceneGroupBuffer>(dungeon, API.Client.World, out var dungeonCustomSceneGroupBuffer)) {
				foreach (var dungeonCustomSceneGroup in dungeonCustomSceneGroupBuffer) {
					if (dungeonCustomSceneGroup.maxSpawns <= 0)
						continue;
					
					var spawnsInRooms = SeparateFlags(dungeonCustomSceneGroup.roomType);
					if (!roomsThatSpawn.Any(x => spawnsInRooms.Contains(x)))
						continue;

					foreach (var customScene in dungeonCustomSceneGroup.customScenes)
						scenes.Add(GetPersistentSceneName(customScene.name.ToString()));
				}
			}
			
			return scenes;
		}
		
		private static HashSet<RandomDungeonRef> GetAllRandomDungeons() {
			var results = new HashSet<RandomDungeonRef>();

			// Biome-specific dungeons
			var dungeonBiomeSpawnTableBuffer = API.Client.GetEntityQuery(typeof(DungeonBiomeSpawnTableBuffer)).GetSingletonBuffer<DungeonBiomeSpawnTableBuffer>(true);
			foreach (var biomeSpawnTable in dungeonBiomeSpawnTableBuffer) {
				var requiredBundlesPresent = new HashSet<DataBlockAddress>();

				if (!CanBiomeGenerate(WorldGenerationType.Classic, biomeSpawnTable.biome.classic) && CanBiomeGenerate(WorldGenerationType.FullRelease, biomeSpawnTable.biome.fullRelease))
					requiredBundlesPresent.Add(FullReleaseContentBundle);
				
				if (CanBiomeGenerate(WorldGenerationType.Classic, biomeSpawnTable.biome.classic) && !CanBiomeGenerate(WorldGenerationType.FullRelease, biomeSpawnTable.biome.fullRelease))
					requiredBundlesPresent.Add(ClassicContentBundle);
				
				foreach (var dungeon in EntityUtility.GetBuffer<DungeonSpawnTableBuffer>(biomeSpawnTable.tableEntity, API.Client.World)) {
					results.Add(new RandomDungeonRef {
						Entity = dungeon.prefabEntity,
						Name = dungeon.name.ToString(),
						SpawnEntry = dungeon,
						BiomeSpawnEntry = biomeSpawnTable,
						RequiredBundlesPresent = requiredBundlesPresent,
						RequiredBundlesAbsent = new HashSet<DataBlockAddress>()
					});
				}
			}
			
			return results;
		}
		
		private static HashSet<UniqueDungeonRef> GetAllUniqueDungeons() {
			var results = new HashSet<UniqueDungeonRef>();
			
			using var pugWorldGenCDs = API.Client.GetEntityQuery(typeof(PugWorldGenCD)).ToComponentDataArray<PugWorldGenCD>(Allocator.Temp);
			foreach (var pugWorldGenCD in pugWorldGenCDs) {
				var requiredBundlesPresent = new HashSet<DataBlockAddress> {
					pugWorldGenCD.contentBundle
				};
				var requiredBundlesAbsent = new HashSet<DataBlockAddress>();
				
				if (pugWorldGenCD.replacedByBundle.hasValue)
					requiredBundlesAbsent.Add(pugWorldGenCD.replacedByBundle.value);

				if (pugWorldGenCD.placementType is UniqueScenePlacementType.AnywhereInBiome or UniqueScenePlacementType.DistanceFromCoreInBiome) {
					if (!CanBiomeGenerate(WorldGenerationType.Classic, pugWorldGenCD.biome.classic) && CanBiomeGenerate(WorldGenerationType.FullRelease, pugWorldGenCD.biome.fullRelease))
						requiredBundlesPresent.Add(FullReleaseContentBundle);
				
					if (CanBiomeGenerate(WorldGenerationType.Classic, pugWorldGenCD.biome.classic) && !CanBiomeGenerate(WorldGenerationType.FullRelease, pugWorldGenCD.biome.fullRelease))
						requiredBundlesPresent.Add(ClassicContentBundle);
				}
				
				results.Add(new UniqueDungeonRef {
					Entity = pugWorldGenCD.entity,
					Name = pugWorldGenCD.name.ToString(),
					SpawnEntry = pugWorldGenCD,
					RequiredBundlesPresent = requiredBundlesPresent,
					RequiredBundlesAbsent = requiredBundlesAbsent
				});
			}

			return results;
		}
		
		public static HashSet<RoomFlags> SeparateFlags(RoomFlags flagsToSeparate) {
			var separatedFlags = new HashSet<RoomFlags>();
				
			foreach (RoomFlags flag in Enum.GetValues(typeof(RoomFlags))) {
				if (flagsToSeparate.HasFlag(flag))
					separatedFlags.Add(flag);
			}

			return separatedFlags;
		}

		public record UniqueDungeonRef {
			public Entity Entity;
			public string Name;
			public PugWorldGenCD SpawnEntry;
			public HashSet<DataBlockAddress> RequiredBundlesPresent;
			public HashSet<DataBlockAddress> RequiredBundlesAbsent;
		}
		
		public record RandomDungeonRef {
			public Entity Entity;
			public string Name;
			public DungeonSpawnTableBuffer SpawnEntry;
			public DungeonBiomeSpawnTableBuffer BiomeSpawnEntry;
			public HashSet<DataBlockAddress> RequiredBundlesPresent;
			public HashSet<DataBlockAddress> RequiredBundlesAbsent;
		}

		public record CustomSceneRef {
			public string Name;
			public int IndexInCustomSceneTable;
			public HashSet<DataBlockAddress> RequiredBundlesPresent;
			public HashSet<DataBlockAddress> RequiredBundlesAbsent;
		}
	}
}