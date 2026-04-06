using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using Pug.Properties;
using PugMod;

namespace ItemBrowser.Content.VanillaData {
	public class VanillaPlugin : ItemBrowserPlugin {
		public override bool AutomaticallyRegisterFromAssets => true;

		public override void OnEarlyRegister(ItemBrowserRegistry registry) {
			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				var isIndexedItem = IsItemIndexed(objectData);
				var isIndexedCreature = IsCreatureIndexed(objectData);
				
				if (isIndexedItem)
					registry.AddItem(objectData);

				if (isIndexedCreature)
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
			AddGroups(registry);
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
			// Item sorters
			registry.AddItemSorter(new Sorter("ItemBrowser-Sorters/Alphabetical") {
				Function = allObjectData => allObjectData.OrderBy(objectData => {
					var localizedDisplayName = ObjectUtility.GetLocalizedDisplayName(objectData);
					return localizedDisplayName ?? $"ZZZ+{ObjectUtility.GetInternalName(objectData)}:{objectData.variation}";
				})
			});
			registry.AddItemSorter(new Sorter("ItemBrowser-Sorters/InternalIndex") {
				Function = allObjectData => allObjectData.OrderBy(objectData => (int) objectData.objectID * 10000 + objectData.variation)
			});
			registry.AddItemSorter(new Sorter("ItemBrowser-Sorters/Damage") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetDamage)
			});
			registry.AddItemSorter(new Sorter("ItemBrowser-Sorters/Armor") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetArmor)
			});
			registry.AddItemSorter(new Sorter("ItemBrowser-Sorters/Level") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetBaseLevel)
			});
			registry.AddItemSorter(new Sorter("ItemBrowser-Sorters/Value") {
				Function = allObjectData => allObjectData.OrderBy(objectData => ObjectUtility.GetValue(objectData))
			});
			
			// Creature sorters
			registry.AddCreatureSorter(new Sorter("ItemBrowser-Sorters/Alphabetical") {
				Function = allObjectData => allObjectData.OrderBy(objectData => {
					var localizedDisplayName = ObjectUtility.GetLocalizedDisplayName(objectData);
					return localizedDisplayName ?? $"ZZZ+{ObjectUtility.GetInternalName(objectData)}:{objectData.variation}";
				})
			});
			registry.AddCreatureSorter(new Sorter("ItemBrowser-Sorters/InternalIndex") {
				Function = allObjectData => allObjectData.OrderBy(objectData => (int) objectData.objectID * 10000 + objectData.variation)
			});
			registry.AddCreatureSorter(new Sorter("ItemBrowser-Sorters/Level") {
				Function = allObjectData => allObjectData.OrderBy(ObjectUtility.GetBaseLevel)
			});
		}
		
		private static void AddGroups(ItemBrowserRegistry registry) {
			// Item groups
			registry.AddItemGroup(new Group("ItemBrowser-Groups/Mod") {
				Function = objectData => {
					var associatedModId = ModUtility.GetAssociatedMod(objectData);
					if (associatedModId == ModUtility.UnknownModId)
						return ("ItemBrowser-Groups/Mod_Unknown", -1, true);
					
					return (ModUtility.GetDisplayName(associatedModId), 0, false);
				}
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
			AddFilters_Effect(registry);
			AddFilters_VersionAdded(registry);
		}

		private static void AddFilters_Source(ItemBrowserRegistry registry) {
			// Source
			const string sourceGroup = "ItemBrowser-Filters/Source";
			const string unknownModSymbol = "?";

			var itemsByMod = new Dictionary<long, HashSet<ObjectDataCD>>();
			var creaturesByMod = new Dictionary<long, HashSet<ObjectDataCD>>();
			
			// Setup Mod Name -> Associated items/creatures
			var modsToCheck = API.ModLoader.LoadedMods.OrderBy(mod => ModUtility.GetDisplayName(mod.ModId)).Select(mod => mod.ModId).ToList();
			modsToCheck.Add(ModUtility.UnknownModId);

			foreach (var mod in modsToCheck) {
				var associatedObjects = ModUtility.GetAssociatedObjects(mod);

				var associatedItems = associatedObjects.Where(ItemBrowserAPI.IsItemIndexed).ToHashSet();
				var associatedCreatures = associatedObjects.Where(ItemBrowserAPI.IsCreatureIndexed).ToHashSet();

				if (associatedItems.Count > 0)
					itemsByMod.TryAdd(mod, associatedItems);
				
				if (associatedCreatures.Count > 0)
					creaturesByMod.TryAdd(mod, associatedCreatures);
			}
			
			// General modded content filters
			if (itemsByMod.Count > 0) {
				registry.AddItemFilter(sourceGroup, new Filter($"{sourceGroup}_Item_FromMods") {
					Symbol = "#",
					Function = ModUtility.IsModded
				});	
			}
			if (creaturesByMod.Count > 0) {
				registry.AddCreatureFilter(sourceGroup, new Filter($"{sourceGroup}_Creature_FromMods") {
					Symbol = "#",
					Function = ModUtility.IsModded
				});
			}

			// Specific mod filters
			foreach (var (mod, associatedItems) in itemsByMod) {
				var displayName = ModUtility.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtility.UnknownModId;

				registry.AddItemFilter(sourceGroup, new Filter($"{sourceGroup}_Item_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
					Symbol = isUnknownMod ? unknownModSymbol : displayName[..Math.Min(displayName.Length, 2)],
					NameFormatFields = new[] { displayName },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { displayName },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => associatedItems.Contains(objectData),
					Group = sourceGroup
				});
			}
			foreach (var (mod, associatedCreatures) in creaturesByMod) {
				var displayName = ModUtility.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtility.UnknownModId;

				registry.AddCreatureFilter(sourceGroup, new Filter($"{sourceGroup}_Item_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
					Symbol = isUnknownMod ? unknownModSymbol : displayName[..Math.Min(displayName.Length, 2)],
					NameFormatFields = new[] { displayName },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { displayName },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => associatedCreatures.Contains(objectData),
					Group = sourceGroup
				});
			}
		}

		private static void AddFilters_Damage(ItemBrowserRegistry registry) {
			// Item damage
			const string damageGroup = "ItemBrowser-Filters/Damage";
			registry.AddItemFilter(damageGroup, new Filter($"{damageGroup}_AnyDamage") {
				Symbol = "#",
				Function = objectData => ObjectUtility.GetDamage(objectData.objectID, objectData.variation) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter($"{damageGroup}_PhysicalMeleeDamage") {
				Icon = ObjectID.RustyDagger,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.PhysicalMelee) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter($"{damageGroup}_PhysicalRangeDamage") {
				Icon = ObjectID.Slingshot,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.PhysicalRange) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter($"{damageGroup}_MagicDamage") {
				Icon = ObjectID.BasicStaff,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.Magic) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter($"{damageGroup}_SummonDamage") {
				Icon = ObjectID.TomeOfRange,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.Summon) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter($"{damageGroup}_ExplosiveDamage") {
				Icon = ObjectID.Bomb,
				Function = objectData => ObjectUtility.GetDamage(objectData, ObjectUtility.DamageCategory.Explosive) > 0,
				Group = damageGroup
			});
		}
		
		private static void AddFilters_Equipment(ItemBrowserRegistry registry) {
			// Item equipment
			const string equipmentGroup = "ItemBrowser-Filters/Equipment";
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Weapon") {
				Icon = ObjectID.TinDagger,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (PugDatabase.HasComponent<HasWeaponDamageCD>(objectData) && !ObjectUtility.MiningToolObjectTypes.Contains(objectType) && !(PugDatabase.TryGetComponent<BeamWeaponCD>(objectData, out var beamWeaponCD) && beamWeaponCD.isStickyBeam))
						return true;

					return PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Tool") {
				Icon = ObjectID.Bucket,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtility.ToolObjectTypes.Contains(objectType) || (PugDatabase.TryGetComponent<BeamWeaponCD>(objectData, out var beamWeaponCD) && beamWeaponCD.isStickyBeam);
				}
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Armor") {
				Icon = ObjectID.IronShield,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtility.ArmorObjectTypes.Contains(objectType);
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Helm") {
				Icon = ObjectID.IronHelm,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Helm;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_BreastArmor") {
				Icon = ObjectID.IronBreastArmor,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.BreastArmor;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_PantsArmor") {
				Icon = ObjectID.IronPantsArmor,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PantsArmor;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Accessory") {
				Icon = ObjectID.HeartBerryNecklace,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtility.AccessoryObjectTypes.Contains(objectType);
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Ring") {
				Icon = ObjectID.CavelingMothersRing,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Ring;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Necklace") {
				Icon = ObjectID.GoldCrystalNecklace,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Necklace;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_OffHand") {
				Icon = ObjectID.OracleDeck,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Offhand;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Bag") {
				Icon = ObjectID.ExplorerBackpack,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Bag;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Pouch") {
				Icon = ObjectID.ValuablePouch,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Pouch;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Lantern") {
				Icon = ObjectID.Lantern,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Lantern;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter($"{equipmentGroup}_Pet") {
				Icon = ObjectID.PetCat,
				Function = PugDatabase.HasComponent<PetCD>,
				Group = equipmentGroup
			});
		}
		
		private static void AddFilters_Type(ItemBrowserRegistry registry) {
			// Creature type
			const string typeGroup = "ItemBrowser-Filters/Type";
			registry.AddCreatureFilter(typeGroup, new Filter($"{typeGroup}_Hostile") {
				Icon = ObjectID.AggressiveSlimeBlob,
				Function = objectData => !ObjectCategoryTagsCD.HasTag(PugDatabase.GetComponent<ObjectCategoryTagsCD>(objectData).tagsBitMask, ObjectCategoryTag.NonHostileCreature)
				                         && !PugDatabase.HasComponent<CattleCD>(objectData)
				                         && !PugDatabase.HasComponent<CritterCD>(objectData)
				                         && !PugDatabase.HasComponent<MerchantCD>(objectData),
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter($"{typeGroup}_Boss") {
				Icon = ObjectID.SlimeBossCrystal,
				Function = objectData => PugDatabase.HasComponent<BossCD>(objectData) || ObjectUtility.GetCategories(objectData).Contains("Boss/BossCreature"),
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter($"{typeGroup}_Merchant") {
				Icon = ObjectID.SlimeMerchant,
				Function = PugDatabase.HasComponent<MerchantCD>,
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter($"{typeGroup}_Cattle") {
				Icon = ObjectID.Cow,
				Function = PugDatabase.HasComponent<CattleCD>,
                                                             				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter($"{typeGroup}_Critter") {
				Icon = ObjectID.ButterflySunset,
				Function = PugDatabase.HasComponent<CritterCD>,
				Group = typeGroup
			});
		}
		
		private static void AddFilters_Utility(ItemBrowserRegistry registry) {
			// Utility
			const string utilityGroup = "ItemBrowser-Filters/Utility";
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_Placeable") {
				Icon = ObjectID.WoodTable,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PlaceablePrefab
					       && PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var properties)
					       && properties.Has(PropertyID.PlaceableObject.placeableObject);
				}
			});
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_Consumable") {
				Icon = ObjectID.HeartberrySoda,
				Function = objectData => PugDatabase.HasComponent<GivesConditionsWhenConsumedBuffer>(objectData)
				                         || (PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItem) && castItem.useType != CastItemUseType.LeashCattle)
			});
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_Craftable") {
				Icon = ObjectID.CopperWorkbench,
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
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_CookingIngredient") {
				Icon = ObjectID.HeartBerry,
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CookingIngredient);
				}
			});
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_Paintable") {
				Icon = ObjectID.PaintBrushTeal,
				Function = PugDatabase.HasComponent<PaintableObjectCD>
			});
			registry.AddItemFilter(utilityGroup, new($"{utilityGroup}_Discovered") {
				Icon = ObjectID.Portal,
				Function = objectData => ObjectUtility.HasBeenDiscovered(objectData),
				FunctionIsDynamic = true
			});
			registry.AddItemFilter(utilityGroup, new($"{utilityGroup}_NonObtainable") {
				Icon = ObjectID.WallObsidianBlock,
				Function = ObjectUtility.IsNonObtainable,
				FunctionIsDynamic = true
			});
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_Technical_Item") {
				Icon = ObjectID.MechanicalPart,
				Function = ItemBrowserAPI.IsTechnicalObject,
				DefaultState = () => FilterState.Exclude
			});
			registry.AddCreatureFilter(utilityGroup, new Filter($"{utilityGroup}_Technical_Creature") {
				Icon = ObjectID.MechanicalPart,
				Function = ItemBrowserAPI.IsTechnicalObject,
				DefaultState = () => FilterState.Exclude
			});
			registry.AddItemFilter(utilityGroup, new Filter($"{utilityGroup}_NoSources_Item") {
				Icon = ObjectID.JingleJamCookie,
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => OptionsManager.Instance.CheatMode ? FilterState.None : FilterState.Exclude
			});
			registry.AddCreatureFilter(utilityGroup, new Filter($"{utilityGroup}_NoSources_Creature") {
				Icon = ObjectID.JingleJamCookie,
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => OptionsManager.Instance.CheatMode ? FilterState.None : FilterState.Exclude
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
			const string factionGroup = "ItemBrowser-Filters/Faction";
			foreach (var faction in Enum.GetValues(typeof(FactionID)).Cast<FactionID>()) {
				if (UnusedCreatureFactions.Contains(faction))
					continue;
				
				registry.AddCreatureFilter(factionGroup, new Filter($"ItemBrowser-FactionNames/{faction}", $"{factionGroup}_FactionDesc") {
					Symbol = faction.ToString()[..2],
					DescriptionFormatFields = new[] {
						$"ItemBrowser-FactionNames/{faction}"
					},
					Function = objectData => PugDatabase.TryGetComponent<FactionCD>(objectData, out var factionCD) && factionCD.faction == faction,
					Group = factionGroup
				});
			}
		}
		
		private static void AddFilters_Rarity(ItemBrowserRegistry registry) {
			// Item rarity
			const string rarityGroup = "ItemBrowser-Filters/Rarity";
			foreach (var rarity in Enum.GetValues(typeof(Rarity)).Cast<Rarity>()) {
				registry.AddItemFilter(rarityGroup, new Filter($"{rarityGroup}_{rarity}") {
					Symbol = rarity.ToString()[..2],
					Function = objectData => PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).rarity == rarity,
					Group = rarityGroup
				});
			}
		}
		
		private static void AddFilters_Level(ItemBrowserRegistry registry) {
			// Item level
			const string levelGroup = "ItemBrowser-Filters/Level";
			for (var i = 1; i <= LevelScaling.GetMaxLevel(); i++) {
				var level = i;
				registry.AddItemFilter(levelGroup, new Filter($"{levelGroup}_Level") {
					Symbol = i.ToString(),
					NameFormatFields = new[] { i.ToString() },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { i.ToString() },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => ObjectUtility.GetBaseLevel(objectData) == level,
					Group = levelGroup
				});
			}
		}
		
		private static readonly Dictionary<string, ObjectID> EffectIcons = new() {
			{ "Appearance", ObjectID.MagicMirror },
			{ "Armor", ObjectID.IronShield },
			{ "Attack Speed", ObjectID.SwiftRing },
			{ "Burn", ObjectID.FlameRing },
			{ "Charm", ObjectID.ValentineNecklace },
			{ "Cooking", ObjectID.CookingPot },
			{ "Crafting", ObjectID.WoodenWorkBench },
			{ "Crit", ObjectID.GoldenSpikeRing },
			{ "Crit Damage", ObjectID.GoldenSpikeRing },
			{ "Damage", ObjectID.TinDagger },
			{ "Damage Taken", ObjectID.TurtleShell },
			{ "Damage vs Bosses", ObjectID.GolemHelm },
			{ "Deal Damage", ObjectID.RadiationCrystal },
			{ "Dodge", ObjectID.NinjaHelm },
			{ "Durability", ObjectID.SalvageAndRepairStation },
			{ "Explosives", ObjectID.BlastHelm },
			{ "Faction Change", ObjectID.RoyalGel },
			{ "Fishing", ObjectID.BaitIncreasedChanceToGetFish },
			{ "Gardening", ObjectID.FarmerHelm },
			{ "Glow", ObjectID.GlowingTulipFlower },
			{ "Heal", ObjectID.HealingPotion },
			{ "Hunger", ObjectID.Mushroom },
			{ "Immunity", ObjectID.RemedaisyNecklace },
			{ "Magic", ObjectID.MagicPotion },
			{ "Mana", ObjectID.ManaPotion },
			{ "Max Health", ObjectID.TrinityHeartOrbOffhand },
			{ "Mining", ObjectID.DrillTool },
			{ "Minions", ObjectID.MinionPotion },
			{ "Movement", ObjectID.SwiftFeather },
			{ "Oil", ObjectID.GroundOilSlime },
			{ "Pet", ObjectID.PetRock },
			{ "Poison", ObjectID.PoisonGrenade },
			{ "Sleep", ObjectID.SleepyHelm },
			{ "Spawn Object", ObjectID.GodsentHelm },
			{ "Stun and Snare", ObjectID.StunGrenade },
			{ "Thorns", ObjectID.BlackSteelUrchin },
			{ "Unique", ObjectID.LiquidMetal },
			{ "Void", ObjectID.VoidFusedHelm }
		};
		
		private static void AddFilters_Effect(ItemBrowserRegistry registry) {
			// Item level
			const string effectGroup = "ItemBrowser-Filters/Effect";

			foreach (var conditionCategory in Manager.ui.conditionsIconsTable.conditionCategories) {
				if (!ObjectUtility.GetAllObjects().Where(ItemBrowserAPI.IsItemIndexed).Any(objectData => ObjectUtility.GetAssociatedConditionCategories(objectData).Contains(conditionCategory.category)))
					continue;
				
				registry.AddItemFilter(effectGroup, new Filter($"ItemBrowser-ConditionCategoryNames/{conditionCategory.category}", $"{effectGroup}_EffectDesc") {
					Icon = EffectIcons.GetValueOrDefault(conditionCategory.category),
					DescriptionFormatFields = new[] { $"ItemBrowser-ConditionCategoryNames/{conditionCategory.category}" },
					Function = objectData => ObjectUtility.GetAssociatedConditionCategories(objectData).Contains(conditionCategory.category),
					Group = effectGroup
				});
			}
		}
		
		private static void AddFilters_VersionAdded(ItemBrowserRegistry registry) {
			// Version added
			const string versionGroup = "ItemBrowser-Filters/VersionAdded";
			foreach (var version in ObjectsAddedByVersion.AllVersions) {
				if (version.HasAnyIndexedItems) {
					registry.AddItemFilter(versionGroup, new Filter($"{versionGroup}_Item_Version") {
						Icon = version.Icon,
						NameFormatFields = new[] { version.Name },
						LocalizeNameFormatFields = false,
						DescriptionFormatFields = new[] { version.Name },
						LocalizeDescriptionFormatFields = false,
						Function = objectData => version.Objects.Contains(objectData.objectID),
						Group = versionGroup
					});	
				}
				if (version.HasAnyIndexedCreatures) {
					registry.AddCreatureFilter(versionGroup, new Filter($"{versionGroup}_Creature_Version") {
						Icon = version.Icon,
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
			
			if (!ObjectUtility.IsPrimaryVariation(objectData))
				return false;
			
			return !objectInfo.isCustomScenePrefab;
		}
		
		private static bool IsCreatureIndexed(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;

			if ((objectInfo.objectType != ObjectType.Creature && objectInfo.objectType != ObjectType.Critter) || PugDatabase.HasComponent<PetCD>(objectData))
				return false;

			if (!ObjectUtility.IsPrimaryVariation(objectData))
				return false;

			return !objectInfo.isCustomScenePrefab;
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