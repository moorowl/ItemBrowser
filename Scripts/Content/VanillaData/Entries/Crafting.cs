using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Api.Entries.Requirements.Types;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.Extensions;
using Pug.Properties;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Crafting : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Crafting", ObjectID.WoodenWorkBench, VanillaPriorities.Crafting);

		public (ObjectID Id, int Variation) Result { get; set; }
		public ObjectID Station { get; set; }
		public ObjectID Recipe { get; set; }
		public int Amount { get; set; }
		public float CraftingTime { get; set; }
		public ObjectID RequiresObjectNearby { get; set; }
		
		public bool UsesStation => Station != ObjectID.None;
		
		public class Provider : ObjectEntryProvider {
			private static readonly HashSet<CraftingType> AcceptedCraftingTypes = new() {
				CraftingType.Simple,
				CraftingType.ProcessResources,
				CraftingType.BossStatue,
				CraftingType.BiomeBossStatue,
				CraftingType.BiomeBossHydraStatue
			};
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var dummyStations = allObjects.Where(entry => IsDummyStation(entry.ObjectData.objectID))
					.Select(entry => entry.ObjectData.objectID)
					.ToHashSet();
				
				// Normal stations
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CraftingCD>(objectData, out var craftingCD) || !PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var objectPropertiesCD) || dummyStations.Contains(objectData.objectID))
						continue;
					
					if (!AcceptedCraftingTypes.Contains(craftingCD.craftingType))
						continue;

					if (ObjectUtils.GetLocalizedDisplayName(objectData.objectID, objectData.variation) == null)
						continue;

					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo.objectType == ObjectType.Creature)
						continue;
					
					// need to check if this is actually a crafting station, signs have a CraftingAuthoring for some reason
					var graphicalPrefab = objectInfo.prefabInfos[0].prefab.gameObject;
					if (graphicalPrefab == null || !graphicalPrefab.TryGetComponent<EntityMonoBehaviour>(out var entityMono) || (entityMono is not CraftingBuilding && entityMono is not PlayerController))
						continue;
					
					using var canCraftObjects = GetUnfilteredRecipes(objectData, objectPropertiesCD, Allocator.Temp);
					var canCraftObjectsIndexesToInclude = new HashSet<int>();
					var craftingPrerequisites = new List<CraftingPrerequisites>();
					
					if (PugDatabase.HasComponent<CraftingSlotsNeedPrerequisitesCheckCD>(objectData) && objectPropertiesCD.TryGetList<CraftingPrerequisites>(PropertyID.Crafting.recipePrerequisites, out var prerequisitesRef, Allocator.Temp)) {
						for (var i = 0; i < prerequisitesRef.Length; i++)
							craftingPrerequisites.Add(prerequisitesRef[i]);
					}
					
					if (PugDatabase.HasComponent<IncludedCraftingBuildingsBuffer>(objectData)) {
						var includedCraftingBuildings = PugDatabase.GetBuffer<IncludedCraftingBuildingsBuffer>(objectData).ConvertToList();
						var endIndex = 0;

						foreach (var includedCraftingBuilding in includedCraftingBuildings) {
							for (var i = 0; i < includedCraftingBuilding.amountOfCraftingOptions; i++) {
								var shouldInclude = (dummyStations.Contains(includedCraftingBuilding.objectID) || includedCraftingBuilding.objectID == objectData.objectID)
								                    && !IsCraftableInAnyBuilding(includedCraftingBuilding.objectID, GetIncludedBuildings(objectData.objectID));

								if (shouldInclude)
									canCraftObjectsIndexesToInclude.Add(endIndex);

								endIndex++;
							}
						}
					} else {
						for (var i = 0; i < canCraftObjects.Length; i++)
							canCraftObjectsIndexesToInclude.Add(i);
					}

					foreach (var i in canCraftObjectsIndexesToInclude) {
						var canCraftObject = canCraftObjects[i];
						var canCraftObjectInfo = PugDatabase.GetObjectInfo(canCraftObject.objectID);
						if (canCraftObjectInfo == null)
							continue;
						
						var isRequiredObjectsToCraftEmpty = canCraftObjectInfo.requiredObjectsToCraft.Count == 0 || canCraftObjectInfo.requiredObjectsToCraft.All(x => x.objectID == ObjectID.None);
						if (isRequiredObjectsToCraftEmpty && canCraftObjectInfo.craftingSettings.canOnlyUseAnyMaterialsWithTag == ObjectCategoryTag.None)
							continue;
						
						var entry = new Crafting {
							Result = (canCraftObject.objectID, 0),
							Station = objectData.objectID,
							Amount = math.max(canCraftObject.amount, 1),
							CraftingTime = craftingCD.craftingType == CraftingType.ProcessResources ? canCraftObjectInfo.craftingTime : 0f
						};

						if (craftingPrerequisites.Count > 0 && craftingPrerequisites[i].HasAny())
							entry.AddRequirement(new CraftingPrerequisitesList(craftingPrerequisites[i]));
						
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
						registry.Register(ObjectEntryType.Usage, entry.Station, 0, entry);
						foreach (var ingredient in ObjectUtils.GroupAndSumObjects(canCraftObjectInfo.requiredObjectsToCraft))
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
						foreach (var ingredient in ObjectUtils.GetAllObjectsWithTag(canCraftObjectInfo.craftingSettings.canOnlyUseAnyMaterialsWithTag))
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
					}
				}
				
				// Parchment recipes
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItemCD) || castItemCD.useType != CastItemUseType.CombineMaterials)
						continue;

					if (!PugDatabase.TryGetComponent<ParchmentRecipeCD>(objectData, out var parchmentRecipeCD))
						continue;

					var castTime = PugDatabase.TryGetComponent<CooldownCD>(objectData, out var cooldown) ? cooldown.cooldown : 0f;
					var objectToCraft = parchmentRecipeCD.objectToCraft;
					var objectToCraftInfo = PugDatabase.GetObjectInfo(objectToCraft.objectID, objectToCraft.variation);

					var entry = new Crafting {
						Result = (objectToCraft.objectID, objectToCraft.variation),
						Recipe = objectData.objectID,
						Amount = math.max(objectToCraft.amount, 1),
						CraftingTime = castTime,
						RequiresObjectNearby = parchmentRecipeCD.requiresNearbyObject
					};
					registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.Recipe, 0, entry);
					if (entry.RequiresObjectNearby != ObjectID.None)
						registry.Register(ObjectEntryType.Usage, entry.RequiresObjectNearby, 0, entry);
					foreach (var ingredient in ObjectUtils.GroupAndSumObjects(objectToCraftInfo.requiredObjectsToCraft)) {
						if (ingredient.objectID != entry.Recipe)
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
					}
				}
			}
			
			private static bool IsCraftableInAnyBuilding(ObjectID id, HashSet<ObjectID> buildings) {
				foreach (var building in buildings) {
					if (!PugDatabase.HasComponent<IncludedCraftingBuildingsBuffer>(building))
						continue;
					
					foreach (var includedCraftingBuildings in PugDatabase.GetBuffer<IncludedCraftingBuildingsBuffer>(building)) {
						if (includedCraftingBuildings.objectID == id)
							return true;
					}
				}

				return false;
			}

			private static HashSet<ObjectID> GetIncludedBuildings(ObjectID id) {
				var includedBuildings = new HashSet<ObjectID>();
				if (!PugDatabase.HasComponent<IncludedCraftingBuildingsBuffer>(id))
					return includedBuildings;
				
				foreach (var includedBuilding in PugDatabase.GetBuffer<IncludedCraftingBuildingsBuffer>(id).ConvertToList().Skip(1)) {
					includedBuildings.Add(includedBuilding.objectID);
					
					foreach (var subIncludedBuilding in GetIncludedBuildings(includedBuilding.objectID))
						includedBuildings.Add(subIncludedBuilding);
				}
				
				return includedBuildings;
			}

			private static bool IsDummyStation(ObjectID id) {
				if (id == ObjectID.Player || PugDatabase.HasComponent<PhysicsCollider>(id))
					return false;
				
				return PugDatabase.TryGetComponent<ObjectPropertiesCD>(id, out var objectPropertiesCD)
				       && !objectPropertiesCD.Has(PropertyID.PlaceableObject.placeableObject);
			}
			
			private static NativeArray<CanCraftObjectsBuffer> GetUnfilteredRecipes(ObjectDataCD objectData, ObjectPropertiesCD objectPropertiesCD, Allocator allocator) {
				if (objectPropertiesCD.Has(PropertyID.Crafting.unfilteredRecipes))
					return objectPropertiesCD.GetList<CanCraftObjectsBuffer>(PropertyID.Crafting.unfilteredRecipes, allocator);

				if (PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData)) {
					var canCraftObjectsBuffer = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					var unfilteredRecipes = new NativeArray<CanCraftObjectsBuffer>(canCraftObjectsBuffer.Length, allocator);

					for (var i = 0; i < canCraftObjectsBuffer.Length; i++)
						unfilteredRecipes[i] = canCraftObjectsBuffer[i];
				
					return unfilteredRecipes;
				}
			
				return new NativeArray<CanCraftObjectsBuffer>(0, allocator);
			}
		}
	}
}