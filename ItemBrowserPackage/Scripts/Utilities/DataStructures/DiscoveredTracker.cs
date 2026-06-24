using System.Collections.Generic;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using UnityEngine;

namespace ItemBrowser.Utilities.DataStructures {
	public static class DiscoveredTracker<T> {
		private static readonly Dictionary<T, float> TemporaryDiscoveredUntil = new();

		public static bool HasBeenDiscovered(T key, out float temporaryTimeRemaining) {
			temporaryTimeRemaining = 0f;

			if (!OptionsManager.Instance.DiscoveryMode)
				return true;

			var isDiscovered = key switch {
				ObjectDataCD objectData => objectData.objectID == ObjectID.None || Manager.saves.HasDiscoveredObject(objectData.objectID, ItemBrowserAPI.IsCreatureIndexed(objectData) ? 0 : objectData.variation),
				Biome biome => biome == Biome.Slime || Manager.saves.HasDiscoveredBiome(biome), // only biomes with a region title can be discovered
				_ => true
			};
			
			if (!isDiscovered && TemporaryDiscoveredUntil.TryGetValue(key, out var temporaryDiscoveredUntil)) {
				temporaryTimeRemaining = Mathf.Max(temporaryDiscoveredUntil - Time.time, 0f);
				return temporaryTimeRemaining > 0f;
			}

			return isDiscovered;
		}

		public static void SetTemporarilyDiscovered(T key, float? duration = null) {
			TemporaryDiscoveredUntil[key] = duration == null ? float.MaxValue : Time.time + duration.Value;
		}

		public static void ClearTemporarilyDiscovered(T key) {
			TemporaryDiscoveredUntil.Remove(key);
		}
	}
}