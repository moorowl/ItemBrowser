using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using Pug.Properties;
using PugMod;

namespace ItemBrowser.Content.VanillaData {
	public class VanillaPlugin : ItemBrowserPlugin {
		public override string AssociatedMod => Main.InternalName;
		public override bool AutomaticallyRegisterFromAssets => true;

		public override void OnEarlyRegister(ItemBrowserRegistry registry) {
			foreach (var objectData in ObjectUtils.GetAllObjects()) {
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
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData)
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Damage") {
				Function = ObjectUtils.GetDamage
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Armor") {
				Function = ObjectUtils.GetArmor
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Level") {
				Function = ObjectUtils.GetBaseLevel
			});
			registry.AddItemSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Value") {
				Function = objectData => ObjectUtils.GetValue(objectData)
			});
			
			// Creature sorters
			registry.AddCreatureSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Alphabetical") {
				Function = objectData => -ObjectUtils.GetDisplayNameSortOrder(objectData)
			});
			registry.AddCreatureSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/InternalIndex") {
				Function = objectData => (int) objectData.objectID * 10000 + objectData.variation
			});
			registry.AddCreatureSorter(new Sorter<ObjectDataCD>("ItemBrowser-Sorters/Level") {
				Function = ObjectUtils.GetBaseLevel
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
			var modsToCheck = new List<long> {
				ModUtils.UnknownModId
			};
			modsToCheck.AddRange(API.ModLoader.LoadedMods.OrderBy(mod => ModUtils.GetDisplayName(mod.ModId)).Select(mod => mod.ModId));

			foreach (var mod in modsToCheck) {
				var associatedObjects = ModUtils.GetAssociatedObjects(mod);

				var associatedItems = associatedObjects.Where(ItemBrowserAPI.IsItemIndexed).ToHashSet();
				var associatedCreatures = associatedObjects.Where(ItemBrowserAPI.IsCreatureIndexed).ToHashSet();

				if (associatedItems.Count > 0)
					itemsByMod.TryAdd(mod, associatedItems);
				
				if (associatedCreatures.Count > 0)
					creaturesByMod.TryAdd(mod, associatedCreatures);
			}
			
			// General modded content filters
			if (itemsByMod.Count > 0) {
				registry.AddItemFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Item_FromMods") {
					Symbol = "#",
					Function = ModUtils.IsModded
				});	
			}
			if (creaturesByMod.Count > 0) {
				registry.AddCreatureFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Creature_FromMods") {
					Symbol = "#",
					Function = ModUtils.IsModded
				});
			}

			// Specific mod filters
			foreach (var (mod, associatedItems) in itemsByMod) {
				var displayName = ModUtils.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtils.UnknownModId;

				registry.AddItemFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Item_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
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
				var displayName = ModUtils.GetDisplayName(mod);
				var isUnknownMod = mod == ModUtils.UnknownModId;

				registry.AddCreatureFilter(sourceGroup, new Filter<ObjectDataCD>($"{sourceGroup}_Item_" + (isUnknownMod ? "FromUnknownMod" : "FromMod")) {
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
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_AnyDamage") {
				Symbol = "#",
				Function = objectData => ObjectUtils.GetDamage(objectData.objectID, objectData.variation) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_PhysicalMeleeDamage") {
				Icon = ObjectID.RustyDagger,
				Function = objectData => ObjectUtils.GetDamage(objectData, ObjectUtils.DamageCategory.PhysicalMelee) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_PhysicalRangeDamage") {
				Icon = ObjectID.Slingshot,
				Function = objectData => ObjectUtils.GetDamage(objectData, ObjectUtils.DamageCategory.PhysicalRange) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_MagicDamage") {
				Icon = ObjectID.BasicStaff,
				Function = objectData => ObjectUtils.GetDamage(objectData, ObjectUtils.DamageCategory.Magic) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_SummonDamage") {
				Icon = ObjectID.TomeOfRange,
				Function = objectData => ObjectUtils.GetDamage(objectData, ObjectUtils.DamageCategory.Summon) > 0,
				Group = damageGroup
			});
			registry.AddItemFilter(damageGroup, new Filter<ObjectDataCD>($"{damageGroup}_ExplosiveDamage") {
				Icon = ObjectID.Bomb,
				Function = objectData => ObjectUtils.GetDamage(objectData, ObjectUtils.DamageCategory.Explosive) > 0,
				Group = damageGroup
			});
		}
		
		private static void AddFilters_Equipment(ItemBrowserRegistry registry) {
			// Item equipment
			const string equipmentGroup = "ItemBrowser-Filters/Equipment";
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Weapon") {
				Icon = ObjectID.TinDagger,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (PugDatabase.HasComponent<HasWeaponDamageCD>(objectData) && !ObjectUtils.MiningToolObjectTypes.Contains(objectType) && !(PugDatabase.TryGetComponent<BeamWeaponCD>(objectData, out var beamWeaponCD) && beamWeaponCD.isStickyBeam))
						return true;

					return PugDatabase.TryGetComponent<SecondaryUseCD>(objectData, out var secondaryUse) && secondaryUse.summonsMinion;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Tool") {
				Icon = ObjectID.Bucket,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.ToolObjectTypes.Contains(objectType) || (PugDatabase.TryGetComponent<BeamWeaponCD>(objectData, out var beamWeaponCD) && beamWeaponCD.isStickyBeam);
				}
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Armor") {
				Icon = ObjectID.IronShield,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.ArmorObjectTypes.Contains(objectType);
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Helm") {
				Icon = ObjectID.IronHelm,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Helm;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_BreastArmor") {
				Icon = ObjectID.IronBreastArmor,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.BreastArmor;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_PantsArmor") {
				Icon = ObjectID.IronPantsArmor,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PantsArmor;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Accessory") {
				Icon = ObjectID.HeartBerryNecklace,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return ObjectUtils.AccessoryObjectTypes.Contains(objectType);
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Ring") {
				Icon = ObjectID.CavelingMothersRing,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Ring;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Necklace") {
				Icon = ObjectID.GoldCrystalNecklace,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Necklace;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_OffHand") {
				Icon = ObjectID.OracleDeck,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Offhand;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Bag") {
				Icon = ObjectID.ExplorerBackpack,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Bag;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Pouch") {
				Icon = ObjectID.ValuablePouch,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Pouch;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Lantern") {
				Icon = ObjectID.Lantern,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.Lantern;
				},
				Group = equipmentGroup
			});
			registry.AddItemFilter(equipmentGroup, new Filter<ObjectDataCD>($"{equipmentGroup}_Pet") {
				Icon = ObjectID.PetCat,
				Function = PugDatabase.HasComponent<PetCD>,
				Group = equipmentGroup
			});
		}
		
		private static void AddFilters_Type(ItemBrowserRegistry registry) {
			// Creature type
			const string typeGroup = "ItemBrowser-Filters/Type";
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Hostile") {
				Icon = ObjectID.AggressiveSlimeBlob,
				Function = objectData => !ObjectCategoryTagsCD.HasTag(PugDatabase.GetComponent<ObjectCategoryTagsCD>(objectData).tagsBitMask, ObjectCategoryTag.NonHostileCreature)
				                         && !PugDatabase.HasComponent<CattleCD>(objectData)
				                         && !PugDatabase.HasComponent<CritterCD>(objectData)
				                         && !PugDatabase.HasComponent<MerchantCD>(objectData),
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Boss") {
				Icon = ObjectID.SlimeBossCrystal,
				Function = objectData => PugDatabase.HasComponent<BossCD>(objectData) || ObjectUtils.GetCategories(objectData).Contains("Boss/BossCreature"),
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Merchant") {
				Icon = ObjectID.SlimeMerchant,
				Function = PugDatabase.HasComponent<MerchantCD>,
				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Cattle") {
				Icon = ObjectID.Cow,
				Function = PugDatabase.HasComponent<CattleCD>,
                                                             				Group = typeGroup
			});
			registry.AddCreatureFilter(typeGroup, new Filter<ObjectDataCD>($"{typeGroup}_Critter") {
				Icon = ObjectID.ButterflySunset,
				Function = PugDatabase.HasComponent<CritterCD>,
				Group = typeGroup
			});
		}
		
		private static void AddFilters_Utility(ItemBrowserRegistry registry) {
			// Utility
			const string utilityGroup = "ItemBrowser-Filters/Utility";
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Placeable") {
				Icon = ObjectID.WoodTable,
				Function = objectData => {
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					return objectType == ObjectType.PlaceablePrefab
					       && PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var properties)
					       && properties.Has(PropertyID.PlaceableObject.placeableObject);
				}
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Consumable") {
				Icon = ObjectID.HeartberrySoda,
				Function = objectData => PugDatabase.HasComponent<GivesConditionsWhenConsumedBuffer>(objectData)
				                         || (PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItem) && castItem.useType != CastItemUseType.LeashCattle)
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Craftable") {
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
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_CookingIngredient") {
				Icon = ObjectID.HeartBerry,
				Function = objectData => {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					return objectInfo.tags.Contains(ObjectCategoryTag.CookingIngredient);
				}
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Paintable") {
				Icon = ObjectID.PaintBrushTeal,
				Function = PugDatabase.HasComponent<PaintableObjectCD>
			});
			registry.AddItemFilter(utilityGroup, new($"{utilityGroup}_Discovered") {
				Icon = ObjectID.Portal,
				Function = objectData => ObjectUtils.HasBeenDiscovered(objectData),
				FunctionIsDynamic = true
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Technical_Item") {
				Icon = ObjectID.MechanicalPart,
				Function = ItemBrowserAPI.IsTechnicalObject,
				DefaultState = () => FilterState.Exclude
			});
			registry.AddCreatureFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_Technical_Creature") {
				Icon = ObjectID.MechanicalPart,
				Function = ItemBrowserAPI.IsTechnicalObject,
				DefaultState = () => FilterState.Exclude
			});
			registry.AddItemFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_NoSources_Item") {
				Icon = ObjectID.JingleJamCookie,
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => Options.Instance.CheatMode ? FilterState.None : FilterState.Exclude
			});
			registry.AddCreatureFilter(utilityGroup, new Filter<ObjectDataCD>($"{utilityGroup}_NoSources_Creature") {
				Icon = ObjectID.JingleJamCookie,
				Function = objectData => !ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, objectData).Any(),
				DefaultState = () => Options.Instance.CheatMode ? FilterState.None : FilterState.Exclude
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
				
				registry.AddCreatureFilter(factionGroup, new Filter<ObjectDataCD>($"ItemBrowser-FactionNames/{faction}", $"{factionGroup}_FactionDesc") {
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
				registry.AddItemFilter(rarityGroup, new Filter<ObjectDataCD>($"{rarityGroup}_{rarity}") {
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
				registry.AddItemFilter(levelGroup, new Filter<ObjectDataCD>($"{levelGroup}_Level") {
					Symbol = i.ToString(),
					NameFormatFields = new[] { i.ToString() },
					LocalizeNameFormatFields = false,
					DescriptionFormatFields = new[] { i.ToString() },
					LocalizeDescriptionFormatFields = false,
					Function = objectData => ObjectUtils.GetBaseLevel(objectData) == level,
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
				if (!ObjectUtils.GetAllObjects().Where(ItemBrowserAPI.IsItemIndexed).Any(objectData => ObjectUtils.GetAssociatedConditionCategories(objectData).Contains(conditionCategory.category)))
					continue;
				
				registry.AddItemFilter(effectGroup, new Filter<ObjectDataCD>($"ItemBrowser-ConditionCategoryNames/{conditionCategory.category}", $"{effectGroup}_EffectDesc") {
					Icon = EffectIcons.GetValueOrDefault(conditionCategory.category),
					DescriptionFormatFields = new[] { $"ItemBrowser-ConditionCategoryNames/{conditionCategory.category}" },
					Function = objectData => ObjectUtils.GetAssociatedConditionCategories(objectData).Contains(conditionCategory.category),
					Group = effectGroup
				});
			}
		}
		
		private static void AddFilters_VersionAdded(ItemBrowserRegistry registry) {
			// Version added
			const string versionGroup = "ItemBrowser-Filters/VersionAdded";
			foreach (var version in ObjectsAddedByVersion.AllVersions) {
				if (version.HasAnyIndexedItems) {
					registry.AddItemFilter(versionGroup, new Filter<ObjectDataCD>($"{versionGroup}_Item_Version") {
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
					registry.AddCreatureFilter(versionGroup, new Filter<ObjectDataCD>($"{versionGroup}_Creature_Version") {
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
			
			if (!ObjectUtils.IsPrimaryVariation(objectData))
				return false;
			
			return !objectInfo.isCustomScenePrefab;
		}
		
		private static bool IsCreatureIndexed(ObjectDataCD objectData) {
			var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);

			if (PugDatabase.HasComponent<CookedFoodCD>(objectData))
				return false;

			if ((objectInfo.objectType != ObjectType.Creature && objectInfo.objectType != ObjectType.Critter) || PugDatabase.HasComponent<PetCD>(objectData))
				return false;

			if (!ObjectUtils.IsPrimaryVariation(objectData))
				return false;

			return !objectInfo.isCustomScenePrefab;
		}

		private static bool IsTechnicalObject(ObjectDataCD objectData) {
			return ObjectUtils.GetLocalizedDisplayName(objectData) == null && ObjectUtils.GetUnlocalizedDisplayNameNote(objectData) == null;
		}
		
		private static bool IsDeprecatedObject(ObjectDataCD objectData) {
			// HydraWorkbench is listed as deprecated even though it's still craftable
			return ObjectUtils.GetCategories(objectData).Contains("NonObtainable/Deprecated") && objectData.objectID != ObjectID.HydraWorkbench;
		}
	}
}