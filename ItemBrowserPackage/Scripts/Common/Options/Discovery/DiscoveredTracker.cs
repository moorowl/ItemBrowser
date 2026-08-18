using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.Options.DiscoveredObjects {
	public static class DiscoveredTracker {
		public static bool HasBeenDiscovered<T>(T key) {
			return key switch {
				ObjectDataCD objectData => objectData.objectID == ObjectID.None || OptionsManager.Instance.HasTag(new ObjectDataCD { objectID = objectData.objectID, variation = ObjectUtility.GetPrimaryVariation(objectData) }, ObjectTagType.Discovered),
				Biome biome => biome == Biome.Slime || Manager.saves.HasDiscoveredBiome(biome), // only biomes with a region title can be discovered
				_ => true
			};
		}
		
		public static bool HasBeenDiscoveredInDiscoveryMode<T>(T key, out float temporaryTimeRemaining) {
			temporaryTimeRemaining = 0f;

			if (!OptionsManager.Instance.DiscoveryMode)
				return true;

			var isDiscovered = HasBeenDiscovered(key);
			if (!isDiscovered && TemporarilyDiscoveredState<T>.TryGet(key, out var temporaryDiscoveredUntil)) {
				temporaryTimeRemaining = Mathf.Max(temporaryDiscoveredUntil - Time.time, 0f);
				return temporaryTimeRemaining > 0f;
			}

			return isDiscovered;
		}
		
		public static void SetTemporarilyDiscovered<T>(T key, float? duration = null) {
			TemporarilyDiscoveredState<T>.Set(key, duration);
		}

		public static void ClearTemporarilyDiscovered<T>(T key) {
			TemporarilyDiscoveredState<T>.Clear(key);
		}

		private static class TemporarilyDiscoveredState<T> {
			private static readonly Dictionary<T, float> Values = new();

			public static void Set(T key, float? duration = null) {
				Values[key] = duration == null ? float.MaxValue : Time.time + duration.Value;
			}
			
			public static bool TryGet(T key, out float temporaryTimeRemaining) {
				return Values.TryGetValue(key, out temporaryTimeRemaining);
			}

			public static void Clear(T key) {
				Values.Remove(key);
			}
		}
	}
}