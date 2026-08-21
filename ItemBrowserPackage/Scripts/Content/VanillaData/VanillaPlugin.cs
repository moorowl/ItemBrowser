using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.Options.DiscoveredObjects;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using Pug.Properties;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData {
	public class VanillaPlugin : ItemBrowserPlugin {
		public override bool AutomaticallyRegisterFromAssets => true;

		private const int CattleVariationCount = 5;

		public override void OnEarlyRegister(ItemBrowserRegistry registry) {
			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				if (IsItemIndexed(objectData))
					registry.AddItem(objectData);

				if (IsCreatureIndexed(objectData))
					registry.AddCreature(objectData);
				
				if (IsTechnicalObject(objectData))
					registry.AddTechnicalObject(objectData);
					
				if (IsDeprecatedObject(objectData))
					registry.AddDeprecatedObject(objectData);
			}
		}

		public override void OnRegister(ItemBrowserRegistry registry) {
			AddProviders(registry);
			AddSorters(registry);
			AddFilters(registry);
		}

		public override void OnLateRegister(ItemBrowserRegistry registry) {
			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				if (IsChecklistObject(objectData)) {
					var variationsToAdd = new List<int>();

					if (PugDatabase.TryGetComponent<PetCD>(objectData, out var petCD) && petCD.maxSkins >= 1) {
						for (var i = 0; i < petCD.maxSkins; i++)
							variationsToAdd.Add(i);
					} else if (PugDatabase.HasComponent<CattleCD>(objectData)) {
						// TODO This assumes all cattle will have 5 variations, maybe change to support mods that add a different amount (or no variations?)
						for (var i = 0; i < CattleVariationCount; i++)
							variationsToAdd.Add(i);
					} else {
						variationsToAdd.Add(objectData.variation);
					}

					foreach (var variation in variationsToAdd) {
						registry.AddToChecklist(new ObjectDataCD {
							objectID = objectData.objectID,
							variation = variation
						});
					}
				}
			}
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
				new VanillaData.Entries.VendingMachine.Provider(),
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
				new TerritoryContents.Provider(),
				new SeedExtracting.Provider(),
				new CritterCatching.Provider(),
				new PetTalents.Provider()
			};
			
			foreach (var provider in providers)
				registry.AddEntryProvider(provider);
		}
		
		private static void AddSorters(ItemBrowserRegistry registry) {
			registry.AddSorter(new Sorter("ItemBrowser-Sorters/Alphabetical") {
				Function = allObjectData => allObjectData.OrderByDescending(objectData => {
					var localizedDisplayName = ObjectUtility.GetLocalizedDisplayName(objectData);
					return localizedDisplayName ?? $"ZZZ+{ObjectUtility.GetInternalName(objectData)}:{objectData.variation}";
				}, StringComparer.Create(LocalizationManager.CurrentCulture, true)),
				Scope = FilterAndSorterScope.All
			});
			registry.AddSorter(new Sorter("ItemBrowser-Sorters/InternalIndex") {
				Function = allObjectData => allObjectData.OrderBy(objectData => (int) objectData.objectID * 10000 + objectData.variation),
				AdditionalInfoFunction = objectData => $"{(int) objectData.objectID}:{objectData.variation}",
				Scope = FilterAndSorterScope.All
			});
			registry.AddSorter(new Sorter("ItemBrowser-Sorters/Damage") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetDamage),
				AdditionalInfoFunction = objectData => {
					var damage = ObjectUtility.GetDamage(objectData);
					if (damage == 0)
						return null;
					
					var damageVariance = (int) (damage * 0.1f);
					return $"{(damage - damageVariance).ToString()}-{(damage + damageVariance).ToString()}";
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddSorter(new Sorter("ItemBrowser-Sorters/Armor") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetArmor),
				AdditionalInfoFunction = objectData => {
					var armor = ObjectUtility.GetArmor(objectData);
					return armor > 0 ? $"+{armor:0.#}" : null;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddSorter(new Sorter("ItemBrowser-Sorters/Level") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetBaseLevel),
				AdditionalInfoFunction = objectData => {
					var baseLevel = ObjectUtility.GetBaseLevel(objectData);
					return baseLevel > 0 ? baseLevel.ToString() : null;
				},
				Scope = FilterAndSorterScope.All
			});
			registry.AddSorter(new Sorter("ItemBrowser-Sorters/Value") {
				Function = allObjectData => allObjectData.OrderBy(objectData => ObjectUtility.GetValue(objectData)),
				AdditionalInfoFunction = objectData => {
					var value = ObjectUtility.GetValue(objectData);
					return value != 0 ? value.ToString() : null;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
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
			const string sourceGroup = "ItemBrowser-Filters/Source";
			const string unknownModSymbol = "?";

			var itemsByMod = new Dictionary<long, HashSet<ObjectDataCD>>();
			var creaturesByMod = new Dictionary<long, HashSet<ObjectDataCD>>();
			var checklistObjectsByMod = new Dictionary<long, HashSet<ObjectDataCD>>();
			
			// Setup Mod Name -> Associated items/creatures
			var modsToCheck = API.ModLoader.LoadedMods.OrderBy(mod => ModUtility.GetDisplayName(mod.ModId)).Select(mod => mod.ModId).ToList();
			modsToCheck.Add(ModUtility.UnknownModId);

			foreach (var mod in modsToCheck) {
				var associatedObjects = ModUtility.GetAssociatedObjects(mod);

				var associatedItems = associatedObjects.Where(ItemBrowserAPI.IsItemIndexed).ToHashSet();
				var associatedCreatures = associatedObjects.Where(ItemBrowserAPI.IsCreatureIndexed).ToHashSet();
				var associatedChecklistObjects = associatedObjects.Where(ItemBrowserAPI.IsChecklistIndexed).ToHashSet();

				if (associatedItems.Count > 0)
					itemsByMod.TryAdd(mod, associatedItems);
				
				if (associatedCreatures.Count > 0)
					creaturesByMod.TryAdd(mod, associatedCreatures);
				
				if (associatedChecklistObjects.Count > 0)
					checklistObjectsByMod.TryAdd(mod, associatedChecklistObjects);
			}
			
			// General modded content filters
			if (itemsByMod.Count > 0) {
				registry.AddFilter(sourceGroup, new Filter($"{sourceGroup}_FromMods") {
					Symbol = "#",
					Function = ModUtility.IsModded,
					Scope = FilterAndSorterScope.Items
				});	
			}
			if (creaturesByMod.Count > 0) {
				registry.AddFilter(sourceGroup, new Filter($"{sourceGroup}_FromMods") {
					Symbol = "#",
					Function = ModUtility.IsModded,
					Scope = FilterAndSorterScope.Creatures
				});
			}
			if (checklistObjectsByMod.Count > 0) {
				registry.AddFilter(sourceGroup, new Filter($"{sourceGroup}_FromMods") {
					Symbol = "#",
					Function = ModUtility.IsModded,
					Scope = FilterAndSorterScope.Checklist
				});
			}

			// Specific mod filters
			foreach (var (mod, associatedItems) in itemsByMod) {
				var displayName = ModUtility.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtility.UnknownModId;

				registry.AddFilter(sourceGroup, new Filter($"{sourceGroup}_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
					Symbol = isUnknownMod ? unknownModSymbol : displayName[..Math.Min(displayName.Length, 2)],
					NameFormatFields = new[] { displayName },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { displayName },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => associatedItems.Contains(objectData),
					Scope = FilterAndSorterScope.Items,
					Group = sourceGroup
				});
			}
			foreach (var (mod, associatedCreatures) in creaturesByMod) {
				var displayName = ModUtility.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtility.UnknownModId;

				registry.AddFilter(sourceGroup, new Filter($"{sourceGroup}_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
					Symbol = isUnknownMod ? unknownModSymbol : displayName[..Math.Min(displayName.Length, 2)],
					NameFormatFields = new[] { displayName },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { displayName },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => associatedCreatures.Contains(objectData),
					Scope = FilterAndSorterScope.Creatures,
					Group = sourceGroup
				});
			}
			foreach (var (mod, associatedChecklistObjects) in checklistObjectsByMod) {
				var displayName = ModUtility.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtility.UnknownModId;

				registry.AddFilter(sourceGroup, new Filter($"{sourceGroup}_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
					Symbol = isUnknownMod ? unknownModSymbol : displayName[..Math.Min(displayName.Length, 2)],
					NameFormatFields = new[] { displayName },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { displayName },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => associatedChecklistObjects.Contains(objectData),
					Scope = FilterAndSorterScope.Checklist,
					Group = sourceGroup
				});
			}
		}

		private static void AddFilters_Damage(ItemBrowserRegistry registry) {
			// Item damage
			const string damageGroup = "ItemBrowser-Filters/Damage";
			registry.AddFilter(damageGroup, new Filter($"{damageGroup}_AnyDamage") {
				Symbol = "#",
				Function = objectData => ObjectUtility.GetDamage(objectData.objectID, objectData.variation) > 0,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = damageGroup
			});
			registry.AddFilter(damageGroup, new Filter($"{damageGroup}_PhysicalMeleeDamage") {
				Icon = ObjectID.RustyDagger,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.PhysicalMelee) > 0,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = damageGroup
			});
			registry.AddFilter(damageGroup, new Filter($"{damageGroup}_PhysicalRangeDamage") {
				Icon = ObjectID.Slingshot,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.PhysicalRange) > 0,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = damageGroup
			});
			registry.AddFilter(damageGroup, new Filter($"{damageGroup}_MagicDamage") {
				Icon = ObjectID.BasicStaff,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.Magic) > 0,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = damageGroup
			});
			registry.AddFilter(damageGroup, new Filter($"{damageGroup}_SummonDamage") {
				Icon = ObjectID.TomeOfRange,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.Summon) > 0,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = damageGroup
			});
			registry.AddFilter(damageGroup, new Filter($"{damageGroup}_ExplosiveDamage") {
				Icon = ObjectID.Bomb,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.Explosive) > 0,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = damageGroup
			});
		}
		
		private static void AddFilters_Equipment(ItemBrowserRegistry registry) {
			// Item equipment
			const string equipmentGroup = "ItemBrowser-Filters/Equipment";
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Weapon") {
				Icon = ObjectID.TinDagger,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (PugDatabase.HasComponent<HasWeaponDamageCD>(objectData) && !ObjectUtility.ToolObjectTypes.Contains(objectType) && objectType != ObjectType.PlaceablePrefab && !(PugDatabase.TryGetComponent<BeamWeaponCD>(objectData, out var beamWeaponCD) && beamWeaponCD.isStickyBeam))
						return true;

					return PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Tool") {
				Icon = ObjectID.Bucket,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return (ObjectUtility.ToolObjectTypes.Contains(objectType) && objectType != ObjectType.PlaceablePrefab) || (PugDatabase.TryGetComponent<BeamWeaponCD>(objectData, out var beamWeaponCD) && beamWeaponCD.isStickyBeam);
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Armor") {
				Icon = ObjectID.IronShield,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtility.ArmorObjectTypes.Contains(objectType);
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Helm") {
				Icon = ObjectID.IronHelm,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Helm;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_BreastArmor") {
				Icon = ObjectID.IronBreastArmor,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.BreastArmor;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_PantsArmor") {
				Icon = ObjectID.IronPantsArmor,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PantsArmor;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Accessory") {
				Icon = ObjectID.HeartBerryNecklace,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtility.AccessoryObjectTypes.Contains(objectType);
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Ring") {
				Icon = ObjectID.CavelingMothersRing,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Ring;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Necklace") {
				Icon = ObjectID.GoldCrystalNecklace,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Necklace;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_OffHand") {
				Icon = ObjectID.OracleDeck,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Offhand;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Bag") {
				Icon = ObjectID.ExplorerBackpack,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Bag;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Pouch") {
				Icon = ObjectID.ValuablePouch,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Pouch;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Lantern") {
				Icon = ObjectID.Lantern,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Lantern;
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
			registry.AddFilter(equipmentGroup, new Filter($"{equipmentGroup}_Pet") {
				Icon = ObjectID.PetCat,
				Function = PugDatabase.HasComponent<PetCD>,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
				Group = equipmentGroup
			});
		}
		
		private static void AddFilters_Type(ItemBrowserRegistry registry) {
			// Creature type
			const string typeGroup = "ItemBrowser-Filters/Type";
			registry.AddFilter(typeGroup, new Filter($"{typeGroup}_Hostile") {
				Icon = ObjectID.AggressiveSlimeBlob,
				Function = objectData => !ObjectCategoryTagsCD.HasTag(PugDatabase.GetComponent<ObjectCategoryTagsCD>(objectData).tagsBitMask, ObjectCategoryTag.NonHostileCreature)
				                         && !PugDatabase.HasComponent<CattleCD>(objectData)
				                         && !PugDatabase.HasComponent<CritterCD>(objectData)
				                         && !PugDatabase.HasComponent<MerchantCD>(objectData),
				Scope = FilterAndSorterScope.Creatures,
				Group = typeGroup
			});
			registry.AddFilter(typeGroup, new Filter($"{typeGroup}_Boss") {
				Icon = ObjectID.SlimeBossCrystal,
				Function = objectData => PugDatabase.HasComponent<BossCD>(objectData) || ObjectUtility.GetCategories(objectData).Contains("Boss/BossCreature"),
				Scope = FilterAndSorterScope.Creatures,
				Group = typeGroup
			});
			registry.AddFilter(typeGroup, new Filter($"{typeGroup}_Merchant") {
				Icon = ObjectID.SlimeMerchant,
				Function = PugDatabase.HasComponent<MerchantCD>,
				Scope = FilterAndSorterScope.Creatures,
				Group = typeGroup
			});
			registry.AddFilter(typeGroup, new Filter($"{typeGroup}_Cattle") {
				Icon = ObjectID.Cow,
				Function = PugDatabase.HasComponent<CattleCD>,
				Scope = FilterAndSorterScope.Creatures | FilterAndSorterScope.Checklist,
				Group = typeGroup
			});
			registry.AddFilter(typeGroup, new Filter($"{typeGroup}_Critter") {
				Icon = ObjectID.ButterflySunset,
				Function = PugDatabase.HasComponent<CritterCD>,
				Scope = FilterAndSorterScope.Creatures,
				Group = typeGroup
			});
		}
		
		private static void AddFilters_Utility(ItemBrowserRegistry registry) {
			// Utility
			const string utilityGroup = "ItemBrowser-Filters/Utility";
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Placeable") {
				Icon = ObjectID.WoodTable,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PlaceablePrefab
					       && PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var properties)
					       && properties.Has(PropertyID.PlaceableObject.placeableObject);
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Consumable") {
				Icon = ObjectID.HeartberrySoda,
				Function = objectData => PugDatabase.HasComponent<GivesConditionsWhenConsumedBuffer>(objectData)
				                         || (PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItem) && castItem.useType != CastItemUseType.LeashCattle),
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_CookingIngredient") {
				Icon = ObjectID.HeartBerry,
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CookingIngredient);
				},
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Paintable") {
				Icon = ObjectID.PaintBrushTeal,
				Function = PugDatabase.HasComponent<PaintableObjectCD>,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Craftable") {
				Icon = ObjectID.CopperWorkbench,
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo == null || !ItemBrowserAPI.ObjectEntryRegistry.GetEntries<Crafting>(ObjectEntryType.Source, objectData).Any())
						return false;

					return ObjectUtility.HasMaterialsInInventoryAndNearbyChestsToCraft(Manager.main.player, objectInfo);
				},
				FunctionIsDynamic = true,
				CausesItemCraftingRequirementsToDisplay = true,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Discovered") {
				Icon = ObjectID.CartographyTable,
				Function = DiscoveredTracker.HasBeenDiscovered,
				FunctionIsDynamic = true,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});
			/*registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Collected") {
				Icon = ObjectID.CritterCatcher,
				Function = objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Collected),
				FunctionIsDynamic = true,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist
			});*/
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_ExcludedFromChecklist") {
				Icon = ObjectID.WallExplosiveBlock,
				Function = objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.ExcludeFromChecklist),
				FunctionIsDynamic = true,
				Scope = FilterAndSorterScope.Checklist
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_NonObtainable") {
				Icon = ObjectID.WallObsidianBlock,
				Function = ObjectUtility.IsNonObtainable,
				Scope = FilterAndSorterScope.Items
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_Technical") {
				Icon = ObjectID.MechanicalPart,
				Function = ItemBrowserAPI.IsTechnicalObject,
				DefaultState = () => FilterState.Exclude,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Creatures
			});
			registry.AddFilter(utilityGroup, new Filter($"{utilityGroup}_NoSources") {
				Icon = ObjectID.JingleJamCookie,
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => ItemBrowserSlot.CanCheatInObjects ? FilterState.None : FilterState.Exclude,
				Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Creatures
			});
		}
		
		private static readonly HashSet<FactionID> UnusedCreatureFactions = new() {
			FactionID.AttacksAllButNotPlayer,
			FactionID.PlayerMinion,
			FactionID.Explosion,
			FactionID.__MAX_VALUE
		};
		
		private static void AddFilters_Faction(ItemBrowserRegistry registry) {
			// Creature faction
			const string factionGroup = "ItemBrowser-Filters/Faction";
			foreach (var faction in Enum.GetValues(typeof(FactionID)).Cast<FactionID>()) {
				if (UnusedCreatureFactions.Contains(faction))
					continue;
				
				registry.AddFilter(factionGroup, new Filter($"ItemBrowser-FactionNames/{faction}", $"{factionGroup}_FactionDesc") {
					Symbol = faction.ToString()[..2],
					DescriptionFormatFields = new[] {
						$"ItemBrowser-FactionNames/{faction}"
					},
					Function = objectData => PugDatabase.TryGetComponent<FactionCD>(objectData, out var factionCD) && factionCD.faction == faction,
					Scope = FilterAndSorterScope.Creatures,
					Group = factionGroup
				});
			}
		}
		
		private static void AddFilters_Rarity(ItemBrowserRegistry registry) {
			// Item rarity
			const string rarityGroup = "ItemBrowser-Filters/Rarity";
			foreach (var rarity in Enum.GetValues(typeof(Rarity)).Cast<Rarity>()) {
				registry.AddFilter(rarityGroup, new Filter($"{rarityGroup}_{rarity}") {
					Symbol = rarity.ToString()[..2],
					Function = objectData => PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).rarity == rarity,
					Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
					Group = rarityGroup
				});
			}
		}
		
		private static void AddFilters_Level(ItemBrowserRegistry registry) {
			// Item level
			const string levelGroup = "ItemBrowser-Filters/Level";
			for (var i = 1; i <= LevelScaling.GetMaxLevel(); i++) {
				var level = i;
				registry.AddFilter(levelGroup, new Filter($"{levelGroup}_Level") {
					Symbol = level.ToString(),
					NameFormatFields = new[] { level.ToString() },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { level.ToString() },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => ObjectUtility.GetBaseLevel(objectData) == level,
					Scope = FilterAndSorterScope.Items | FilterAndSorterScope.Checklist,
					Group = levelGroup
				});
			}
		}

		private static void AddFilters_VersionAdded(ItemBrowserRegistry registry) {
			// Version added
			const string versionGroup = "ItemBrowser-Filters/VersionAdded";
			foreach (var version in ObjectsAddedByVersion.AllVersions) {
				var filterScope = FilterAndSorterScope.None;
				if (version.HasAnyIndexedItems)
					filterScope |= FilterAndSorterScope.Items;
				if (version.HasAnyIndexedCreatures)
					filterScope |= FilterAndSorterScope.Creatures;
				if (version.Objects.Any(id => IsChecklistObject(new ObjectDataCD { objectID = id })))
					filterScope |= FilterAndSorterScope.Checklist;

				registry.AddFilter(versionGroup, new Filter($"{versionGroup}_Version") {
					Icon = version.Icon,
					NameFormatFields = new[] { version.Name },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { version.Name },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => version.Objects.Contains(objectData.objectID),
					Scope = filterScope,
					Group = versionGroup
				});
			}
		}

		private static bool IsItemIndexed(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
			if (objectInfo == null)
				return false;

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;
			
			if (objectInfo.objectType is ObjectType.Creature or ObjectType.Critter && !PugDatabase.HasComponent<PetCD>(objectData))
				return false;
			
			if (!ObjectUtility.IsPrimaryVariation(objectData))
				return false;
			
			return !objectInfo.isCustomScenePrefab;
		}
		
		private static bool IsCreatureIndexed(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
			if (objectInfo == null)
				return false;

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;

			if ((objectInfo.objectType != ObjectType.Creature && objectInfo.objectType != ObjectType.Critter) || PugDatabase.HasComponent<PetCD>(objectData))
				return false;

			if (!ObjectUtility.IsPrimaryVariation(objectData))
				return false;

			return !objectInfo.isCustomScenePrefab;
		}

		private static bool IsChecklistObject(ObjectDataCD objectData) {
			if (PugDatabase.HasComponent<CattleCD>(objectData))
				return true;
			
			if (!IsItemIndexed(objectData))
				return false;

			if (ObjectUtility.IsNonObtainable(objectData) || ObjectUtility.GetLocalizedDisplayName(objectData) == null || PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation)?.icon == null)
				return false;

			if (!ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any())
				return false;

			return true;
		}

		private static bool IsTechnicalObject(ObjectDataCD objectData) {
			return ObjectUtility.GetLocalizedDisplayName(objectData) == null && ObjectUtility.GetUnlocalizedDisplayNameNote(objectData) == null;
		}
		
		private static bool IsDeprecatedObject(ObjectDataCD objectData) {
			// HydraWorkbench is listed as deprecated even though it's still craftable
			return ObjectUtility.GetCategories(objectData).Contains("NonObtainable/Deprecated") && objectData.objectID != ObjectID.HydraWorkbench;
		}
	}
}