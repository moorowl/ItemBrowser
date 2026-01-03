using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using I2.Loc;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using PugMod;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class ObjectUtils {
		public static readonly IEnumerable<ObjectType> CreatureObjectTypes = new List<ObjectType> {
			ObjectType.Creature,
			ObjectType.Pet,
			ObjectType.PlayerType
		};
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
			ObjectType.BeamWeapon,
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
			ObjectType.DrillTool,
			ObjectType.BeamWeapon
		};
		
		private static readonly Dictionary<ObjectDataCD, string> DisplayNames = new();
		private static readonly Dictionary<ObjectDataCD, string> DisplayNameNotes = new();
		private static readonly Dictionary<ObjectDataCD, int> DisplayNameSortOrders = new();
		private static readonly Dictionary<ObjectID, HashSet<string>> Categories = new();
		private static readonly Dictionary<ObjectDataCD, int> PrimaryVariations = new();

		private static readonly MemberInfo MiObjectPrefabEntityLookup = typeof(PugDatabase).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "objectPrefabEntityLookup");
		private static Dictionary<ObjectDataCD, Entity> ObjectPrefabEntityLookupRef;
		
		internal static void InitOnWorldLoad() {
			ObjectPrefabEntityLookupRef = (Dictionary<ObjectDataCD, Entity>) API.Reflection.GetValue(MiObjectPrefabEntityLookup, null);
			SetupDisplayNamesAndCategories();
		}
		
		private static void SetupDisplayNamesAndCategories() {
			DisplayNames.Clear();
			DisplayNameNotes.Clear();
			Categories.Clear();
			
			var authoringList = new List<MonoBehaviour>();
			authoringList.AddRange(Manager.ecs.pugDatabase.prefabList);
			authoringList.AddRange(Manager.mod.ExtraAuthoring);
			
			foreach (var authoring in authoringList) {
				var gameObject = authoring.gameObject;
				if (!gameObject.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
					continue;

				var objectInfo = entityMonoBehaviourData.ObjectInfo;
				var objectData = new ObjectDataCD {
					objectID = objectInfo.objectID,
					variation = objectInfo.variation
				};
				
				string localizedName;
				string unlocalizedName;
				if (ItemBrowserAPI.Registry.ObjectNameOverrides.TryGetValue(objectData, out var term)) {
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
				
				if (ItemBrowserAPI.Registry.ObjectNameNotes.TryGetValue(objectData, out var unlocalizedNameNote))
					DisplayNameNotes.TryAdd(objectData, unlocalizedNameNote);
				
				if (!Categories.ContainsKey(objectData.objectID))
					Categories[objectData.objectID] = new HashSet<string>();
			}
			
			DisplayNameSortOrders.Clear();
			var objectNames = new List<(ObjectDataCD ObjectData, string Name)>();
			
			foreach (var authoring in authoringList) {
				var gameObject = authoring.gameObject;
				if (!gameObject.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
					continue;

				var objectInfo = entityMonoBehaviourData.ObjectInfo;
				var objectData = new ObjectDataCD {
					objectID = objectInfo.objectID,
					variation = objectInfo.variation
				};
				var objectName = DisplayNames.GetValueOrDefault(objectData);
				objectName ??= $"ZZZ+{GetInternalName(objectData.objectID)}+{objectData.variation}";
				objectName += $"+{objectData.variation}";

				objectNames.Add((objectData, objectName));
			}
			
			objectNames.Sort((a, b) => string.Compare(a.Name, b.Name, LocalizationManager.CurrentCulture, CompareOptions.StringSort));

			for (var i = 0; i < objectNames.Count; i++) {
				var (objectData, _) = objectNames[i];
				DisplayNameSortOrders.TryAdd(objectData, i);
			}
			
			foreach (var subCategory in ObjectIDCategoryManager.SubCategories) {
				foreach (var objectId in subCategory.ObjectIds) {
					if (!Categories.TryGetValue(objectId, out var categories))
						continue;
					
					categories.Add(subCategory.ToString());
				}
			}

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
		
		public static string GetLocalizedDisplayName(ObjectID id, int variation = 0) {
			return DisplayNames.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}
		
		public static string GetLocalizedDisplayNameOrDefault(ObjectID id, int variation = 0) {
			return GetLocalizedDisplayName(id, variation) ?? $"{GetInternalName(id)} ({variation})";
		}
		
		public static string GetUnlocalizedDisplayNameNote(ObjectID id, int variation = 0) {
			return DisplayNameNotes.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}
		
		public static int GetDisplayNameSortOrder(ObjectID id, int variation = 0) {
			return DisplayNameSortOrders.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}

		public static string GetInternalName(ObjectID id) {
			return API.Authoring.ObjectProperties.TryGetPropertyString(id, "name", out var internalName) ? internalName : id.ToString();
		}
		
		public static HashSet<string> GetCategories(ObjectID id) {
			return Categories.TryGetValue(id, out var categories) ? categories : new HashSet<string>();
		}

		public static int GetPrimaryVariation(ObjectID id, int variation) {
			return PrimaryVariations.GetValueOrDefault(new ObjectDataCD {
				objectID = id,
				variation = variation
			}, 0);
		}
		
		public static bool IsPrimaryVariation(ObjectID id, int variation) {
			return GetPrimaryVariation(id, variation) == variation;
		}

		public static bool HasBeenDiscovered(ObjectID id, int variation = 0, bool considerNonObtainablesDiscovered = false) {
			if (id == ObjectID.None)
				return false;
			
			if (considerNonObtainablesDiscovered)
				return IsNonObtainable(id, variation) || Manager.saves.HasDiscoveredObject(id, variation);
			
			return Manager.saves.HasDiscoveredObject(id, variation);
		}

		public static Sprite GetIcon(ObjectID id, int variation = 0, bool preferSmallIcons = false) {
			var objectInfo = PugDatabase.GetObjectInfo(id, variation);
			if (objectInfo == null)
				return null;

			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			
			var iconToUse = preferSmallIcons ? (objectInfo.smallIcon ?? objectInfo.icon) : objectInfo.icon;
			var iconOverride = Manager.ui.itemOverridesTable.GetIconOverride(objectData, preferSmallIcons);
			if (iconOverride != null)
				iconToUse = iconOverride;
			
			if (ItemBrowserAPI.Registry.ObjectIconOverrides.TryGetValue(objectData, out iconOverride))
				iconToUse = iconOverride;

			return iconToUse;
		}
		
		public static bool IsNonObtainable(ObjectID id, int variation = 0) {
			var objectInfo = PugDatabase.GetObjectInfo(id, variation);
			if (objectInfo == null)
				return true;

			var objectData = new ObjectData {
				objectID = id,
				variation = variation,
				amount = 1
			};

			if (objectInfo.objectType is ObjectType.NonObtainable or ObjectType.Creature or ObjectType.Critter or ObjectType.PlayerType && !PugDatabase.HasComponent<PetCD>(objectData))
				return true;

			if (PugDatabase.HasComponent<DontSerializeCD>(objectData) && !PugDatabase.HasComponent<TileCD>(objectData) && !PugDatabase.HasComponent<TileCD>(objectData) && objectInfo.objectType is not ObjectType.Creature or ObjectType.Critter)
				return true;
			
			if (ObjectPrefabEntityLookupRef.TryGetValue(objectData, out var primaryPrefabEntity)) {
				if (EntityUtility.IsComponentEnabled<IndestructibleCD>(primaryPrefabEntity, API.Client.World))
					return true;
				
				if (EntityUtility.IsComponentEnabled<DontDropSelfCD>(primaryPrefabEntity, API.Client.World) && objectInfo.objectType != ObjectType.PlaceablePrefab && !PugDatabase.HasComponent<IndirectProjectileCD>(objectData) && !PugDatabase.HasComponent<WayPointCD>(objectData) && !PugDatabase.HasComponent<PortalCD>(objectData))
					return true;
			}
			
			if (PugDatabase.HasComponent<AllowHealthRegenerationInCombatCD>(id, variation) && PugDatabase.GetComponent<HealthCD>(id, variation).maxHealth >= 9999999 && PugDatabase.GetComponent<HealthRegenerationCD>(id, variation).NormalizedHealthIncreasePerFiveSeconds >= 1f)
				return true;

			if (!ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, id, variation).Any())
				return true;

			return false;
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
		
		public static int GetDamage(ObjectID id, int variation = 0) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};

			if (PugDatabase.TryGetComponent<HasWeaponDamageCD>(objectData, out var hasWeaponDamageCD)) {
				if (hasWeaponDamageCD.isMagic)
					return GetDamage(id, variation, DamageCategory.Magic);
				
				if (hasWeaponDamageCD.isRange)
					return GetDamage(id, variation, DamageCategory.PhysicalRange);

				return math.max(GetDamage(id, variation, DamageCategory.PhysicalMelee), GetDamage(id, variation, DamageCategory.Explosive));
			}

			return math.max(GetDamage(id, variation, DamageCategory.Summon), GetDamage(id, variation, DamageCategory.Explosive));
		}

		private static int GetDamageFromLevelEntity(ObjectID id, int variation = 0) {
			var levelEntity = EntityUtility.GetLevelEntity(new ObjectData {
				objectID = id,
				variation = variation
			});
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
		
		public static int GetDamage(ObjectID id, int variation, DamageCategory category) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};

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
			
					return GetDamageFromLevelEntity(id, variation);
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
		
		public static int GetBaseLevel(ObjectID id, int variation = 0) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			return PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) ? levelCD.level : 0;
		}

		// from InventoryUtility.GetRaritySellValue
		public static int GetValue(ObjectID id, int variation = 0, bool buy = false) {
			var objectData = new ObjectData {
				objectID = id,
				variation = variation
			};
			
			var objectInfo = PugDatabase.GetObjectInfo(id, variation);
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

		// from InventoryUtility.GetRaritySellValue
		private static int GetRaritySellValue(Rarity rarity) {
			return 1 + math.max(0, (int) rarity) * 5;
		}
	}
}