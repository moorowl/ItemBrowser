using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class ChallengeArenaRewardDisplay : ObjectEntryDisplay<ChallengeArenaReward> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot sourceSlot;
		public PugText chanceForOneText;
		public PugText poolTypeText;
		public float textOffsetWhenShowingBoth;

		public override IEnumerable<ChallengeArenaReward> OnSort(IEnumerable<ChallengeArenaReward> entries) {
			return entries.OrderByDescending(entry => entry.ChanceForOne).ThenByDescending(entry => entry.Amount.Max);
		}
		
		protected override void OnRender(ChallengeArenaReward entry) {
			resultSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = entry.Result
			}, entry.Amount);
			sourceSlot.DisplayedObject = new DisplayedObject.Basic(new ObjectDataCD {
				objectID = ObjectID.AlienChest
			});
			
			var showPoolTypeText = entry.IsFromTableWithGuaranteedPool;
			var chanceText = $"{UserInterfaceUtils.FormatChance(entry.ChanceForOne)}%";
			if (entry.ChanceWhenBraveMerchantAlive != null)
				chanceText = $"{chanceText} / {UserInterfaceUtils.FormatChance(entry.ChanceWhenBraveMerchantAlive.Value)}%";

			chanceForOneText.Render(chanceText);
			chanceForOneText.transform.localPosition = new Vector3(
				chanceForOneText.transform.localPosition.x,
				showPoolTypeText ? textOffsetWhenShowingBoth : 0f,
				chanceForOneText.transform.localPosition.z
			);
			
			poolTypeText.gameObject.SetActive(showPoolTypeText);
			if (showPoolTypeText)
				poolTypeText.Render(entry.IsFromGuaranteedPool ? "ItemBrowser-General/GuaranteedPool" : "ItemBrowser-General/RandomPool");
		}

		protected override void OnRenderDescription(ChallengeArenaReward entry, EntryDescriptionButton description) {
			var showPoolTypeText = entry.IsFromTableWithGuaranteedPool;
			var rolls = UserInterfaceUtils.FormatRange(entry.Rolls);
			var chanceForOne = UserInterfaceUtils.FormatChance(entry.ChanceForOne);
			var chancePerRoll = UserInterfaceUtils.FormatChance(entry.Chance);
			var amount = UserInterfaceUtils.FormatRange(entry.Amount);
			
			description.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (entry.IsFromGuaranteedPool ? "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_0_GuaranteedPool" : "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_0_RandomPool") : "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_0",
				color = UserInterfaceUtils.DescriptionColor
			});
			description.AddPadding();
			if (chanceForOne != chancePerRoll) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_1_ForOne",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_1_PerRoll",
					formatFields = new[] {
						chancePerRoll,
						amount,
						rolls
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_1",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			if (entry.ChanceWhenBraveMerchantAlive != null) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_1_BraveMerchant",
					formatFields = new[] {
						UserInterfaceUtils.FormatChance(entry.ChanceWhenBraveMerchantAlive.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			
			if (entry.OnlyDropsInBiome != Biome.None) {
				description.AddPadding();

				if (entry.OnlyDropsInBiome != Biome.None) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/ChallengeArenaReward_2",
						formatFields = new[] {
							$"BiomeNames/{entry.OnlyDropsInBiome}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
			}
		}
	}
}