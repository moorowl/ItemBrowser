using System.Collections.Generic;
using HarmonyLib;
using ItemBrowser.Common.Api;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using Unity.Entities;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.Options.Discovery {
	[HarmonyPatch]
	public static class DiscoverTilesAndObjects {
		private static bool _hasInit;
		private static bool _shouldDiscoverTilesAndObjects;

		static DiscoverTilesAndObjects() {
			ItemBrowserAPI.OnBrowserInit += () => {
				API.Client.OnObjectSpawnedOnClient += DiscoverNearbyObjects;

				AddAlreadyDiscoveredObjects();
			};
			ItemBrowserAPI.OnBrowserUninit += () => {
				API.Client.OnObjectSpawnedOnClient -= DiscoverNearbyObjects;
			};

			ItemBrowserAPI.OnBrowserUpdate += () => {
				_shouldDiscoverTilesAndObjects = OptionsManager.Instance.DiscoveryMode;
				DiscoverNearbyTiles();
			};
		}
		
		private static void AddDiscoveredTags(ObjectDataCD objectData, bool automaticallyCollected) {
			objectData.amount = 0;
			
			if (OptionsManager.Instance.AddTag(objectData, ObjectTagType.Discovered))
				OptionsManager.Instance.AddTag(objectData, ObjectTagType.New);
			
			if (automaticallyCollected && OptionsManager.Instance.AutoMarkDiscoveredAsCollected && ItemBrowserAPI.IsChecklistIndexed(objectData) && !OptionsManager.Instance.HasTag(objectData, ObjectTagType.Uncollected))
				OptionsManager.Instance.AddTag(objectData, ObjectTagType.Collected);
		}
		
		private static void DiscoverNearbyObjects(Entity entity, EntityManager entityManager, GameObject graphicalObject) {
			if (!_shouldDiscoverTilesAndObjects)
				return;
			
			// Discover placed objects and creatures
			var objectData = entityManager.GetComponentData<ObjectDataCD>(entity);
			AddDiscoveredTags(GetObjectDataToUse(objectData), false);
		}
		
		private static void DiscoverNearbyTiles() {
			if (!_shouldDiscoverTilesAndObjects || Time.frameCount % 60 != 0)
				return;
			
			Manager.audio.ambientSoundsHandler.GetNearbyTileData(out var tileCounts).Complete();

			foreach (var entry in tileCounts) {
				if (entry.Value == 0)
					continue;

				if (TileUtility.TryGetAssociatedObject(entry.Key.TileType, entry.Key.Tileset, out var associatedObjectData))
					AddDiscoveredTags(GetObjectDataToUse(associatedObjectData), false);
			}
		}

		public static void AddAlreadyDiscoveredObjects() {
			var characterData = Manager.saves.GetValue<CharacterData[]>("characterData");
			var currentCharacterData = characterData[Manager.saves.GetCharacterId()];

			var autoMarkDiscoveredAsCollected = OptionsManager.Instance.AutoMarkDiscoveredAsCollected;
			
			foreach (var discoveredObject in currentCharacterData.nonSerialized.discoveredObjects)
				AddDiscoveredTags(GetObjectDataToUse(discoveredObject), autoMarkDiscoveredAsCollected);
		}
		
		[HarmonyPatch(typeof(SaveManager), "SetObjectAsDiscovered")]
		[HarmonyPostfix]
		private static void AddDiscoveredObject(SaveManager __instance, ref bool __result, ObjectDataCD objectData) {
			if (!__result)
				return;

			AddDiscoveredTags(GetObjectDataToUse(objectData), true);
		}
		
		[HarmonyPatch(typeof(PlayerController), "DetectUndiscoveredObjectsInInventory")]
		[HarmonyPrefix]
		private static void DetectUndiscoveredObjectsInInventory(PlayerController __instance, ref List<ContainedObjectsBuffer> ___previousInventoryObjects, InventoryHandler inventoryHandler) {
			for (var i = 0; i < inventoryHandler.size; i++) {
				var containedObjectData = inventoryHandler.GetContainedObjectData(i);
				if (containedObjectData.objectID == ObjectID.None || ___previousInventoryObjects[i].Equals(containedObjectData))
					continue;

				if (PugDatabase.HasComponent<PetCD>(containedObjectData.objectID))
					AddDiscoveredTags(GetObjectDataToUse(containedObjectData), true);
			}
		}

		
		private static ObjectDataCD GetObjectDataToUse(ObjectDataCD objectData) {
			objectData.variation = ObjectUtility.GetPrimaryVariation(objectData);
			return objectData;
		}
		
		private static ObjectDataCD GetObjectDataToUse(ContainedObjectsBuffer containedObject) {
			var objectData = containedObject.objectData;

			if (InventoryHandler.TryGetExtraInventoryData<PetSkinCD>(containedObject, out var data))
				objectData.variation = data.skinIndex;
			else
				objectData.variation = ObjectUtility.GetPrimaryVariation(objectData);
			
			return objectData;
		}
	}
}