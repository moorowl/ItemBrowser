using System;
using System.Collections.Generic;
using System.Linq;
using ModIO;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class ModUtility {
		private static readonly Dictionary<long, string> DisplayNames = new();
		private static readonly Dictionary<long, HashSet<ObjectDataCD>> AssociatedObjects = new();
		private static readonly Dictionary<ObjectDataCD, long> AssociatedMod = new();

		private static readonly Lazy<Dictionary<int, long>> AssetToModMap = new(() => {
			var map = new Dictionary<int, long>();

			foreach (var mod in API.ModLoader.LoadedMods) {
				foreach (var asset in mod.Assets)
					map.TryAdd(asset.GetInstanceID(), mod.ModId);
			}

			return map;
		});

		public const long UnknownModId = -1;
		private const string UnknownModName = "(Unknown Mod)";
		public const long CoreKeeperModId = 0;
		private const string CoreKeeperModName = "Core Keeper";
		
		internal static void Bake() {
			BakeDisplayNames();
			BakeAssociatedObjects();
		}
		
		private static void BakeDisplayNames() {
			DisplayNames.Clear();
			
			foreach (var mod in API.ModLoader.LoadedMods)
				DisplayNames[mod.ModId] = !string.IsNullOrWhiteSpace(mod.Metadata.displayName) ? mod.Metadata.displayName : mod.Metadata.name;
			
			// Override from mod.io
			var subscribedMods = ModIOUnity.GetSubscribedMods(out var result);
			if (result.Succeeded()) {
				foreach (var subscribedMod in subscribedMods) {
					var profile = subscribedMod.modProfile;
					DisplayNames[profile.id.id] = profile.name;
				}	
			}
		}

		private static void BakeAssociatedObjects() {
			AssociatedObjects.Clear();
			AssociatedMod.Clear();

			foreach (var authoring in Manager.mod.ExtraAuthoring) {
				var gameObject = authoring.gameObject;

				var associatedModId = UnknownModId;
				var objectData = default(ObjectDataCD);

				if (gameObject.TryGetComponent<ObjectAuthoring>(out var objectAuthoring)) {
					associatedModId = GetSourceModOrDefault(objectAuthoring, associatedModId);
					objectData = new ObjectDataCD {
						objectID = API.Authoring.GetObjectID(objectAuthoring.objectName),
						variation = objectAuthoring.variation
					};
				} else if (gameObject.TryGetComponent<EntityMonoBehaviourData>(out var entityMonoBehaviourData)) {
					objectData = new ObjectDataCD {
						objectID = entityMonoBehaviourData.objectInfo.objectID,
						variation = entityMonoBehaviourData.objectInfo.variation
					};
				}
				if (objectData.objectID == ObjectID.None)
					continue;
				
				if (!AssociatedObjects.ContainsKey(associatedModId))
					AssociatedObjects[associatedModId] = new HashSet<ObjectDataCD>();
				
				AssociatedObjects[associatedModId].Add(objectData);
				AssociatedMod.TryAdd(objectData, associatedModId);
			}
		}

		private static long GetSourceModOrDefault(ObjectAuthoring objectAuthoring, long defaultModId) {
			// Try to get mod from prefab -> mod id map first
			// This will work for most objects, but won't detect those instantiated at runtime (e.g. corelib workbenches)
			if (AssetToModMap.Value.TryGetValue(objectAuthoring.gameObject.GetInstanceID(), out var modId))
				return modId;
			
			var internalName = objectAuthoring.objectName;
			if (internalName.Contains(":") && internalName.Length >= 3) {
				var sourceMod = NormalizeModInternalName(internalName.Split(':')[0]);
				var sourceLoadedMod = API.ModLoader.LoadedMods.FirstOrDefault(mod => NormalizeModInternalName(mod.Metadata.name) == sourceMod);

				if (sourceLoadedMod != null)
					return sourceLoadedMod.ModId;
			}

			return defaultModId;
			
			string NormalizeModInternalName(string name) {
				return name.ToLowerInvariant().Replace("-", "").Replace("_", "").Replace(" ", "");
			}
		}

		public static bool IsLoaded(long mod) {
			return API.ModLoader.LoadedMods.Any(loadedMod => loadedMod.ModId == mod);
		}
		
		public static bool IsLoaded(string mod) {
			return API.ModLoader.LoadedMods.Any(loadedMod => loadedMod.Metadata.name == mod);
		}
		
		public static string GetDisplayName(long mod) {
			return mod switch {
				CoreKeeperModId => CoreKeeperModName,
				UnknownModId => UnknownModName,
				_ => DisplayNames.GetValueOrDefault(mod, mod.ToString())
			};
		}
		
		public static HashSet<ObjectDataCD> GetAssociatedObjects(long mod) {
			return AssociatedObjects.TryGetValue(mod, out var value) ? value : new HashSet<ObjectDataCD>();
		}
		
		public static long GetAssociatedMod(ObjectDataCD objectData) {
			return AssociatedMod.GetValueOrDefault(objectData, CoreKeeperModId);
		}
		
		public static long GetAssociatedMod(ObjectID id, int variation = 0) {
			return AssociatedMod.GetValueOrDefault(new ObjectDataCD { objectID = id, variation = variation }, CoreKeeperModId);
		}

		public static bool IsModded(ObjectDataCD objectData) {
			return GetAssociatedMod(objectData) != CoreKeeperModId;
		}
		
		public static bool IsModded(ObjectID id, int variation = 0) {
			return IsModded(new ObjectDataCD { objectID = id, variation = variation });
		}
	}
}