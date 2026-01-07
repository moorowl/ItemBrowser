using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using PugWorldGen;
using Unity.Collections;
using Unity.Entities;
using SceneReference = Pug.UnityExtensions.SceneReference;

namespace ItemBrowser.Utilities {
	public static class StructureUtils {
		private static readonly HashSet<string> ScenesThatSpawnInAnyWorld = new();
		private static readonly HashSet<string> ScenesThatSpawnInCurrentWorld = new();
		private static readonly HashSet<string> DungeonsThatSpawnInAnyWorld = new();
		private static readonly HashSet<string> DungeonsThatSpawnInCurrentWorld = new();

		private static readonly HashSet<Biome> BiomesAvailableInClassicWorlds = new() {
			Biome.None,
			Biome.Slime,
			Biome.Larva,
			Biome.Stone,
			Biome.Nature,
			Biome.Sea,
			Biome.Desert
		};
		
		internal static void InitOnWorldLoad() {
			ScenesThatSpawnInAnyWorld.Clear();
			ScenesThatSpawnInCurrentWorld.Clear();
			DungeonsThatSpawnInAnyWorld.Clear();
			DungeonsThatSpawnInCurrentWorld.Clear();

			var currentWorldGenType = API.Client.GetEntityQuery(typeof(WorldGenerationTypeCD)).GetSingleton<WorldGenerationTypeCD>().Value;

			ref var customScenesTable = ref API.Client.GetEntityQuery(typeof(CustomSceneTableCD)).GetSingleton<CustomSceneTableCD>().Value.Value;
			for (var i = 0; i < customScenesTable.scenes.Length; i++) {
				ref var customScene = ref customScenesTable.scenes[i];
				var name = GetPersistentSceneName(customScene.sceneName.ToString());

				if (customScene.maxOccurrences == 0 || customScene.replacedByContentBundle.hasValue)
					continue;

				var classicBiomes = customScene.biomesToSpawnIn.classic.ConvertToList();
				var fullReleaseBiomes = customScene.biomesToSpawnIn.fullRelease.ConvertToList();

				if (classicBiomes.Count == 0 || classicBiomes.Any(biome => CanBiomeGenerate(WorldGenerationType.Classic, biome))) {
					ScenesThatSpawnInAnyWorld.Add(name);
					
					if (currentWorldGenType == WorldGenerationType.Classic)
						ScenesThatSpawnInCurrentWorld.Add(name);
				}
				
				if (fullReleaseBiomes.Count == 0 || fullReleaseBiomes.Any(biome => CanBiomeGenerate(WorldGenerationType.FullRelease, biome))) {
					ScenesThatSpawnInAnyWorld.Add(name);
					
					if (currentWorldGenType == WorldGenerationType.FullRelease)
						ScenesThatSpawnInCurrentWorld.Add(name);
				}
			}

			foreach (var dungeon in GetAllRandomDungeons()) {
				foreach (var scene in GetAllScenesInDungeon(dungeon.Entity)) {
					ScenesThatSpawnInAnyWorld.Add(scene);

					if (CanBiomeGenerate(currentWorldGenType, dungeon.BiomeSpawnEntry.biome.Get(currentWorldGenType)))
						ScenesThatSpawnInCurrentWorld.Add(scene);
				}
			}
			
			foreach (var dungeon in GetAllUniqueDungeons()) {
				foreach (var scene in GetAllScenesInDungeon(dungeon.Entity)) {
					ScenesThatSpawnInAnyWorld.Add(scene);
				}
			}
			
			for (var i = 0; i < customScenesTable.scenes.Length; i++) {
				ref var customScene = ref customScenesTable.scenes[i];
				var name = GetPersistentSceneName(customScene.sceneName.ToString());

				if (!ScenesThatSpawnInAnyWorld.Contains(name))
					Main.Log(nameof(StructureUtils), $"Scene {name} doesn't spawn in any world");
			}
		}
		
		public static string GetPersistentSceneName(string sceneName) {
			// This is to turn SceneBuilder's runtime names (e.g. SB/318923147) into its identifier
			return new SceneReference {
				ScenePath = sceneName
			}.SceneName;
		}

		public static bool CanSceneGenerateInAnyWorld(string sceneName) {
			return ScenesThatSpawnInAnyWorld.Contains(sceneName);
		}

		public static bool CanContentBundleBeActive(WorldGenerationType worldGenerationType, ContentBundleID contentBundle) {
			return worldGenerationType switch {
				WorldGenerationType.Classic => contentBundle == ContentBundleID.Classic,
				WorldGenerationType.FullRelease => contentBundle != ContentBundleID.Classic,
				_ => false
			};
		}
		
		public static bool CanBiomeGenerate(WorldGenerationType worldGenerationType, Biome biome) {
			return worldGenerationType switch {
				WorldGenerationType.Classic => BiomesAvailableInClassicWorlds.Contains(biome),
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
		
		public static HashSet<(Entity Entity, string Name, DungeonSpawnTableBuffer SpawnEntry, DungeonBiomeSpawnTableBuffer BiomeSpawnEntry)> GetAllRandomDungeons() {
			var dungeons = new HashSet<(Entity Entity, string Name, DungeonSpawnTableBuffer SpawnEntry, DungeonBiomeSpawnTableBuffer BiomeSpawnEntry)>();

			// Biome-specific dungeons
			var dungeonBiomeSpawnTableBuffer = API.Client.GetEntityQuery(typeof(DungeonBiomeSpawnTableBuffer)).GetSingletonBuffer<DungeonBiomeSpawnTableBuffer>(true);
			foreach (var biomeSpawnTable in dungeonBiomeSpawnTableBuffer) {
				foreach (var dungeon in EntityUtility.GetBuffer<DungeonSpawnTableBuffer>(biomeSpawnTable.tableEntity, API.Client.World))
					dungeons.Add((dungeon.prefabEntity, dungeon.name.ToString(), dungeon, biomeSpawnTable));
			}
			
			return dungeons;
		}
		
		public static HashSet<(Entity Entity, string Name, PugWorldGenCD SpawnEntry)> GetAllUniqueDungeons() {
			var dungeons = new HashSet<(Entity Entity, string Name, PugWorldGenCD SpawnEntry)>();
			
			using var pugWorldGenCDs = API.Client.GetEntityQuery(typeof(PugWorldGenCD)).ToComponentDataArray<PugWorldGenCD>(Allocator.Temp);
			foreach (var pugWorldGenCD in pugWorldGenCDs)
				dungeons.Add((pugWorldGenCD.entity, pugWorldGenCD.name.ToString(), pugWorldGenCD));

			return dungeons;
		}
		
		public static HashSet<(Entity Entity, string Name)> GetAllDungeons() {
			var dungeons = new HashSet<(Entity Entity, string Name)>();

			foreach (var dungeon in GetAllRandomDungeons())
				dungeons.Add((dungeon.Entity, dungeon.Name));
			
			foreach (var dungeon in GetAllUniqueDungeons())
				dungeons.Add((dungeon.Entity, dungeon.Name));

			return dungeons;
		}
		
		public static HashSet<RoomFlags> SeparateFlags(RoomFlags flagsToSeparate) {
			var separatedFlags = new HashSet<RoomFlags>();
				
			foreach (RoomFlags flag in Enum.GetValues(typeof(RoomFlags))) {
				if (flagsToSeparate.HasFlag(flag))
					separatedFlags.Add(flag);
			}

			return separatedFlags;
		}
	}
}