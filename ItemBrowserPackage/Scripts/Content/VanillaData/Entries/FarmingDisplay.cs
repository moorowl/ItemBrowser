using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class FarmingDisplay : ObjectEntryDisplay<Farming> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot seedSlot;

		public override IEnumerable<Farming> OnSort(IEnumerable<Farming> entries) {
			return entries
				.OrderBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Result))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Seed));
		}
		
		protected override void OnRender(Farming entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			});
			seedSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Seed
			});
		}

		protected override void OnRenderDescription(Farming entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = entry.HasGoldSeed ? "ItemBrowser-ObjectEntryDescriptions/Farming_0_" + (entry.RequiresGoldSeed ? "Golden" : "Normal") : "ItemBrowser-ObjectEntryDescriptions/Farming_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Seed)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Farming_2",
				formatFields = new[] {
					(entry.GrowthTime / 60f).ToString(LocalizationManager.CurrentCulture)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			
			if (entry.RequiresGoldSeed) {
				var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
					.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceToGainRarePlant).conditionValuePerPoint;
				var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
				
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Farming_1",
					formatFields = new[] {
						chanceAtMin.ToString(),
						chanceAtMax.ToString(),
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Farming_3",
					formatFields = new[] {
						chanceAtMin.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}