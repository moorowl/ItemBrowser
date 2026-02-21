using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public abstract class DisplayedObject {
		public virtual ContainedObjectsBuffer ContainedObject => default;
		public virtual ContainedObjectsBuffer VisualObject => ContainedObject;
		public virtual (int Min, int Max) Amount => ContainedObject.objectID == ObjectID.None ? (VisualObject.amount, VisualObject.amount) : (ContainedObject.amount, ContainedObject.amount);
			
		public virtual void Update(SlotUIBase slot) { }
		
		public virtual bool ShowDetails(SlotUIBase slot, DetailsTab initialTab) {
			return false;
		}

		public virtual TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			return null;
		}

		public virtual List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
			return new List<TextAndFormatFields>();
		}

		public virtual List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
			return null;
		}
		
		public class Basic : DisplayedObject {
			public override ContainedObjectsBuffer ContainedObject => new() {
				objectData = _objectsToDisplay.CurrentObjectData
			};
			public override (int Min, int Max) Amount { get; } = (1, 1);
			
			private readonly CyclingObjectData _objectsToDisplay;

			public Basic(ObjectDataCD objectData) {
				_objectsToDisplay = new CyclingObjectData(new ObjectDataCD[] {
					objectData
				});

				Amount = (objectData.amount, objectData.amount);
			}
			
			public Basic(ObjectDataCD objectData, (int Min, int Max) amount) : this(objectData) {
				Amount = amount;
			}
			
			public Basic(ObjectDataCD objectData, int amount) : this(objectData) {
				Amount = (amount, amount);
			}
			
			public Basic(params ObjectDataCD[] objectData) {
				_objectsToDisplay = new CyclingObjectData(objectData);
			}
			
			public override void Update(SlotUIBase slot) {
				_objectsToDisplay.Update(slot);
			}
			
			public override bool ShowDetails(SlotUIBase slot, DetailsTab initialTab) {
				return ItemBrowserAPI.ItemBrowserUI.ShowDetails(_objectsToDisplay.CurrentObjectData, initialTab);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				var objectName = new TextAndFormatFields {
					text = ObjectUtils.GetLocalizedDisplayNameOrDefault(_objectsToDisplay.CurrentObjectData),
					dontLocalize = true
				};

				var objectInfo = PugDatabase.GetObjectInfo(_objectsToDisplay.CurrentObjectData.objectID);
				if (objectInfo != null)
					objectName.color = Manager.text.GetRarityColor(objectInfo.rarity);

				return objectName;
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				var objectData = _objectsToDisplay.CurrentObjectData;
				var term = ObjectUtils.GetInternalName(objectData);

				var nameTermOverride = Manager.ui.itemOverridesTable.GetNameTermOverride(objectData);
				if (nameTermOverride != null)
					term = nameTermOverride;

				var lines = new List<TextAndFormatFields> {
					new() {
						text = $"Items/{term}Desc"
					}
				};
				
				if (InputHelper.IsShowTechnicalInfoHeld) {
					lines.Add(new TextAndFormatFields {
						text = $"{(int) objectData.objectID}:{objectData.variation}",
						dontLocalize = true
					});
					lines.Add(new TextAndFormatFields {
						text = ObjectUtils.GetInternalName(objectData.objectID),
						dontLocalize = true
					});
					if (PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD)) {
						var isBlock = TileUtils.IsBlock(tileCD.tileType, (Tileset) tileCD.tileset);
						lines.Add(new TextAndFormatFields {
							text = isBlock ? $"{(Tileset) tileCD.tileset} ({TileType.wall} / {TileType.ground})" : $"{(Tileset) tileCD.tileset} ({tileCD.tileType})",
							dontLocalize = true
						});
					}
					var prefabInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).prefabInfos[0];
					if (prefabInfo.ecsPrefab != null) {
						lines.Add(new TextAndFormatFields {
							text = $"{prefabInfo.ecsPrefab.gameObject.name}",
							dontLocalize = true
						});
					}
					if (prefabInfo.prefab != null) {
						lines.Add(new TextAndFormatFields {
							text = $"{prefabInfo.prefab.gameObject.name}",
							dontLocalize = true
						});
					}
				}

				if (objectData.objectID != ObjectID.None && Options.Instance.ShowSourceMod) {
					var associatedMod = ModUtils.GetAssociatedMod(objectData);
					lines.Add(new TextAndFormatFields {
						text = ModUtils.GetDisplayName(associatedMod),
						dontLocalize = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}
				
				if (ItemBrowserAPI.IsItemIndexed(objectData) && !ObjectUtils.IsNonObtainable(objectData)) {
					ObjectUtils.GetTotalAmountInInventoryAndNearbyChests(Manager.main.player, objectData, out var inInventory, out var inNearbyChests);
					
					if (inInventory > 0 || inNearbyChests > 0) {
						lines.Add(new TextAndFormatFields {
							text = "ItemBrowser-General/AmountInInventory",
							formatFields = new[] {
								(inInventory + inNearbyChests).ToString()
							},
							dontLocalizeFormatFields = true,
							color = Color.white * 0.95f
						});
					}
				}

				return lines;
			}

			public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
				var lines = slot.GetHoverStats(ContainedObject, previewReinforced, false);

				var displayNameNote = ObjectUtils.GetUnlocalizedDisplayNameNote(_objectsToDisplay.CurrentObjectData);
				if (displayNameNote == null)
					return lines;
				
				lines ??= new List<TextAndFormatFields>();
				lines.Insert(0, new TextAndFormatFields {
					text = displayNameNote,
					color = UserInterfaceUtils.DescriptionColor
				});

				return lines;
			}
		}

		public class Tag : DisplayedObject {
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectsToDisplay.CurrentObjectData
			};

			private readonly ObjectCategoryTag _tag;
			private readonly int _amount;
			private readonly CyclingObjectData _objectsToDisplay;
			
			public Tag(ObjectCategoryTag tag, int amount = 1) {
				_tag = tag;
				_objectsToDisplay = new CyclingObjectData(GetObjectsToDisplay(tag).Select(objectData => new ObjectDataCD {
					objectID = objectData.objectID,
					variation = objectData.variation,
					amount = amount
				}));
			}

			public override void Update(SlotUIBase slot) {
				_objectsToDisplay.Update(slot);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"ItemBrowser-ObjectCategoryNames/{_tag}"
				};
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				var lines = new List<TextAndFormatFields>();
				
				if (InputHelper.IsShowTechnicalInfoHeld) {
					lines.Add(new TextAndFormatFields {
						text = _tag.ToString(),
						dontLocalize = true
					});
				}
				
				ObjectUtils.GetTotalAmountInInventoryAndNearbyChests(Manager.main.player, _tag, out var inInventory, out var inNearbyChests);
				
				if (inInventory > 0 || inNearbyChests > 0) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/AmountInInventory",
						formatFields = new[] {
							(inInventory + inNearbyChests).ToString()
						},
						dontLocalizeFormatFields = true,
						color = Color.white * 0.95f
					});
				}
				
				return lines;
			}

			private static IEnumerable<ObjectDataCD> GetObjectsToDisplay(ObjectCategoryTag tag) {
				var allObjectsWithTag = ObjectUtils.GetAllObjectsWithTag(tag);
				return tag switch {
					ObjectCategoryTag.UncommonOrLowerCookedFood or ObjectCategoryTag.RareOrHigherCookedFood => allObjectsWithTag.Select(objectData => new ObjectDataCD {
						objectID = objectData.objectID,
						variation = CookedFoodCD.GetFoodVariation(ObjectID.HeartBerry, ObjectID.GlowingTulipFlower)
					}),
					ObjectCategoryTag.CattlePlantFood => allObjectsWithTag.Where(objectData => !PugDatabase.HasComponent<PlantCD>(objectData)),
					_ => allObjectsWithTag
				};
			}
		}
		
		public class Tile : DisplayedObject {
			public override ContainedObjectsBuffer ContainedObject => _staticObject?.ContainedObject ?? new ContainedObjectsBuffer();
			public override ContainedObjectsBuffer VisualObject =>  _staticObject?.VisualObject ?? new ContainedObjectsBuffer {
				objectData = _visualObjects?.CurrentObjectData ?? default
			};

			private readonly TileType _tileType;
			private readonly Tileset _tileset;
			private readonly CyclingObjectData _visualObjects;
			private readonly Basic _staticObject;

			public Tile(TileType tileType, Tileset? tileset = null) {
				_tileType = tileType;
				_tileset = tileset ?? Tileset.MAX_VALUE;

				if (tileset == null) {
					_visualObjects = new CyclingObjectData(GetObjectsToDisplay(_tileType).OrderBy(objectData => PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) ? levelCD.level : 0));
				} else {
					var objectInfo = PugDatabase.TryGetTileItemInfo(_tileType == TileType.ground ? TileType.wall : _tileType, (int) tileset);
					if (objectInfo != null) {
						_staticObject = new Basic(new ObjectDataCD {
							objectID = objectInfo.objectID,
							variation = objectInfo.variation
						});
					}	
				}
			}
			
			public override void Update(SlotUIBase slot) {
				_visualObjects?.Update(slot);
			}
			
			public override bool ShowDetails(SlotUIBase slot, DetailsTab initialTab) {
				return _staticObject != null && _staticObject.ShowDetails(slot, initialTab);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = TileUtils.GetLocalizedDisplayName(_tileType, _tileset == Tileset.MAX_VALUE ? null : _tileset),
					dontLocalize = true
				};
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				return _staticObject != null ? _staticObject.GetHoverDescription(slot) : base.GetHoverDescription(slot);
			}

			public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
				return _staticObject != null ? _staticObject.GetHoverStats(slot, previewReinforced) : base.GetHoverStats(slot, previewReinforced);
			}
			
			private static IEnumerable<ObjectDataCD> GetObjectsToDisplay(TileType tileType) {
				return tileType switch {
					TileType.pit => new[] {
						new ObjectDataCD {
							objectID = ObjectID.Pit
						}
					},
					TileType.smallGrass => new[] {
						new ObjectDataCD {
							objectID = ObjectID.SmallGrass
						}
					},
					TileType.wall => ObjectUtils.GetAllObjects()
						.Where(objectData => PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.wall),
					TileType.water => ObjectUtils.GetAllObjects()
						.Where(objectData => PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.water),
					TileType.ground => ObjectUtils.GetAllObjects()
						.Where(objectData => PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.wall && TileUtils.IsBlock(tileCD.tileType, (Tileset) tileCD.tileset)),
					_ => Array.Empty<ObjectDataCD>()
				};
			}
		}
		
		public class CookedFood : DisplayedObject {
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectData
			};

			private readonly string _name;
			private readonly ObjectDataCD _objectData;
			
			public CookedFood(ObjectID id, ObjectID primaryIngredient = ObjectID.Egg, ObjectID secondaryIngredient = ObjectID.HeartBerry) {
				_objectData = new ObjectDataCD {
					objectID = id,
					variation = CookedFoodCD.GetFoodVariation(primaryIngredient, secondaryIngredient)
				};
				_name = ObjectUtils.GetInternalName(id).Replace("Rare", "").Replace("Epic", "");
			}
			
			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"Items/{_name}"
				};
			}
		}
		
		public class BiomeIcon : DisplayedObject {
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectsToDisplay.CurrentObjectData
			};
			
			private readonly Biome[] _biomes;
			private readonly CyclingObjectData _objectsToDisplay;
			
			public BiomeIcon(params Biome[] biomes) {
				_biomes = biomes;
				_objectsToDisplay = new CyclingObjectData(_biomes.Select(biome => new ObjectDataCD {
					objectID = GetBiomeIcon(biome)
				}));
			}

			public override void Update(SlotUIBase slot) {
				_objectsToDisplay.Update(slot);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				if (_biomes.Length > 1) {
					return new TextAndFormatFields {
						text = $"ItemBrowser-General/MultipleBiomes"
					};
				}
				
				return new TextAndFormatFields {
					text = $"BiomeNames/{_biomes[0]}"
				};
			}
			
			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				var lines = new List<TextAndFormatFields>();
				if (_biomes.Length <= 1)
					return lines;

				foreach (var biome in _biomes) {
					lines.Add(new TextAndFormatFields {
						text = "- {0}",
						formatFields = new[] {
							$"BiomeNames/{biome}"
						},
						dontLocalize = true,
						color = GetBiomeIcon(biome) == _objectsToDisplay.CurrentObjectData.objectID ? Color.white * 0.99f : UserInterfaceUtils.DescriptionColor
					});
				}

				return lines;
			}

			private static ObjectID GetBiomeIcon(Biome biome) {
				return biome switch {
					Biome.Slime => ObjectID.WallDirtBlock,
					Biome.Larva => ObjectID.WallClayBlock,
					Biome.Stone => ObjectID.WallStoneBlock,
					Biome.Nature => ObjectID.WallGrassBlock,
					Biome.Sea => ObjectID.WallLimestoneBlock,
					Biome.Desert => ObjectID.WallDesertBlock,
					Biome.Crystal => ObjectID.WallCrystalBlock,
					Biome.Passage => ObjectID.WallPassageBlock,
					Biome.Excavation => ObjectID.WallExcavationBlock,
					_ => ObjectID.WallObsidianBlock
				};
			}
		}
		
		public class SeasonIcon : DisplayedObject {
			public override ContainedObjectsBuffer VisualObject => new() {
				objectData = _objectData
			};
			
			private readonly Season _season;
			private readonly ObjectDataCD _objectData;
			
			public SeasonIcon(Season season) {
				_season = season;
				_objectData = new ObjectDataCD {
					objectID = GetSeasonIcon(_season)
				};
			}
			
			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"Seasons/{_season}"
				};
			}
			
			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				if (InputHelper.IsShowTechnicalInfoHeld) {
					return new List<TextAndFormatFields> {
						new() {
							text = _season.ToString(),
							dontLocalize = true
						}
					};
				}
				return base.GetHoverDescription(slot);
			}

			private static ObjectID GetSeasonIcon(Season season) {
				return season switch {
					Season.Easter => ObjectID.EasterEggNature,
					Season.Halloween => ObjectID.PumpkinHelm,
					Season.Christmas => ObjectID.ChristmasLuxuryPresent,
					Season.Valentine => ObjectID.BoxOfChocolates,
					Season.Anniversary => ObjectID.AnniversaryCake,
					Season.CherryBlossom => ObjectID.PinkCherryFlower,
					Season.LunarNewYear => ObjectID.ChineseCoin,
					_ => ObjectID.None
				};
			}
		}

		private class CyclingObjectData {
			private const float DefaultCycleSpeed = 2f;
			
			private readonly List<ObjectDataCD> _objectsToDisplay;
			private readonly float _cycleSpeed;
			private int _currentObjectDataIndex;
			private float _lastCycledTime;
			
			public ObjectDataCD CurrentObjectData => _objectsToDisplay.Count > 0 ? _objectsToDisplay[_currentObjectDataIndex] : default;

			public CyclingObjectData(IEnumerable<ObjectDataCD> objectsToDisplay, float cycleSpeed = DefaultCycleSpeed) {
				_objectsToDisplay = objectsToDisplay.ToList();
				_cycleSpeed = cycleSpeed;
			}
			
			public CyclingObjectData(float cycleSpeed = DefaultCycleSpeed) {
				_objectsToDisplay = new List<ObjectDataCD>();
				_cycleSpeed = cycleSpeed;
			}

			public void Add(ObjectDataCD objectData) {
				_objectsToDisplay.Add(objectData);
			}
			
			public void Update(SlotUIBase slot) {
				if (_lastCycledTime == 0f)
					_lastCycledTime = Time.time;
				
				if (Time.time >= _lastCycledTime + _cycleSpeed) {
					_currentObjectDataIndex++;
					if (_currentObjectDataIndex >= _objectsToDisplay.Count)
						_currentObjectDataIndex = 0;

					_lastCycledTime = Time.time;
					if (slot is ItemBrowserSlot basicItemSlot)
						basicItemSlot.UpdateVisuals();
				}
			}
		}
	}
}