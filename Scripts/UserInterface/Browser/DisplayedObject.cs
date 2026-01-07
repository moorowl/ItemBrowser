using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public abstract class DisplayedObject {
		public virtual ContainedObjectsBuffer ContainedObject => default;
		public virtual ContainedObjectsBuffer VisualObject => ContainedObject;
		public virtual (int Min, int Max) Amount => (ContainedObject.amount, ContainedObject.amount);
			
		public virtual void Update(SlotUIBase slot) { }
		
		public virtual bool ShowEntries(SlotUIBase slot, ObjectEntryType type) {
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
		
		public class Static : DisplayedObject {
			public override ContainedObjectsBuffer ContainedObject => new() {
				objectData = _objectData
			};
			public override (int Min, int Max) Amount { get; } = (1, 1);

			private readonly ObjectDataCD _objectData;

			public Static(ObjectDataCD objectData) {
				_objectData = objectData;
				Amount = (_objectData.amount, _objectData.amount);
			}
			
			public Static(ObjectDataCD objectData, (int Min, int Max) amount) : this(objectData) {
				Amount = amount;
			}
			
			public Static(ObjectDataCD objectData, int amount) : this(objectData) {
				Amount = (amount, amount);
			}
			
			public override bool ShowEntries(SlotUIBase slot, ObjectEntryType type) {
				return ItemBrowserAPI.ItemBrowserUI.ShowObjectEntries(_objectData, type);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				var objectName = new TextAndFormatFields {
					text = ObjectUtils.GetLocalizedDisplayNameOrDefault(_objectData.objectID, _objectData.variation),
					dontLocalize = true
				};

				var objectInfo = PugDatabase.GetObjectInfo(_objectData.objectID);
				if (objectInfo != null)
					objectName.color = Manager.text.GetRarityColor(objectInfo.rarity);

				return objectName;
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				var term = ObjectUtils.GetInternalName(_objectData.objectID);

				var nameTermOverride = Manager.ui.itemOverridesTable.GetNameTermOverride(_objectData);
				if (nameTermOverride != null)
					term = nameTermOverride;

				var lines = new List<TextAndFormatFields> {
					new() {
						text = $"Items/{term}Desc"
					}
				};

				if (InputHandler.IsShowTechnicalInfoHeld) {
					lines.Add(new TextAndFormatFields {
						text = $"{(int) _objectData.objectID}:{_objectData.variation}",
						dontLocalize = true
					});
					lines.Add(new TextAndFormatFields {
						text = ObjectUtils.GetInternalName(_objectData.objectID),
						dontLocalize = true
					});
					if (PugDatabase.TryGetComponent<TileCD>(_objectData, out var tileCD)) {
						var isBlock = TileUtils.IsBlock(tileCD.tileType, (Tileset) tileCD.tileset);
						lines.Add(new TextAndFormatFields {
							text = isBlock ? $"{(Tileset) tileCD.tileset} ({TileType.wall} / {TileType.ground})" : $"{(Tileset) tileCD.tileset} ({tileCD.tileType})",
							dontLocalize = true
						});
					}
					var prefabInfo = PugDatabase.GetObjectInfo(_objectData.objectID, _objectData.variation).prefabInfos[0];
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

				if (_objectData.objectID != ObjectID.None && Options.ShowSourceMod) {
					var associatedMod = ModUtils.GetAssociatedMod(_objectData);
					lines.Add(new TextAndFormatFields {
						text = ModUtils.GetDisplayName(associatedMod),
						dontLocalize = true,
						color = UserInterfaceUtils.DescriptionColor
					});
				}

				return lines;
			}

			public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
				var lines = slot.GetHoverStats(ContainedObject, previewReinforced, false);

				var displayNameNote = ObjectUtils.GetUnlocalizedDisplayNameNote(_objectData.objectID, _objectData.variation);
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
					text = $"ItemBrowser:ObjectCategoryNames/{_tag}"
				};
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				if (InputHandler.IsShowTechnicalInfoHeld) {
					return new List<TextAndFormatFields> {
						new() {
							text = _tag.ToString(),
							dontLocalize = true
						}
					};
				}
				return base.GetHoverDescription(slot);
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
			private readonly Static _staticObject;

			public Tile(TileType tileType, Tileset? tileset = null) {
				_tileType = tileType;
				_tileset = tileset ?? Tileset.MAX_VALUE;

				if (tileset == null) {
					_visualObjects = new CyclingObjectData(GetObjectsToDisplay(_tileType));
				} else {
					var objectInfo = PugDatabase.TryGetTileItemInfo(_tileType == TileType.ground ? TileType.wall : _tileType, (int) tileset);
					if (objectInfo != null) {
						_staticObject = new Static(new ObjectDataCD {
							objectID = objectInfo.objectID,
							variation = objectInfo.variation
						});
					}	
				}
			}
			
			public override void Update(SlotUIBase slot) {
				_visualObjects?.Update(slot);
			}
			
			public override bool ShowEntries(SlotUIBase slot, ObjectEntryType type) {
				return _staticObject != null && _staticObject.ShowEntries(slot, type);
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
				objectData = _objectData
			};
			
			private readonly Biome _biome;
			private readonly ObjectDataCD _objectData;
			
			public BiomeIcon(Biome biome) {
				_biome = biome;
				_objectData = new ObjectDataCD {
					objectID = GetBiomeIcon(_biome)
				};
			}
			
			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				return new TextAndFormatFields {
					text = $"BiomeNames/{_biome}"
				};
			}
			
			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				if (InputHandler.IsShowTechnicalInfoHeld) {
					return new List<TextAndFormatFields> {
						new() {
							text = _biome.ToString(),
							dontLocalize = true
						}
					};
				}
				return base.GetHoverDescription(slot);
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
				if (InputHandler.IsShowTechnicalInfoHeld) {
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
			private const float DefaultCycleSpeed = 1f;
			
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
				if (Time.time >= _lastCycledTime + _cycleSpeed) {
					_currentObjectDataIndex++;
					if (_currentObjectDataIndex >= _objectsToDisplay.Count)
						_currentObjectDataIndex = 0;

					_lastCycledTime = Time.time;
					if (slot is BasicItemSlot basicItemSlot)
						basicItemSlot.UpdateVisuals();
				}
			}
		}
	}
}