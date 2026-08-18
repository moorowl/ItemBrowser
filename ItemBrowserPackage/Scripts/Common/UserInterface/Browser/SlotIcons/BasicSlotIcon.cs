using System.Collections.Generic;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Input;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.Options.DiscoveredObjects;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugTilemap;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
	public class BasicSlotIcon : SlotIcon {
		public override ContainedObjectsBuffer ContainedObject => new() {
			objectData = _objectsToDisplay.CurrentObjectData
		};

		public override (int Min, int Max) Amount { get; } = (1, 1);

		private readonly CyclingObjectData _objectsToDisplay;

		public BasicSlotIcon(ObjectDataCD objectData) {
			_objectsToDisplay = new CyclingObjectData(new ObjectDataCD[] {
				objectData
			});

			Amount = (objectData.amount, objectData.amount);
		}

		public BasicSlotIcon(ObjectDataCD objectData, (int Min, int Max) amount) : this(objectData) {
			Amount = amount;
		}

		public BasicSlotIcon(ObjectDataCD objectData, int amount) : this(objectData) {
			Amount = (amount, amount);
		}

		public BasicSlotIcon(params ObjectDataCD[] objectData) {
			_objectsToDisplay = new CyclingObjectData(objectData);
		}

		public override void Update(SlotUIBase slot) {
			_objectsToDisplay.Update(slot);
		}
		
		public override bool HasBeenDiscovered(SlotUIBase slot, out float temporaryTimeRemaining) {
			return DiscoveredTracker.HasBeenDiscoveredInDiscoveryMode(_objectsToDisplay.CurrentObjectData, out temporaryTimeRemaining);
		}

		public override void SetTemporarilyDiscovered(SlotUIBase slot, float? duration = null) {
			DiscoveredTracker.SetTemporarilyDiscovered(_objectsToDisplay.CurrentObjectData, duration);
		}

		public override bool ShowDetails(SlotUIBase slot, DetailsTab initialTab) {
			return ItemBrowserAPI.ItemBrowserUI.ShowDetails(_objectsToDisplay.CurrentObjectData, initialTab);
		}

		public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			var objectName = new TextAndFormatFields {
				text = ObjectUtility.GetLocalizedDisplayNameOrDefault(_objectsToDisplay.CurrentObjectData),
				dontLocalize = true
			};
			
			if (!DiscoveredTracker.HasBeenDiscoveredInDiscoveryMode(_objectsToDisplay.CurrentObjectData, out _)) {
				objectName = new TextAndFormatFields {
					text = "ItemBrowser-General/Undiscovered"
				};
			}

			var objectInfo = PugDatabase.GetObjectInfo(_objectsToDisplay.CurrentObjectData.objectID);
			if (objectInfo != null)
				objectName.color = Manager.text.GetRarityColor(objectInfo.rarity);

			return objectName;
		}

		public override List<TextAndFormatFields> GetHoverDescription(SlotUIBase slot) {
			if (!DiscoveredTracker.HasBeenDiscoveredInDiscoveryMode(_objectsToDisplay.CurrentObjectData, out _))
				return new List<TextAndFormatFields>();
			
			var objectData = _objectsToDisplay.CurrentObjectData;
			var term = ObjectUtility.GetInternalName(objectData);

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
					text = $"{(int)objectData.objectID}:{objectData.variation}",
					dontLocalize = true
				});
				lines.Add(new TextAndFormatFields {
					text = ObjectUtility.GetInternalName(objectData.objectID),
					dontLocalize = true
				});
				if (PugDatabase.TryGetComponent<TileCD>(objectData, out var tileCD)) {
					var isBlock = TileUtility.IsBlock(tileCD.tileType, (Tileset)tileCD.tileset);
					lines.Add(new TextAndFormatFields {
						text = isBlock ? $"{(Tileset)tileCD.tileset} ({TileType.wall} / {TileType.ground})" : $"{(Tileset)tileCD.tileset} ({tileCD.tileType})",
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

			if (objectData.objectID != ObjectID.None && OptionsManager.Instance.ShowSourceMod) {
				var associatedMod = ModUtility.GetAssociatedMod(objectData);
				lines.Add(new TextAndFormatFields {
					text = ModUtility.GetDisplayName(associatedMod),
					dontLocalize = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}

			if (ItemBrowserAPI.IsItemIndexed(objectData)) {
				ObjectUtility.GetTotalAmountInInventoryAndNearbyChests(Manager.main.player, objectData, out var inInventory, out var inNearbyChests);

				if (inInventory > 0 || inNearbyChests > 0) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/AmountInInventory",
						formatFields = new[] {
							(inInventory + inNearbyChests).ToString()
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtility.AlmostWhiteColor
					});
				}
			}

			return lines;
		}

		public override List<TextAndFormatFields> GetHoverStats(SlotUIBase slot, bool previewReinforced) {
			if (!HasBeenDiscovered(slot, out _))
				return base.GetHoverStats(slot, previewReinforced);
			
			var lines = slot.GetHoverStats(ContainedObject, previewReinforced, false);

			var displayNameNote = ObjectUtility.GetUnlocalizedDisplayNameNote(_objectsToDisplay.CurrentObjectData);
			if (displayNameNote == null)
				return lines;

			lines ??= new List<TextAndFormatFields>();
			lines.Insert(0, new TextAndFormatFields {
				text = displayNameNote,
				color = UserInterfaceUtility.DescriptionColor
			});

			return lines;
		}
	}
}