using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Options.DiscoveredObjects;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
	public class BiomeSlotIcon : SlotIcon {
		public override ContainedObjectsBuffer VisualObject => new() {
			objectData = _objectsToDisplay.CurrentObjectData
		};

		private readonly Biome[] _biomes;
		private readonly CyclingObjectData _objectsToDisplay;

		public BiomeSlotIcon(params Biome[] biomes) {
			_biomes = biomes;
			_objectsToDisplay = new CyclingObjectData(_biomes.Select(biome => new ObjectDataCD {
				objectID = GetBiomeIcon(biome)
			}));
		}

		public override void Update(SlotUIBase slot) {
			_objectsToDisplay.Update(slot);
		}

		public override bool HasBeenDiscovered(SlotUIBase slot, out float temporaryTimeRemaining) {
			return DiscoveredTracker.HasBeenDiscoveredInDiscoveryMode(_biomes[_objectsToDisplay.CurrentObjectDataIndex], out temporaryTimeRemaining);
		}

		public override void SetTemporarilyDiscovered(SlotUIBase slot, float? duration = null) {
			DiscoveredTracker.SetTemporarilyDiscovered(_biomes[_objectsToDisplay.CurrentObjectDataIndex], duration);
		}

		public override TextAndFormatFields GetHoverTitle(SlotUIBase slot) {
			if (_biomes.Length > 1) {
				return new TextAndFormatFields {
					text = $"ItemBrowser-General/MultipleBiomes"
				};
			}

			return new TextAndFormatFields {
				text = DiscoveredTracker.HasBeenDiscoveredInDiscoveryMode(_biomes[0], out _) ? $"BiomeNames/{_biomes[0]}" : "ItemBrowser-General/Undiscovered"
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
						DiscoveredTracker.HasBeenDiscoveredInDiscoveryMode(biome, out _) ? $"BiomeNames/{biome}" : "ItemBrowser-General/Undiscovered"
					},
					dontLocalize = true,
					color = GetBiomeIcon(biome) == _objectsToDisplay.CurrentObjectData.objectID ? UserInterfaceUtility.AlmostWhiteColor : UserInterfaceUtility.DescriptionColor
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
}