using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class FarmingDisplay : ObjectEntryDisplay<Farming> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot seedSlot;

		public override IEnumerable<Farming> OnSort(IEnumerable<Farming> entries) {
			return entries
				.OrderBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Seed.Id, entry.Seed.Variation));
		}
		
		protected override void OnRender(Farming entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result
			});
			seedSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Seed.Id,
				variation = entry.Seed.Variation
			});
		}

		protected override void OnRenderDescription(Farming entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = entry.HasGoldSeed ? "ItemBrowser-ObjectEntryDescriptions/Farming_0_" + (entry.RequiresGoldSeed ? "Golden" : "Normal") : "ItemBrowser-ObjectEntryDescriptions/Farming_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Seed.Id, entry.Seed.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Farming_2",
				formatFields = new[] {
					(entry.GrowthTime / 60f).ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			
			if (entry.RequiresGoldSeed && entry.HasGoldSeed) {
				var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
					.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceToGainRarePlant).conditionValuePerPoint;
				var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
				
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Farming_1",
					formatFields = new[] {
						((int) Constants.baseChanceToGainRarePlantPercentage).ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Farming_3",
					formatFields = new[] {
						chanceAtMin.ToString(),
                        chanceAtMax.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
	}
}