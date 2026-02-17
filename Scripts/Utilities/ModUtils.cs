using System.Collections.Generic;
using System.Linq;
using ModIO;
using PugMod;

namespace ItemBrowser.Utilities {
	public static class ModUtils {
		private static readonly Dictionary<long, string> DisplayNames = new();
		private static readonly Dictionary<long, HashSet<ObjectDataCD>> AssociatedObjects = new();
		private static readonly Dictionary<ObjectDataCD, long> AssociatedMod = new();

		private const long UnknownModId = -1;
		private const string UnknownModName = "(Unknown Mod)";
		private const long CoreKeeperModId = 0;
		private const string CoreKeeperModName = "Core Keeper";
		
		internal static void InitOnModLoad() {
			SetupDisplayNames();
			SetupAssociatedObjects();
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
		
		private static void SetupDisplayNames() {
			DisplayNames.Clear();
			
			foreach (var mod in API.ModLoader.LoadedMods)
				DisplayNames[mod.ModId] = mod.Metadata.name;
			
			// Override from mod.io
			var subscribedMods = ModIOUnity.GetSubscribedMods(out var result);
			if (result.Succeeded()) {
				foreach (var subscribedMod in subscribedMods) {
					var profile = subscribedMod.modProfile;
					DisplayNames[profile.id.id] = profile.name;
				}	
			}
			
			/* Override from steam workshop
			var resultPageTask = Query.All.WhereUserSubscribed(SteamClient.SteamId).GetPageAsync(1);
			resultPageTask.Wait();

			if (resultPageTask.Result.HasValue) {
				foreach (var entry in resultPageTask.Result.Value.Entries) {
					if (entry.IsBanned || !entry.IsInstalled)
						continue;

					DisplayNames.TryAdd((long) entry.Id.Value, entry.Title);
				}
			}*/
		}

		private static void SetupAssociatedObjects() {
			AssociatedObjects.Clear();
			AssociatedMod.Clear();

			foreach (var authoring in Manager.mod.ExtraAuthoring) {
				var gameObject = authoring.gameObject;

				var associatedModId = UnknownModId;
				var objectData = default(ObjectDataCD);
				
				if (gameObject.TryGetComponent<ObjectAuthoring>(out var objectAuthoring)) {
					var internalName = objectAuthoring.objectName;
					if (internalName.Contains(":")) {
						var sourceMod = ProcessModInternalName(internalName.Split(':')[0]);
						associatedModId = API.ModLoader.LoadedMods.FirstOrDefault(mod => ProcessModInternalName(mod.Metadata.name) == sourceMod)?.ModId ?? UnknownModId;
						objectData = new ObjectDataCD {
							objectID = API.Authoring.GetObjectID(internalName),
							variation = objectAuthoring.variation
						};
					}
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

			return;

			string ProcessModInternalName(string name) {
				return name.ToLowerInvariant().Replace("-", "").Replace("_", "").Replace(" ", "");
			}
		}
	}
}