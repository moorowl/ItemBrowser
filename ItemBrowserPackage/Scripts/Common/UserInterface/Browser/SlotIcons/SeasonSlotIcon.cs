using System.Collections.Generic;
using ItemBrowser.Common.Input;

namespace ItemBrowser.Common.UserInterface.SlotIcons {
	public class SeasonSlotIcon : SlotIcon {
		public override ContainedObjectsBuffer VisualObject => new() {
			objectData = _objectData
		};

		private readonly Season _season;
		private readonly ObjectDataCD _objectData;

		public SeasonSlotIcon(Season season) {
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
						text = "{0} ({1})",
						dontLocalize = true,
						formatFields = new[] {
							_season.ToString(),
							((int)_season).ToString()
						},
						dontLocalizeFormatFields = true,
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
}