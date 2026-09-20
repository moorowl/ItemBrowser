using System.Collections.Generic;
using Inventory;
using ItemBrowser.Common.Api;
using PugMod;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class MaterialInfoUtility {
		public static List<PugDatabase.MaterialInfo> GetMaterialInfos(params ObjectWithAmount[] materials) {
			var results = new List<PugDatabase.MaterialInfo>();

			var querySystem = API.Client.World.GetExistingSystemManaged<PugQuerySystem>();
			var nearbyChests = ClientWorldStateSystem.NearbyChests;

			using var cookingIngredientsRequired = new NativeList<ObjectWithAmount>(Allocator.Temp);
			using var inventories = new NativeList<Entity>(Allocator.Temp);
			inventories.Add(Manager.main.player.entity);
			
			foreach (var nearbyChest in nearbyChests)
				inventories.Add(nearbyChest);
			
			using var objectsRequired = new NativeList<ObjectWithAmount>(1, Allocator.Temp);
			foreach (var material in materials)
				objectsRequired.Add(material);
			
			using var materialInfos = InventoryUtility.GetMaterialInfos(
				querySystem.GetBufferLookup<ContainedObjectsBuffer>(),
				querySystem.GetBufferLookup<InventoryBuffer>(),
				ClientWorldStateSystem.PugDatabaseBank,
				objectsRequired,
				1f,
				inventories,
				1,
				Allocator.Temp
			);

			for (var i = 0; i < materialInfos.Length; i++) {
				GetNearestMatchingChest(nearbyChests, materialInfos[i].nearbyChestWithMaterial, out var nearestChest, out var nearestChestIcon);

				results.Add(new PugDatabase.MaterialInfo(
					materialInfos[i].objectID,
					materialInfos[i].amountNeeded,
					materialInfos[i].amountAvailable,
					nearestChest,
					nearestChestIcon
				));
			}

			return results;
		}
		
		public static List<PugDatabase.MaterialInfo> GetMaterialInfosForRecipe(ObjectDataCD objectToCraft) {
			var results = new List<PugDatabase.MaterialInfo>();
			
			var objectInfo = PugDatabase.GetObjectInfo(objectToCraft.objectID, objectToCraft.variation);
			if (objectInfo == null)
				return results;

			var querySystem = API.Client.World.GetExistingSystemManaged<PugQuerySystem>();
			var nearbyChests = ClientWorldStateSystem.NearbyChests;
			
			using var inventories = new NativeList<Entity>(Allocator.Temp);
			inventories.Add(Manager.main.player.entity);
				
			foreach (var nearbyChest in nearbyChests)
				inventories.Add(nearbyChest);
			
			using var cookingIngredientsRequired = new NativeList<ObjectWithAmount>(Allocator.Temp);
			if (PugDatabase.HasComponent<CookedFoodCD>(objectToCraft)) {
				cookingIngredientsRequired.Add(new ObjectWithAmount {
					objectID = CookedFoodCD.GetPrimaryIngredientFromVariation(objectToCraft.variation)
				});
				cookingIngredientsRequired.Add(new ObjectWithAmount {
					objectID = CookedFoodCD.GetSecondaryIngredientFromVariation(objectToCraft.variation)
				});
			}
				
			using var materialInfos = InventoryUtility.GetCraftingMaterialInfosForRecipe(
				ClientWorldStateSystem.PugDatabaseBank,
				querySystem.GetBufferLookup<ContainedObjectsBuffer>(),
				querySystem.GetBufferLookup<InventoryBuffer>(),
				querySystem.GetComponentLookup<AnvilCD>(),
				querySystem.GetComponentLookup<ObjectDataCD>(),
				querySystem.GetBufferLookup<SummarizedConditionsBuffer>(),
				querySystem.GetComponentLookup<DurabilityCD>(),
				querySystem.GetComponentLookup<PrioritizedRepairMaterialCD>(),
				querySystem.GetComponentLookup<LevelCD>(),
				new ObjectWithAmount { objectID = objectToCraft.objectID, amount = 1 },
				cookingIngredientsRequired,
				inventories,
				1,
				false,
				false,
				Manager.main.player.entity,
				Manager.main.player.entity,
				Allocator.Temp
			);

			for (var i = 0; i < materialInfos.Length; i++) {
				GetNearestMatchingChest(nearbyChests, materialInfos[i].nearbyChestWithMaterial, out var nearestChest, out var nearestChestIcon);

				results.Add(new PugDatabase.MaterialInfo(
					materialInfos[i].objectID,
					materialInfos[i].amountNeeded,
					materialInfos[i].amountAvailable,
					nearestChest,
					nearestChestIcon
				));
			}

			return results;
		}
		
		private static void GetNearestMatchingChest(List<Entity> allNearbyChests, Entity chestInMaterialInfo, out Entity nearestChest, out Sprite nearestChestIcon) {
			nearestChest = Entity.Null;
			nearestChestIcon = null;

			foreach (var nearbyChest in allNearbyChests) {
				if (nearbyChest != chestInMaterialInfo)
					continue;

				nearestChest = nearbyChest;
				break;
			}
			
			if (EntityUtility.TryGetComponentData<ObjectDataCD>(nearestChest, API.Client.World, out var nearestChestObjectData))
				nearestChestIcon = PugDatabase.GetObjectInfo(nearestChestObjectData.objectID, nearestChestObjectData.variation)?.smallIcon;
		}
	}
}