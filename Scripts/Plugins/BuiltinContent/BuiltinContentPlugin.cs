using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Plugins.BuiltinContent.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using PugMod;
using PugProperties;
using PugTilemap;
using Unity.Physics;

namespace ItemBrowser.Plugins.BuiltinContent {
	public class BuiltinContentPlugin : ItemBrowserPlugin {
		public override string AssociatedMod => Main.InternalName;
		public override bool AutomaticallyRegisterFromAssets => true;

		public override void OnEarlyRegister(ItemBrowserRegistry registry) {
			foreach (var objectData in ObjectUtils.GetAllObjects()) {
				if (IsItemIndexed(objectData)) {
					registry.AddItem(objectData);
					
					if (IsTechnicalItem(objectData))
						registry.AddTechnicalItem(objectData);
				}

				if (IsCreatureIndexed(objectData)) {
					registry.AddCreature(objectData);
					
					if (IsTechnicalCreature(objectData))
						registry.AddTechnicalCreature(objectData);
				}
			}
		}

		public override void OnRegister(ItemBrowserRegistry registry) {
			AddProviders(registry);
			AddSorters(registry);
			AddFilters(registry);
		}

		private static void AddProviders(ItemBrowserRegistry registry) {
			var providers = new ObjectEntryProvider[] {
				new ArchaeologistDrops.Provider(),
				new BackgroundPerks.Provider(),
				new CattleProduce.Provider(),
				new Crafting.Provider(),
				new Drops.Provider(),
				new Farming.Provider(),
				new Fishing.Provider(),
				new JewelryCrafter.Provider(),
				new LockedChestDrops.Provider(),
				new Merchant.Provider(),
				new NaturalSpawnInitial.Provider(),
				new NaturalSpawnRespawn.Provider(),
				new OreBoulderExtraction.Provider(),
				new Salvaging.Provider(),
				new Trading.Provider(),
				new Loot.Provider(),
				new ChallengeArenaReward.Provider(),
				new BuiltinContent.Entries.VendingMachine.Provider(),
				new StructureContents.Provider(),
				new NaturalSpawnAroundObject.Provider(),
				new TerrainGeneration.Provider(),
				new MerchantSpawning.Provider(),
				new Miscellaneous.Provider(),
				new Breeding.Provider(),
				new CreatureSummoning.Provider(),
				new UpgradeMaterial.Provider(),
				new DropsWhenDamaged.Provider(),
				new Unlocking.Provider(),
				new Bucketing.Provider(),
				new CookingIngredient.Provider(),
				new TerritoryContents.Provider()
			};
			
			foreach (var provider in providers)
				registry.AddEntryProvider(provider);
		}
		
		private static void AddSorters(ItemBrowserRegistry registry) {
			// Item sorters
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData.objectID, objectData.variation)
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/Damage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation)
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/Level") {
				Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation)
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/Value") {
				Function = objectData => ObjectUtils.GetValue(objectData.objectID, objectData.variation)
			});
			
			// Creature sorters
			registry.AddCreatureSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData.objectID, objectData.variation)
			});
			registry.AddCreatureSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			registry.AddCreatureSorter(new Sorter<ObjectDataCD>("ItemBrowser:Sorters/Level") {
				Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation)
			});
		}
		
		private static void AddFilters(ItemBrowserRegistry registry) {
			AddFilters_Source(registry);
			AddFilters_Damage(registry);
			AddFilters_Equipment(registry);
			AddFilters_Type(registry);
			AddFilters_Utility(registry);
			AddFilters_Faction(registry);
			AddFilters_Rarity(registry);
			AddFilters_Level(registry);
			AddFilters_VersionAdded(registry);
		}

		private static void AddFilters_Source(ItemBrowserRegistry registry) {
			// Source
			const string sourceGroup = "ItemBrowser:Filters/Source";
			registry.AddItemFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Item_FromMods") {
				Function = ModUtils.IsModded
			});
			registry.AddCreatureFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Creature_FromMods") {
				Function = objectData => (int) objectData.objectID > Constants.maxNonModdedObjectID
			});
			foreach (var mod in API.ModLoader.LoadedMods.OrderBy(mod => ModUtils.GetDisplayName(mod.ModId))) {
				var displayName = ModUtils.GetDisplayName(mod.ModId);
				
				var objectIds = ModUtils.GetAssociatedObjects(mod.ModId);
				var itemIds = objectIds
					.Where(ItemBrowserAPI.IsItemIndexed)
					.ToList();
				var creatureIds = objectIds
					.Where(ItemBrowserAPI.IsCreatureIndexed)
					.ToList();

				if (itemIds.Count > 0) {
					registry.AddItemFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Item_FromMod") {
						NameFormatFields = new[] { displayName },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { displayName },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => itemIds.Contains(objectData),
						Group = sourceGroup
					});
				}
				
				if (creatureIds.Count > 0) {
					registry.AddCreatureFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Creature_FromMod") {
						NameFormatFields = new[] { displayName },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { displayName },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => creatureIds.Contains(objectData),
						Group = sourceGroup
					});	
				}
			}
		}

		private static void AddFilters_Damage(ItemBrowserRegistry registry) {
			// Item damage
			const string damageGroup = "ItemBrowser:Filters/Damage";
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_AnyDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_PhysicalMeleeDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.PhysicalMelee) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_PhysicalRangeDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.PhysicalRange) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_MagicDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.Magic) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_SummonDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.Summon) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_ExplosiveDamage") {
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation, ObjectUtils.DamageCategory.Explosive) > 0,
				Group = damageGroup
			});
		}
		
		private static void AddFilters_Equipment(ItemBrowserRegistry registry) {
			// Item equipment
			const string equipmentGroup = "ItemBrowser:Filters/Equipment";
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Weapon") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (PugDatabase.HasComponent<HasWeaponDamageCD>(objectData) && !ObjectUtils.MiningToolObjectTypes.Contains(objectType))
						return true;

					return PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Tool") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.ToolObjectTypes.Contains(objectType);
				}
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Armor") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.ArmorObjectTypes.Contains(objectType);
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Helm") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Helm;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_BreastArmor") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.BreastArmor;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_PantsArmor") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PantsArmor;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Accessory") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.AccessoryObjectTypes.Contains(objectType);
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Ring") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Ring;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Necklace") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Necklace;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_OffHand") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Offhand;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Bag") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Bag;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Pouch") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Pouch;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Lantern") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Lantern;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Pet") {
				Function = objectData => PugDatabase.HasComponent<PetCD>(objectData),
				Group = equipmentGroup
			});
		}
		
		private static void AddFilters_Type(ItemBrowserRegistry registry) {
			// Creature type
			const string typeGroup = "ItemBrowser:Filters/Type";
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Hostile") {
				Function = objectData => !ObjectCategoryTagsCD.HasTag(PugDatabase.GetComponent<ObjectCategoryTagsCD>(objectData).tagsBitMask, ObjectCategoryTag.NonHostileCreature)
				                         && !PugDatabase.HasComponent<CattleCD>(objectData)
				                         && !PugDatabase.HasComponent<CritterCD>(objectData)
				                         && !PugDatabase.HasComponent<MerchantCD>(objectData),
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Boss") {
				Function = objectData => PugDatabase.HasComponent<BossCD>(objectData) || ObjectUtils.GetCategories(objectData.objectID).Contains("Boss/BossCreature"),
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Merchant") {
				Function = PugDatabase.HasComponent<MerchantCD>,
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Cattle") {
				Function = PugDatabase.HasComponent<CattleCD>,
                                                             				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Critter") {
				Function = PugDatabase.HasComponent<CritterCD>,
				Group = typeGroup
			});
		}
		
		private static void AddFilters_Utility(ItemBrowserRegistry registry) {
			// Utility
			const string utilityGroup = "ItemBrowser:Filters/Utility";
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Placeable") {
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PlaceablePrefab
					       && PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var properties)
					       && properties.Has(PropertyID.PlaceableObject.placeableObject);
				}
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Consumable") {
				Function = objectData => PugDatabase.HasComponent<GivesConditionsWhenConsumedBuffer>(objectData)
				                         || (PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItem) && castItem.useType != CastItemUseType.LeashCattle)
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Craftable") {
				Function = objectData => {
					var craftingHandler = Manager.main.player?.playerCraftingHandler;
					if (craftingHandler == null)
						return false;

					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo == null)
						return false;

					if (!ItemBrowserAPI.ObjectEntryRegistry.GetEntries<Crafting>(ObjectEntryType.Source, objectData).Any())
						return false;

					var nearbyChests = craftingHandler.GetNearbyChests();
					var recipeInfo = new CraftingHandler.RecipeInfo(objectInfo, 1);

					return craftingHandler.HasMaterialsInCraftingInventoryToCraftRecipe(recipeInfo, true, nearbyChests, true);
				},
				FunctionIsDynamic = true,
				CausesItemCraftingRequirementsToDisplay = true
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_CookingIngredient") {
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CookingIngredient);
				}
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Paintable") {
				Function = PugDatabase.HasComponent<PaintableObjectCD>
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Technical_Item") {
				Function = ItemBrowserAPI.IsTechnicalItem,
				DefaultState = () => Options.DefaultTechnicalFilter ? FilterState.Exclude : FilterState.None
			});
			registry.AddCreatureFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Technical_Creature") {
				Function = ItemBrowserAPI.IsTechnicalCreature,
				DefaultState = () => Options.DefaultTechnicalFilter ? FilterState.Exclude : FilterState.None
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_NoSources_Item") {
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => Options.CheatMode ? FilterState.None : FilterState.Exclude
			});
			registry.AddCreatureFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_NoSources_Creature") {
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => Options.CheatMode ? FilterState.None : FilterState.Exclude
			});
			/*registry.AddItemFilter(utilityGroup, new($"{utilityGroup}_IsNonObtainable") {
				Function = objectData => ObjectUtils.IsNonObtainable(objectData.objectID, objectData.variation)
			});*/
		}
		
		private static readonly HashSet<FactionID> UnusedCreatureFactions = new() {
			FactionID.AttacksAllButNotPlayer,
			FactionID.PlayerMinion,
			FactionID.Explosion,
			FactionID.__MAX_VALUE
		};
		
		private static void AddFilters_Faction(ItemBrowserRegistry registry) {
			// Creature faction
			const string factionGroup = "ItemBrowser:Filters/Faction";
			foreach (var faction in Enum.GetValues(typeof(FactionID)).Cast<FactionID>()) {
				if (UnusedCreatureFactions.Contains(faction))
					continue;
				
				registry.AddCreatureFilter(factionGroup, new Filter<ObjectDataCD>($"ItemBrowser:FactionNames/{faction}", $"{factionGroup}_FactionDesc") {
					DescriptionFormatFields = new[] {
						$"ItemBrowser:FactionNames/{faction}"
					},
					Function = objectData => PugDatabase.TryGetComponent<FactionCD>(objectData, out var factionCD) && factionCD.faction == faction,
					Group = factionGroup
				});
			}
		}
		
		private static void AddFilters_Rarity(ItemBrowserRegistry registry) {
			// Item rarity
			const string rarityGroup = "ItemBrowser:Filters/Rarity";
			foreach (var rarity in Enum.GetValues(typeof(Rarity)).Cast<Rarity>()) {
				registry.AddItemFilter(rarityGroup, new Filter<ObjectDataCD>($"{rarityGroup}_{rarity}") {
					Function = objectData => PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).rarity == rarity,
					Group = rarityGroup
				});
			}
		}
		
		private static void AddFilters_Level(ItemBrowserRegistry registry) {
			// Item level
			const string levelGroup = "ItemBrowser:Filters/Level";
			for (var i = 1; i <= LevelScaling.GetMaxLevel(); i++) {
				var level = i;
				registry.AddItemFilter(levelGroup, new Filter<ObjectDataCD>($"{levelGroup}_Level") {
					NameFormatFields = new[] { i.ToString() },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { i.ToString() },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => ObjectUtils.GetBaseLevel(objectData.objectID, objectData.variation) == level,
					Group = levelGroup
				});
			}
		}
		
		private static void AddFilters_VersionAdded(ItemBrowserRegistry registry) {
			// Version added
			const string versionGroup = "ItemBrowser:Filters/VersionAdded";
			foreach (var version in ObjectsAddedByVersion.AllVersions) {
				if (version.HasAnyItems) {
					registry.AddItemFilter(versionGroup, new Filter<ObjectDataCD>($"{versionGroup}_Item_Version") {
						NameFormatFields = new[] { version.Name },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { version.Name },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => version.Objects.Contains(objectData.objectID),
						Group = versionGroup
					});	
				}
				if (version.HasAnyCreatures) {
					registry.AddCreatureFilter(versionGroup, new Filter<ObjectDataCD>($"{versionGroup}_Creature_Version") {
						NameFormatFields = new[] { version.Name },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { version.Name },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => version.Objects.Contains(objectData.objectID),
						Group = versionGroup
					});
				}
			}
		}

		private static bool IsItemIndexed(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;
			
			if (objectInfo.objectType is ObjectType.Creature or ObjectType.Critter && !PugDatabase.HasComponent<PetCD>(objectData))
				return false;
			
			if (!ObjectUtils.IsPrimaryVariation(objectData.objectID, objectData.variation))
				return false;
			
			return !objectInfo.isCustomScenePrefab;
		}
		
		private static bool IsCreatureIndexed(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;

			if ((objectInfo.objectType != ObjectType.Creature && objectInfo.objectType != ObjectType.Critter) || PugDatabase.HasComponent<PetCD>(objectData))
				return false;

			if (!ObjectUtils.IsPrimaryVariation(objectData.objectID, objectData.variation))
				return false;

			return !objectInfo.isCustomScenePrefab;
		}

		private static bool IsTechnicalItem(ObjectDataCD item) {
			var objectInfo = PugDatabase.GetObjectInfo(item.objectID, item.variation);
					
			if (PugDatabase.HasComponent<ProjectileCD>(item) && !PugDatabase.HasComponent<IndirectProjectileCD>(item))
				return true;
					
			if (PugDatabase.TryGetComponent<TileCD>(item, out var tileCD) && tileCD.tileType == TileType.ground)
				return true;

			if (ObjectUtils.GetLocalizedDisplayName(item.objectID, item.variation) == null)
				return true;
					
			if (objectInfo.objectType == ObjectType.NonObtainable && !PugDatabase.HasComponent<DestructibleObjectCD>(item) && !PugDatabase.HasComponent<SoulOrbCD>(item) && !PugDatabase.HasComponent<DiggableCD>(item))
				return true;
					
			if (item.objectID != ObjectID.Player && PugDatabase.HasComponent<CraftingCD>(item) && !PugDatabase.HasComponent<PhysicsCollider>(item))
				return true;
					
			return false;
		}
		
		private static bool IsTechnicalCreature(ObjectDataCD creature) {
			if (PugDatabase.HasComponent<MinionCD>(creature))
				return true;

			if (ObjectUtils.GetLocalizedDisplayName(creature.objectID, creature.variation) == null)
				return true;
					
			return false;
		}
	}
}