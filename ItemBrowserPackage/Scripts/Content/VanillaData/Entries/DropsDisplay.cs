using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class DropsDisplay : ObjectEntryDisplay<Drops> {
		public ItemBrowserSlot resultSlot;
		public ItemBrowserSlot sourceSlot;
		public PugText chanceForOneText;
		public PugText poolTypeText;
		public float textOffsetWhenShowingBoth;
		public EntryDescriptionButton structureInfo;

		private int _lastPlayerCount;
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (_lastPlayerCount != ClientWorldStateSystem.PlayerCount) {
				RequestRerender();
				_lastPlayerCount = ClientWorldStateSystem.PlayerCount;
			}
		}
		
		public override IEnumerable<Drops> OnSort(IEnumerable<Drops> entries) {
			return entries
				.OrderByDescending(entry => entry.FoundInScenes.Count > 0 ? 0 : 1)
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation))
				.ThenByDescending(entry => entry.IsFromGuaranteedPool ? 1 : 0);
		}
		
		protected override void OnRender(Drops entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			}, entry.Amount.Get());
			sourceSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Entity.Id,
				variation = entry.Entity.Variation
			});
			
			var showPoolTypeText = entry.IsFromLootTableWithGuaranteedPool;
			var chanceForOne = UserInterfaceUtility.FormatChance(entry.ChanceForOne.Get());

			chanceForOneText.Render(chanceForOne + "%");
			chanceForOneText.transform.localPosition = new Vector3(
				chanceForOneText.transform.localPosition.x,
				showPoolTypeText ? textOffsetWhenShowingBoth : 0f,
				chanceForOneText.transform.localPosition.z
			);
			
			poolTypeText.gameObject.SetActive(showPoolTypeText);
			if (showPoolTypeText)
				poolTypeText.Render(entry.IsFromGuaranteedPool ? "ItemBrowser-General/GuaranteedPool" : "ItemBrowser-General/RandomPool");
		}

		protected override void OnRenderDescription(Drops entry, EntryDescriptionButton description) {
			var showPoolTypeText = entry.IsFromLootTableWithGuaranteedPool;
			var rolls = UserInterfaceUtility.FormatRange(entry.Rolls.Get());
			var chanceForOne = UserInterfaceUtility.FormatChance(entry.ChanceForOne.Get());
			var chancePerRoll = UserInterfaceUtility.FormatChance(entry.Chance);
			var amount = UserInterfaceUtility.FormatRange(entry.Amount.Get());

			description.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (entry.IsFromGuaranteedPool ? "ItemBrowser-ObjectEntryDescriptions/Drops_0_GuaranteedPool" : "ItemBrowser-ObjectEntryDescriptions/Drops_0_RandomPool") : "ItemBrowser-ObjectEntryDescriptions/Drops_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			if (chanceForOne != chancePerRoll) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Drops_1_ForOne",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Drops_1_PerRoll",
					formatFields = new[] {
						chancePerRoll,
						amount,
						rolls
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			} else {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Drops_1",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
			
			if (entry.OnlyDropsInBiome != Biome.None) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Drops_2",
					formatFields = new[] {
						$"BiomeNames/{entry.OnlyDropsInBiome}"
					},
					color = UserInterfaceUtility.AlmostWhiteColor
				});
			}
			
			if (entry.IsAffectedByPlayerCount || entry.IsAffectedByWorldMode) {
				description.AddPadding();
				description.AddLine(new TextAndFormatFields {
					text = entry.IsAffectedByPlayerCount && entry.IsAffectedByWorldMode ? "ItemBrowser-DynamicValues/PlayerCountAndWorldMode" : "ItemBrowser-DynamicValues/PlayerCount",
					color = UserInterfaceUtility.AlmostWhiteColor
				});	
			}
			
			OnRenderStructureInfo(entry);
		}
		
		private void OnRenderStructureInfo(Drops entry) {
			structureInfo.gameObject.SetActive(entry.FoundInScenes.Count > 0);
			if (!structureInfo.gameObject.activeSelf)
				return;
			
			structureInfo.Clear();
			structureInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-General/StructureExclusiveDrop"
			});
			structureInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/Drops_5",
				color = UserInterfaceUtility.DescriptionColor
			});

			foreach (var scene in entry.FoundInScenes) {
				structureInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Drops_6",
					formatFields = new[] {
						StructureUtility.GetPersistentSceneName(scene.Name)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});	
			}
		}
	}
}