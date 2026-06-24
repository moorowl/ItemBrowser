using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugTilemap;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
		public class TileSlotIcon : SlotIcon {
			public override ContainedObjectsBuffer ContainedObject => _staticObject?.ContainedObject ?? new ContainedObjectsBuffer();

			public override ContainedObjectsBuffer VisualObject => _staticObject?.VisualObject ?? new ContainedObjectsBuffer {
				objectData = _visualObjects?.CurrentObjectData ?? default
			};

			private readonly TileType _tileType;
			private readonly Tileset _tileset;
			private readonly CyclingObjectData _visualObjects;
			private readonly BasicSlotIcon _staticObject;

			public TileSlotIcon(TileType tileType, Tileset? tileset = null) {
				_tileType = tileType;
				_tileset = tileset ?? Tileset.MAX_VALUE;

				if (tileset == null) {
					_visualObjects = new CyclingObjectData(GetObjectsToDisplay(_tileType).OrderBy(objectData => PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD) ? levelCD.level : 0));
				} else {
					var objectInfo = PugDatabase.TryGetTileItemInfo(_tileType == TileType.ground ? TileType.wall : _tileType, (int)tileset);
					if (objectInfo != null) {
						_staticObject = new BasicSlotIcon(new ObjectDataCD {
							objectID = objectInfo.objectID,
							variation = objectInfo.variation
						});
					}
				}
			}

			public override void Update(SlotUIBase slot) {
				_visualObjects?.Update(slot);
			}
			
			public override bool HasBeenDiscovered(SlotUIBase slot, out float temporaryTimeRemaining) {
				if (_staticObject != null)
					return DiscoveredTracker<ObjectDataCD>.HasBeenDiscovered(_staticObject.ContainedObject.objectData, out temporaryTimeRemaining);
				if (_visualObjects != null)
					return DiscoveredTracker<ObjectDataCD>.HasBeenDiscovered(_visualObjects.CurrentObjectData, out temporaryTimeRemaining);

				temporaryTimeRemaining = 0f;
				return true;
			}

			public override void SetTemporarilyDiscovered(SlotUIBase slot, float? duration = null) {
				if (_staticObject != null)
					DiscoveredTracker<ObjectDataCD>.SetTemporarilyDiscovered(_staticObject.ContainedObject.objectData, duration);
				if (_visualObjects != null)
					DiscoveredTracker<ObjectDataCD>.SetTemporarilyDiscovered(_visualObjects.CurrentObjectData, duration);
			}

			public override bool ShowDetails(SlotUIBase slot, DetailsTab initialTab) {
				return _staticObject != null && _staticObject.ShowDetails(slot, initialTab);
			}

			public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
				if (!HasBeenDiscovered(slot, out _)) {
					return new TextAndFormatFields {
						text = "ItemBrowser-General/Undiscovered"
					};
				}
				
				return new TextAndFormatFields {
					text = TileUtility.GetLocalizedDisplayName(_tileType, _tileset == Tileset.MAX_VALUE ? null : _tileset),
					dontLocalize = true
				};
			}

			public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
				if (!HasBeenDiscovered(slot, out _))
					return base.GetHoverDescription(slot);
				
				return _staticObject != null ? _staticObject.GetHoverDescription(slot) : base.GetHoverDescription(slot);
			}

			public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
				if (!HasBeenDiscovered(slot, out _))
					return base.GetHoverStats(slot, previewReinforced);
				
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
					TileType.wall => ObjectUtility.GetAllObjects()
						.Where(objectData => PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.wall),
					TileType.water => ObjectUtility.GetAllObjects()
						.Where(objectData => PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.water),
					TileType.ground => ObjectUtility.GetAllObjects()
						.Where(objectData => PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD) && tileCD.tileType == TileType.wall && TileUtility.IsBlock(tileCD.tileType, (Tileset)tileCD.tileset)),
					_ => Array.Empty<ObjectDataCD>()
				};
			}
	}
}