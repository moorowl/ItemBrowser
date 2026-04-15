using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using I2.Loc;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Api.Overrides;
using ItemBrowser.Utilities.Extensions;
using PlayerEquipment;
using Pug.Properties;
using PugMod;
using PugTilemap;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class ObjectUtility {
		public static readonly IEnumerable<ObjectType> ArmorObjectTypes = new List<ObjectType> {
			ObjectType.Helm,
			ObjectType.BreastArmor,
			ObjectType.PantsArmor
		};
		public static readonly IEnumerable<ObjectType> AccessoryObjectTypes = new List<ObjectType> {
			ObjectType.Ring,
			ObjectType.Necklace,
			ObjectType.Offhand,
			ObjectType.Lantern
		};
		public static readonly IEnumerable<ObjectType> ToolObjectTypes = new List<ObjectType> {
			ObjectType.MiningPick,
			ObjectType.Sledge,
			ObjectType.DrillTool,
			ObjectType.Shovel,
			ObjectType.FishingRod,
			ObjectType.Hoe,
			ObjectType.PaintTool,
			ObjectType.Bucket,
			ObjectType.RoofingTool
		};
		public static readonly IEnumerable<ObjectType> MiningToolObjectTypes = new List<ObjectType> {
			ObjectType.MiningPick,
			ObjectType.Sledge,
			ObjectType.DrillTool
		};
		
		private static readonly Dictionary<ObjectDataCD, string> DisplayNames = new();
		private static readonly Dictionary<ObjectDataCD, string> DisplayNameNotes = new();
		private static readonly Dictionary<ObjectDataCD, string> Descriptions = new();
		private static readonly Dictionary<ObjectID, HashSet<string>> Categories = new();
		private static readonly Dictionary<ObjectDataCD, int> PrimaryVariations = new();
		private static readonly Dictionary<ObjectDataCD, HashSet<string>> AssociatedConditionCategories = new();
		private static readonly Dictionary<ObjectDataCD, int> ArmorValues = new();
		private static readonly Dictionary<ObjectDataCD, Sprite> IconOverrides = new();

		internal static void Bake() {
			BakeDisplayNamesAndNotes();
			BakeCategories();
			BakePrimaryVariations();
			BakeConditions();
		}

		private static void BakeDisplayNamesAndNotes() {
			DisplayNames.Clear();
			DisplayNameNotes.Clear();
			Descriptions.Clear();
			IconOverrides.Clear();

			var nameOverrides = new Dictionary<ObjectDataCD, string>();
			var nameNotes = new Dictionary<ObjectDataCD, string>();

			foreach (var objectOverride in ScriptableData.GetDataBlocks<ItemBrowserObjectOverrideDataBlock>()) {
				var appliesToObjectData = objectOverride.AppliesToObjectData;
				
				if (objectOverride.overrideName && !string.IsNullOrWhiteSpace(objectOverride.name))
					nameOverrides.TryAdd(appliesToObjectData, objectOverride.name);
				
				if (objectOverride.overrideIcon)
					IconOverrides.TryAdd(appliesToObjectData, objectOverride.icon);
				
				if (objectOverride.showNameNote)
					nameNotes.TryAdd(appliesToObjectData, objectOverride.nameNote);
			}
			
			foreach (var objectData in GetAllObjects()) {
				// Setup display names
				string localizedName;
				string unlocalizedName;
				if (nameOverrides.TryGetValue(objectData, out var term)) {
					localizedName = API.Localization.GetLocalizedTerm(term);
					unlocalizedName = term;
				} else {
					var localizedNameFormatFields = PlayerController.GetObjectName(new ContainedObjectsBuffer {
						objectData = objectData
					}, true);
					var unlocalizedNameFormatFields = PlayerController.GetObjectName(new ContainedObjectsBuffer {
						objectData = objectData
					}, false);

					localizedName = localizedNameFormatFields.text;
					unlocalizedName = unlocalizedNameFormatFields.text;
					
					if (localizedNameFormatFields.dontLocalize)
						localizedName = unlocalizedNameFormatFields.text;
				}

				if (localizedName != null)
					DisplayNames.TryAdd(objectData, localizedName.Replace("\n", ""));
				
				if (nameNotes.TryGetValue(objectData, out var unlocalizedNameNote))
					DisplayNameNotes.TryAdd(objectData, unlocalizedNameNote);

				var localizedDescription = API.Localization.GetLocalizedTerm($"Items/{PlayerController.GetAnyObjectIDReplaceForNameAndDesc(objectData.objectID)}Desc");
				if (localizedDescription != null)
					Descriptions.TryAdd(objectData, localizedDescription);
				
				if (!Categories.ContainsKey(objectData.objectID))
					Categories[objectData.objectID] = new HashSet<string>();
			}
			
			// Setup display name sort orders
			//DisplayNameSortOrders.Clear();
			var objectNames = new List<(ObjectDataCD ObjectData, string Name)>();
			
			foreach (var objectData in GetAllObjects()) {
				var objectName = DisplayNames.GetValueOrDefault(objectData);
				objectName ??= $"ZZZ+{GetInternalName(objectData.objectID)}+{objectData.variation}";
				objectName += $"+{objectData.variation}";

				objectNames.Add((objectData, objectName));
			}
			
			objectNames.Sort((a, b) => string.Compare(a.Name, b.Name, LocalizationManager.CurrentCulture, CompareOptions.StringSort));

			for (var i = 0; i < objectNames.Count; i++) {
				var (objectData, _) = objectNames[i];
				//DisplayNameSortOrders.TryAdd(objectData, i);
			}
		}

		private static void BakeCategories() {
			Categories.Clear();
			
			foreach (var subCategory in ObjectIDCategoryManager.SubCategories) {
				foreach (var objectId in subCategory.ObjectIds) {
					if (!Categories.TryGetValue(objectId, out var categories))
						continue;
					
					categories.Add(subCategory.ToString());
				}
			}
		}
		
		private static void BakePrimaryVariations() {
			PrimaryVariations.Clear();
			
			var nameToPrimaryVariation = new Dictionary<string, int>();
			foreach (var objectData in GetAllObjects().OrderBy(objectData => objectData.variation)) {
				var displayName = DisplayNames.GetValueOrDefault(objectData) ?? $"{objectData.objectID}:{objectData.variation}";
				var displayNameNote = DisplayNameNotes.GetValueOrDefault(objectData);

				if (displayNameNote != null)
					displayName = $"{displayName} ({displayNameNote})";

				if (nameToPrimaryVariation.TryGetValue(displayName, out var primaryVariation)) {
					PrimaryVariations[objectData] = primaryVariation;
				} else {
					nameToPrimaryVariation[displayName] = objectData.variation;
					PrimaryVariations[objectData] = objectData.variation;
				}
			}
		}
		
		private static void BakeConditions() {
			AssociatedConditionCategories.Clear();
			ArmorValues.Clear();

			var conditionToCategory = new Dictionary<ConditionID, string>();
			foreach (var conditionCategory in Manager.ui.conditionsIconsTable.conditionCategories) {
				foreach (var condition in conditionCategory.conditions)
					conditionToCategory.TryAdd(condition.Id, conditionCategory.category);
			}
			
			foreach (var objectData in GetAllObjects()) {
				var associatedConditions = new List<(ConditionID Id, int Value)>();
				
				// Equipped conditions
				if (PugDatabase.HasComponent<GivesConditionsWhenEquippedBuffer>(objectData)) {
					foreach (var givesConditionsWhenEquipped in PugDatabase.GetBuffer<GivesConditionsWhenEquippedBuffer>(objectData))
						associatedConditions.Add((givesConditionsWhenEquipped.equipmentCondition.id, givesConditionsWhenEquipped.equipmentCondition.value));
				}
				
				// Consumed conditions
				if (PugDatabase.HasComponent<GivesConditionsWhenConsumedBuffer>(objectData)) {
					foreach (var givesConditionsWhenEquipped in PugDatabase.GetBuffer<GivesConditionsWhenConsumedBuffer>(objectData)) {
						associatedConditions.Add((givesConditionsWhenEquipped.conditionDataContainer.conditionData.conditionID, givesConditionsWhenEquipped.conditionDataContainer.conditionData.value));
						
						if (PugDatabase.HasComponent<CookingIngredientCD>(objectData))
							associatedConditions.Add((givesConditionsWhenEquipped.conditionDataContainer.conditionDataWhenCooked.conditionID, givesConditionsWhenEquipped.conditionDataContainer.conditionDataWhenCooked.value));
					}
				}

				// Set bonus conditions
				var setBonusesTable = Manager.ui.mouse.setBonusesTable;
				var associatedSetBonus = setBonusesTable.GetSetBonusID(objectData.objectID);
				if (associatedSetBonus != SetBonusID.None) {
					var associatedSetBonusInfo = setBonusesTable.GetSetBonusInfo(associatedSetBonus);
					
					foreach (var data in associatedSetBonusInfo.setBonusDatas)
						associatedConditions.Add((data.conditionData.conditionID, data.conditionData.value));
				}
				
				// Creature conditions (pets)
				if (PugDatabase.HasComponent<ConditionsBuffer>(objectData)) {
					foreach (var initialCondition in PugDatabase.GetBuffer<ConditionsBuffer>(objectData))
						associatedConditions.Add((initialCondition.condition.conditionData.conditionID, initialCondition.condition.conditionData.value));
				}
				if (PugDatabase.TryGetComponent<InitialRandomConditionsCD>(objectData, out var initialRandomConditionsCD)) {
					ref var initialConditions = ref initialRandomConditionsCD.initialConditions.Value;
					
					for (var i = 0; i < initialConditions.Length; i++)
						associatedConditions.Add((initialConditions[i].condition.conditionID, initialConditions[i].condition.value));
				}
				
				// Pet talents
				if (PugDatabase.HasComponent<PetTalentPoolBuffer>(objectData)) {
					var petInfosTable = Manager.ui.petInfosTable;

					foreach (var petTalent in PugDatabase.GetBuffer<PetTalentPoolBuffer>(objectData)) {
						var petTalentInfo = petInfosTable.petTalents.FirstOrDefault(petTalentInfo => petTalentInfo.petTalentID == petTalent.petTalentID);
						associatedConditions.Add((petTalentInfo.conditionID, petTalentInfo.value));
					}
				}

				var highestArmorValue = associatedConditions
					.Where(associatedCondition => {
						var conditionEffect = Manager.ui.conditionsIconsTable.GetConditionInfo(associatedCondition.Id).effect;
						return conditionEffect is ConditionEffect.Armor or ConditionEffect.ArmorPercentage;
					})
					.Select(associatedCondition => associatedCondition.Value)
					.DefaultIfEmpty(0)
					.Max();

				ArmorValues.TryAdd(objectData, highestArmorValue);
				AssociatedConditionCategories.TryAdd(objectData, associatedConditions
					.Select(associatedCondition => associatedCondition.Id)
					.Where(id => id != ConditionID.None)
					.Select(id => conditionToCategory.GetValueOrDefault(id))
					.ToHashSet()
				);
			}
		}

		public static string GetLocalizedDisplayName(ObjectDataCD objectData) {
			return DisplayNames.GetValueOrDefault(objectData);
		}
		
		public static string GetLocalizedDisplayName(ObjectID id, int variation = 0) {
			return GetLocalizedDisplayName(new ObjectDataCD { objectID = id, variation = variation });
		}

		public static string GetLocalizedDisplayNameOrDefault(ObjectDataCD objectData) {
			var displayName = GetLocalizedDisplayName(objectData);
			if (displayName == null)
				return objectData.objectID == ObjectID.None ? GetInternalName(objectData) : $"{GetInternalName(objectData)} ({objectData.variation})";

			return displayName;
		}
		
		public static string GetLocalizedDisplayNameOrDefault(ObjectID id, int variation = 0) {
			return GetLocalizedDisplayNameOrDefault(new ObjectDataCD { objectID = id, variation = variation });
		}

		public static string GetUnlocalizedDisplayNameNote(ObjectDataCD objectData) {
			return DisplayNameNotes.GetValueOrDefault(objectData);
		}
		
		public static string GetUnlocalizedDisplayNameNote(ObjectID id, int variation = 0) {
			return GetUnlocalizedDisplayNameNote(new ObjectDataCD { objectID = id, variation = variation });
		}
		
		public static string GetLocalizedDescription(ObjectDataCD objectData) {
			return Descriptions.GetValueOrDefault(objectData);
		}
		
		public static string GetLocalizedDescription(ObjectID id, int variation = 0) {
			return GetLocalizedDescription(new ObjectDataCD { objectID = id, variation = variation });
		}
		
		public static string GetInternalName(ObjectDataCD objectData) {
			return API.Authoring.ObjectProperties.TryGetPropertyString(objectData.objectID, "name", out var internalName) ? internalName : objectData.objectID.ToString();
		}
		
		public static string GetInternalName(ObjectID id) {
			return GetInternalName(new ObjectDataCD { objectID = id });
		}

		public static HashSet<string> GetCategories(ObjectDataCD objectData) {
			return Categories.TryGetValue(objectData.objectID, out var categories) ? categories : new HashSet<string>();
		}
		
		public static HashSet<string> GetCategories(ObjectID id) {
			return GetCategories(new ObjectDataCD { objectID = id });
		}

		public static int GetPrimaryVariation(ObjectDataCD objectData) {
			return PrimaryVariations.GetValueOrDefault(objectData, 0);
		}
		
		public static int GetPrimaryVariation(ObjectID id, int variation = 0) {
			return GetPrimaryVariation(new ObjectDataCD { objectID = id, variation = variation });
		}

		public static bool IsPrimaryVariation(ObjectDataCD objectData) {
			return GetPrimaryVariation(objectData) == objectData.variation;
		}
		
		public static bool IsPrimaryVariation(ObjectID id, int variation) {
			return IsPrimaryVariation(new ObjectDataCD { objectID = id, variation = variation });
		}

		public static HashSet<string> GetAssociatedConditionCategories(ObjectDataCD objectData) {
			return AssociatedConditionCategories.GetValueOrDefault(objectData);
		}
		
		public static HashSet<string> GetAssociatedConditionCategories(ObjectID id, int variation) {
			return GetAssociatedConditionCategories(new ObjectDataCD { objectID = id, variation = variation });
		}

		public static bool HasBeenDiscovered(ObjectDataCD objectData, bool considerNonObtainablesDiscovered = false) {
			if (objectData.objectID == ObjectID.None)
				return false;
			
			if (considerNonObtainablesDiscovered)
				return IsNonObtainable(objectData) || Manager.saves.HasDiscoveredObject(objectData.objectID, objectData.variation);
			
			return Manager.saves.HasDiscoveredObject(objectData.objectID, objectData.variation);
		}
		
		public static bool HasBeenDiscovered(ObjectID id, int variation = 0, bool considerNonObtainablesDiscovered = false) {
			return HasBeenDiscovered(new ObjectDataCD { objectID = id, variation = variation }, considerNonObtainablesDiscovered);
		}

		public static Sprite GetIcon(ObjectDataCD objectData, bool preferSmallIcons = false) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
			if (objectInfo == null)
				return null;
			
			var iconToUse = preferSmallIcons ? (objectInfo.smallIcon ?? objectInfo.icon) : objectInfo.icon;
			var iconOverride = Manager.ui.itemOverridesTable.GetIconOverride(objectData, preferSmallIcons);
			if (iconOverride != null)
				iconToUse = iconOverride;
			
			if (IconOverrides.TryGetValue(objectData, out iconOverride))
				iconToUse = iconOverride;

			return iconToUse;
		}
		
		public static Sprite GetIcon(ObjectID id, int variation = 0, bool preferSmallIcons = false) {
			return GetIcon(new ObjectDataCD { objectID = id, variation = variation }, preferSmallIcons);
		}

		public static bool IsNonObtainable(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
			if (objectInfo == null)
				return true;

			if (PugDatabase.HasComponent<PetCD>(objectData))
				return false;
			
			if (objectInfo.objectType is ObjectType.NonObtainable or ObjectType.Creature or ObjectType.Critter or ObjectType.PlayerType && !PugDatabase.HasComponent<CraftingCD>(objectData))
				return true;

			if (PugDatabase.HasComponent<DontSerializeCD>(objectData) && !PugDatabase.HasComponent<TileCD>(objectData) && !PugDatabase.HasComponent<TileCD>(objectData) && objectInfo.objectType is not ObjectType.Creature or ObjectType.Critter)
				return true;
			
			var primaryPrefabEntity = PugDatabase.GetPrimaryPrefabEntity(objectData.objectID, ClientWorldStateSystem.PugDatabaseBank, objectData.variation);
			if (primaryPrefabEntity != Entity.Null) {
				if (EntityUtility.IsComponentEnabled<IndestructibleCD>(primaryPrefabEntity, API.Client.World))
					return true;
				
				if (EntityUtility.IsComponentEnabled<DontDropSelfCD>(primaryPrefabEntity, API.Client.World) && objectInfo.objectType != ObjectType.PlaceablePrefab && !PugDatabase.HasComponent<IndirectProjectileCD>(objectData) && !PugDatabase.HasComponent<WayPointCD>(objectData) && !PugDatabase.HasComponent<PortalCD>(objectData))
					return true;
			}
			
			if (PugDatabase.HasComponent<AllowHealthRegenerationInCombatCD>(objectData) && PugDatabase.GetComponent<HealthCD>(objectData).maxHealth >= 9999999 && PugDatabase.GetComponent<HealthRegenerationCD>(objectData).NormalizedHealthIncreasePerFiveSeconds >= 1f)
				return true;

			if (!ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any())
				return true;

			return false;
		}
		
		public static bool IsNonObtainable(ObjectID id, int variation = 0) {
			return IsNonObtainable(new ObjectDataCD { objectID = id, variation = variation });
		}
		
		public static IEnumerable<ObjectDataCD> GroupAndSumObjects(IEnumerable<ObjectDataCD> objects, bool ignoreVariation) {
			return objects.GroupBy(objectData => ignoreVariation ? (int) objectData.objectID : (((int) objectData.objectID) * 10000) + objectData.variation)
				.Select(group => {
					var first = group.First();
					return new ObjectDataCD {
						objectID = first.objectID,
						variation = first.variation,
						amount = group.Sum(objectData => PugDatabase.GetObjectInfo(first.objectID, first.variation) is { isStackable: true } ? objectData.amount : 1)
					};
				});
		}
		
		public static IEnumerable<CraftingObject> GroupAndSumObjects(IEnumerable<CraftingObject> objects) {
			return objects.GroupBy(objectData => objectData.objectID)
				.Select(group => {
					var first = group.First();
					return new CraftingObject {
						objectID = first.objectID,
						amount = group.Sum(objectData => PugDatabase.GetObjectInfo(first.objectID) is { isStackable: true } ? objectData.amount : 1)
					};
				});
		}
		
		public static IEnumerable<ObjectDataCD> GetAllObjects() {
			return PugDatabase.objectsByType.Keys;
		}
		
		public static IEnumerable<ObjectDataCD> GetAllObjectsWithTag(ObjectCategoryTag tag) {
			if (tag == ObjectCategoryTag.None)
				return Array.Empty<ObjectDataCD>();
			
			return GetAllObjects().Where(objectData => objectData.variation == 0 && PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).tags.Contains(tag));
		}

		public static int GetDamage(ObjectDataCD objectData) {
			if (PugDatabase.TryGetComponent<HasWeaponDamageCD>(objectData, out var hasWeaponDamageCD)) {
				if (hasWeaponDamageCD.isMagic)
					return GetDamage(objectData, DamageCategory.Magic);
				
				if (hasWeaponDamageCD.isRange)
					return GetDamage(objectData, DamageCategory.PhysicalRange);

				return math.max(GetDamage(objectData, DamageCategory.PhysicalMelee), GetDamage(objectData, DamageCategory.Explosive));
			}

			return math.max(GetDamage(objectData, DamageCategory.Summon), GetDamage(objectData, DamageCategory.Explosive));
		}
		
		public static int GetDamage(ObjectID id, int variation = 0) {
			return GetDamage(new ObjectData { objectID = id, variation = variation });
		}

		private static int GetDamageFromLevelEntity(ObjectDataCD objectData) {
			var levelEntity = EntityUtility.GetLevelEntity(objectData);
			if (levelEntity != Entity.Null && EntityUtility.TryGetComponentData<WeaponDamageCD>(levelEntity, API.Client.World, out var weaponDamageCD))
				return weaponDamageCD.GetDamage(false);

			return 0;
		}

		public enum DamageCategory {
			PhysicalMelee,
			PhysicalRange,
			Magic,
			Summon,
			Explosive,
			Trap
		}
		
		public static int GetDamage(ObjectDataCD objectData, DamageCategory category) {
			switch (category) {
				case DamageCategory.PhysicalMelee:
				case DamageCategory.PhysicalRange:
				case DamageCategory.Magic:
					if (!PugDatabase.TryGetComponent<HasWeaponDamageCD>(objectData, out var hasWeaponDamageCD))
						return 0;

					if (category == DamageCategory.PhysicalMelee && (hasWeaponDamageCD.isRange || hasWeaponDamageCD.isMagic))
						return 0;

					if (category == DamageCategory.PhysicalRange && (!hasWeaponDamageCD.isRange || hasWeaponDamageCD.isMagic))
						return 0;
					
					if (category == DamageCategory.Magic && !hasWeaponDamageCD.isMagic)
						return 0;
			
					return GetDamageFromLevelEntity(objectData);
				case DamageCategory.Summon:
					if (!PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUseCD) || !secondaryUseCD.summonsMinion)
						return 0;

					if (!PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) || !PugDatabase.TryGetComponent<MinionCD>(secondaryUseCD.minionToSpawn, out var minionCD))
						return 0;
					
					return MinionExtensions.GetMinionBaseDamage(minionCD, levelCD.level);
				case DamageCategory.Explosive:
					return StatsUIUtility.HasExplosiveWeapon(objectData, out var isExplosiveCD, API.Client.World) ? isExplosiveCD.damage : 0;
			}

			return 0;
		}

		public static int GetDamage(ObjectID id, int variation, DamageCategory category) {
			return GetDamage(new ObjectDataCD { objectID = id, variation = variation }, category);
		}

		public static int GetBaseLevel(ObjectDataCD objectData) {
			return PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) ? levelCD.level : 0;
		}
		
		public static int GetBaseLevel(ObjectID id, int variation = 0) {
			return GetBaseLevel(new ObjectData { objectID = id, variation = variation });
		}

		public static int GetArmor(ObjectDataCD objectData) {
			return ArmorValues.GetValueOrDefault(objectData);
		}

		public static int GetArmor(ObjectID id, int variation = 0) {
			return GetArmor(new ObjectData { objectID = id, variation = variation });
		}

		// from InventoryUtility.GetRaritySellValue
		public static int GetValue(ObjectDataCD objectData, bool buy = false) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
			if (objectData.objectID == ObjectID.None || PugDatabase.HasComponent<CantBeSoldAuthoring>(objectData) || objectInfo.rarity == Rarity.Legendary)
				return 0;

			var sellValue = objectInfo.sellValue;
			if (sellValue < 0) {
				sellValue = GetRaritySellValue(objectInfo.rarity);
				
				if (PugDatabase.HasComponent<CookedFoodAuthoring>(objectData)) {
					var primaryIngredient = CookedFoodCD.GetPrimaryIngredientFromVariation(objectData.variation);
					var secondaryIngredient = CookedFoodCD.GetSecondaryIngredientFromVariation(objectData.variation);
					sellValue = GetValue(primaryIngredient, 0, buy) + GetValue(secondaryIngredient, 0, buy);
				} else {
					var extraSellFromIngredients = 0;
					var requiredObjectsToCraft = objectInfo.requiredObjectsToCraft;

					foreach (var craftingObject in requiredObjectsToCraft) {
						var ingredientObjectInfo = PugDatabase.GetObjectInfo(craftingObject.objectID);
						if (ingredientObjectInfo.sellValue != 0)
							extraSellFromIngredients += GetRaritySellValue(ingredientObjectInfo.rarity) * craftingObject.amount;
					}

					if (extraSellFromIngredients > 0)
						sellValue = (int) math.round(math.max(1f, sellValue * 0.3f) + extraSellFromIngredients);
				}

				var randomization = Unity.Mathematics.Random.CreateFromIndex((uint)objectData.objectID).NextFloat(-0.1f, 0.1f);
				sellValue = math.max(1, sellValue + (int) math.round(sellValue * randomization));
			}

			if (buy) {
				sellValue = math.max(1, sellValue);
				var buyValueMultiplier = objectInfo.buyValueMultiplier;
				return (int) math.round(sellValue * 5f * buyValueMultiplier);
			}

			return sellValue;
		}
		
		public static int GetValue(ObjectID id, int variation = 0, bool buy = false) {
			return GetValue(new ObjectData { objectID = id, variation = variation }, buy);
		}

		// from InventoryUtility.GetRaritySellValue
		private static int GetRaritySellValue(Rarity rarity) {
			return 1 + math.max(0, (int) rarity) * 5;
		}

		public static void GetTotalAmountInInventoryAndNearbyChests(PlayerController player, ObjectDataCD objectData, out int inInventory, out int inNearbyChests) {
			objectData = new ObjectDataCD {
				objectID = objectData.objectID,
				variation = GetPrimaryVariation(objectData.objectID, objectData.variation)
			};
			inInventory = 0;
			inNearbyChests = 0;
			
			var querySystem = player.querySystem;
			if (querySystem == null)
				return;

			var inventoryBufferLookup = querySystem.GetBufferLookup<InventoryBuffer>();
			var containedObjectsBufferLookup = querySystem.GetBufferLookup<ContainedObjectsBuffer>();
			
			inInventory = GetTotalAmountInEntity(player.entity, objectData, inventoryBufferLookup, containedObjectsBufferLookup);
			foreach (var chest in player.playerCraftingHandler.GetNearbyChests())
				inNearbyChests += GetTotalAmountInEntity(chest, objectData, inventoryBufferLookup, containedObjectsBufferLookup);
		}
		
		private static int GetTotalAmountInEntity(Entity entity, ObjectDataCD objectData, BufferLookup<InventoryBuffer> inventoryBufferLookup, BufferLookup<ContainedObjectsBuffer> containedObjectsBufferLookup) {
			var isStackable = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).isStackable;
			var amount = 0;
			
			var containedObjectsBuffer = containedObjectsBufferLookup[entity];
			foreach (var item in inventoryBufferLookup[entity]) {
				for (var i = item.startIndex; i < item.startIndex + item.size; i++)  {
					if (containedObjectsBuffer[i].objectID == objectData.objectID && GetPrimaryVariation(objectData.objectID, containedObjectsBuffer[i].variation) == objectData.variation)
						amount = isStackable ? amount + containedObjectsBuffer[i].amount : amount + 1;
				}
			}
			
			return amount;
		}
		
		public static void GetTotalAmountInInventoryAndNearbyChests(PlayerController player, ObjectCategoryTag tag, out int inInventory, out int inNearbyChests) {
			inInventory = 0;
			inNearbyChests = 0;
			
			var querySystem = player.querySystem;
			if (querySystem == null)
				return;

			var inventoryBufferLookup = querySystem.GetBufferLookup<InventoryBuffer>();
			var containedObjectsBufferLookup = querySystem.GetBufferLookup<ContainedObjectsBuffer>();
			
			inInventory = GetTotalAmountInEntity(player.entity, tag, inventoryBufferLookup, containedObjectsBufferLookup);
			foreach (var chest in player.playerCraftingHandler.GetNearbyChests())
				inNearbyChests += GetTotalAmountInEntity(chest, tag, inventoryBufferLookup, containedObjectsBufferLookup);
		}
		
		private static int GetTotalAmountInEntity(Entity entity, ObjectCategoryTag tag, BufferLookup<InventoryBuffer> inventoryBufferLookup, BufferLookup<ContainedObjectsBuffer> containedObjectsBufferLookup) {
			var amount = 0;
			
			var containedObjectsBuffer = containedObjectsBufferLookup[entity];
			foreach (var item in inventoryBufferLookup[entity]) {
				for (var i = item.startIndex; i < item.startIndex + item.size; i++) {
					var objectInfo = PugDatabase.GetObjectInfo(containedObjectsBuffer[i].objectID);
					if (objectInfo != null && objectInfo.tags.Contains(tag))
						amount = objectInfo.isStackable ? amount + containedObjectsBuffer[i].amount : amount + 1;
				}
			}
			
			return amount;
		}
		
		public static HashSet<ObjectID> GetObjectsInInventoryAndNearbyChests(PlayerController player) {
			var objects = new HashSet<ObjectID>();
			
			var querySystem = player.querySystem;
			if (querySystem == null)
				return objects;

			var inventoryBufferLookup = querySystem.GetBufferLookup<InventoryBuffer>();
			var containedObjectsBufferLookup = querySystem.GetBufferLookup<ContainedObjectsBuffer>();
			
			AddObjectsInEntity(player.entity, objects, inventoryBufferLookup, containedObjectsBufferLookup);
			foreach (var chest in player.playerCraftingHandler.GetNearbyChests())
				AddObjectsInEntity(chest, objects, inventoryBufferLookup, containedObjectsBufferLookup);

			return objects;
		}
		
		private static void AddObjectsInEntity(Entity entity, HashSet<ObjectID> objects, BufferLookup<InventoryBuffer> inventoryBufferLookup, BufferLookup<ContainedObjectsBuffer> containedObjectsBufferLookup) {
			var containedObjectsBuffer = containedObjectsBufferLookup[entity];
			foreach (var inventory in inventoryBufferLookup[entity]) {
				for (var i = inventory.startIndex; i < inventory.startIndex + inventory.size; i++)
					objects.Add(containedObjectsBuffer[i].objectID);
			}
		}
		
		public static List<(ObjectID Id, List<Biome> Biomes, List<Tileset> Tilesets)> GetAllCritterSpawnAreas(List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
			return allObjects
				.Where(entry => PugDatabase.TryGetComponent<ObjectPropertiesCD>(entry.ObjectData, out var propertiesCD) && propertiesCD.Has(PropertyID.Critter.spawnContinuously))
				.GroupBy(entry => entry.ObjectData.objectID)
				.Select(group => {
					var objectData = group.First().ObjectData;
					var propertiesCD = PugDatabase.GetComponent<ObjectPropertiesCD>(objectData);

					var biomesToSpawnIn = propertiesCD.GetManagedList<Biome>(PropertyID.Critter.biomesToSpawnIn);
					var tilesetsToSpawnIn = propertiesCD.Has(PropertyID.Critter.tilesetsToSpawnIn)
						? propertiesCD.GetManagedList<Tileset>(PropertyID.Critter.tilesetsToSpawnIn)
						: new List<Tileset>();

					biomesToSpawnIn.Remove(Biome.None);
					
					return (
						objectData.objectID,
						biomesToSpawnIn,
						tilesetsToSpawnIn
					);
				})
				.ToList();
		}
		
		private static readonly HashSet<EquipmentSlotType> CarriedEquipmentSlotTypes = new() {
			EquipmentSlotType.MeleeWeaponSlot,
			EquipmentSlotType.RangeWeaponSlot,
			EquipmentSlotType.ShovelSlot,
			EquipmentSlotType.HoeSlot,
			EquipmentSlotType.BugNet,
			EquipmentSlotType.SeederSlot,
			EquipmentSlotType.Shield,
			EquipmentSlotType.InstrumentSlot,
			EquipmentSlotType.FishingRodSlot
		};
		
		public static bool IsCarriedObject(ObjectType objectType) {
			var equipmentSlotType = PlayerController.GetEquippedSlotTypeForObjectType(objectType, default, default, default, default);
			return objectType != ObjectType.ThrowingWeapon && CarriedEquipmentSlotTypes.Contains(equipmentSlotType);
		}
	}
}