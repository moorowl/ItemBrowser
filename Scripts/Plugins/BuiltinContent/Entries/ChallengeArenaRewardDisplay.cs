using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class ChallengeArenaRewardDisplay : ObjectEntryDisplay<ChallengeArenaReward> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot sourceSlot;
		[SerializeField]
		private PugText chanceForOneText;
		[SerializeField]
		private PugText poolTypeText;
		[SerializeField]
		private float textOffsetWhenShowingBoth;

		public override IEnumerable<ChallengeArenaReward> SortEntries(IEnumerable<ChallengeArenaReward> entries) {
			return entries.OrderByDescending(entry => entry.ChanceForOne).ThenByDescending(entry => entry.Amount.Max);
		}
		
		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			}, Entry.Amount);
			sourceSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectID.AlienChest
			});
			
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;
			var chanceText = $"{UserInterfaceUtils.FormatChance(Entry.ChanceForOne)}%";
			if (Entry.ChanceWhenBraveMerchantAlive != null)
				chanceText = $"{chanceText} / {UserInterfaceUtils.FormatChance(Entry.ChanceWhenBraveMerchantAlive.Value)}%";

			chanceForOneText.Render(chanceText);
			chanceForOneText.transform.localPosition = new Vector3(
				chanceForOneText.transform.localPosition.x,
				showPoolTypeText ? textOffsetWhenShowingBoth : 0f,
				chanceForOneText.transform.localPosition.z
			);
			
			poolTypeText.gameObject.SetActive(showPoolTypeText);
			if (showPoolTypeText)
				poolTypeText.Render(Entry.IsFromGuaranteedPool ? "ItemBrowser:GuaranteedPool" : "ItemBrowser:RandomPool");
		}

		private void RenderMoreInfo() {
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;
			var rolls = UserInterfaceUtils.FormatAmountOrRollsRange(Entry.Rolls);
			var chanceForOne = UserInterfaceUtils.FormatChance(Entry.ChanceForOne);
			var chancePerRoll = UserInterfaceUtils.FormatChance(Entry.Chance);
			var amount = UserInterfaceUtils.FormatAmountOrRollsRange(Entry.Amount);
			
			MoreInfo.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (Entry.IsFromGuaranteedPool ? "ItemBrowser:MoreInfo/ChallengeArenaReward_0_GuaranteedPool" : "ItemBrowser:MoreInfo/ChallengeArenaReward_0_RandomPool") : "ItemBrowser:MoreInfo/ChallengeArenaReward_0",
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			if (chanceForOne != chancePerRoll) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/ChallengeArenaReward_1_ForOne",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/ChallengeArenaReward_1_PerRoll",
					formatFields = new[] {
						chancePerRoll,
						amount,
						rolls
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/ChallengeArenaReward_1",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}

			if (Entry.ChanceWhenBraveMerchantAlive != null) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/ChallengeArenaReward_1_BraveMerchant",
					formatFields = new[] {
						UserInterfaceUtils.FormatChance(Entry.ChanceWhenBraveMerchantAlive.Value)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			
			if (Entry.OnlyDropsInBiome != Biome.None) {
				MoreInfo.AddPadding();

				if (Entry.OnlyDropsInBiome != Biome.None) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/ChallengeArenaReward_2",
						formatFields = new[] {
							$"BiomeNames/{Entry.OnlyDropsInBiome}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
			}
		}
	}
}