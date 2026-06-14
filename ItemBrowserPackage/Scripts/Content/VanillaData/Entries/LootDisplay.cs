using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class LootDisplay : ObjectEntryDisplay<Loot> {
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
		
		public override IEnumerable<Loot> OnSort(IEnumerable<Loot> entries) {
			return entries
				// Normal -> dungeon -> scene
				.OrderByDescending(entry => entry.FoundInDungeons.Count > 0 ? 0 : (entry.FoundInScenes.Count > 0 ? 1 : 2))
				.ThenBy(entry => ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation))
				.ThenByDescending(entry => entry.IsFromGuaranteedPool ? 1 : 0);
		}
		
		protected override void OnRender(Loot entry) {
			resultSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Result.Id,
				variation = entry.Result.Variation
			}, entry.Amount.Get());
			sourceSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = entry.Entity.Id,
				variation = entry.Entity.Variation
			});
			
			var showPoolTypeText = entry.IsFromLootTableWithGuaranteedPool;

			chanceForOneText.Render(UserInterfaceUtility.FormatChance(entry.ChanceForOne.Get()) + "%");
			chanceForOneText.transform.localPosition = new Vector3(
				chanceForOneText.transform.localPosition.x,
				showPoolTypeText ? textOffsetWhenShowingBoth : 0f,
				chanceForOneText.transform.localPosition.z
			);
			
			poolTypeText.gameObject.SetActive(showPoolTypeText);
			if (showPoolTypeText)
				poolTypeText.Render(entry.IsFromGuaranteedPool ? "ItemBrowser-General/GuaranteedPool" : "ItemBrowser-General/RandomPool");
		}

		protected override void OnRenderDescription(Loot entry, EntryDescriptionButton description) {
			var showPoolTypeText = entry.IsFromLootTableWithGuaranteedPool;
			var rolls = UserInterfaceUtility.FormatRange(entry.Rolls.Get());
			var chanceForOne = UserInterfaceUtility.FormatChance(entry.ChanceForOne.Get());
			var chancePerRoll = UserInterfaceUtility.FormatChance(entry.Chance);
			var amount = UserInterfaceUtility.FormatRange(entry.Amount.Get());

			description.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (entry.IsFromGuaranteedPool ? "ItemBrowser-ObjectEntryDescriptions/Loot_0_GuaranteedPool" : "ItemBrowser-ObjectEntryDescriptions/Loot_0_RandomPool") : "ItemBrowser-ObjectEntryDescriptions/Loot_0",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			description.AddPadding();
			if (chanceForOne != chancePerRoll) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Loot_1_ForOne",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Loot_1_PerRoll",
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
					text = "ItemBrowser-ObjectEntryDescriptions/Loot_1",
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

				if (entry.OnlyDropsInBiome != Biome.None) {
					description.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/Loot_2",
						formatFields = new[] {
							$"BiomeNames/{entry.OnlyDropsInBiome}"
						},
						color = UserInterfaceUtility.DescriptionColor
					});	
				}
			}

			OnRenderStructureInfo(entry);
		}
		
		private void OnRenderStructureInfo(Loot entry) {
			structureInfo.gameObject.SetActive(entry.FoundInScenes.Count > 0 || entry.FoundInDungeons.Count > 0);
			if (!structureInfo.gameObject.activeSelf)
				return;
			
			structureInfo.Clear();
			structureInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-General/StructureExclusiveLoot"
			});
			
			if (entry.FoundInDungeons.Count > 0) {
				structureInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Loot_5",
					color = UserInterfaceUtility.DescriptionColor
				});

				foreach (var dungeon in entry.FoundInDungeons) {
					structureInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/Loot_5",
						formatFields = new[] {
							dungeon.Name
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtility.DescriptionColor
					});	
				}
			}
			
			if (entry.FoundInScenes.Count > 0) {
				structureInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/Loot_3",
					color = UserInterfaceUtility.DescriptionColor
				});

				foreach (var scene in entry.FoundInScenes) {
					structureInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser-ObjectEntryDescriptions/Loot_4",
						formatFields = new[] {
							StructureUtility.GetPersistentSceneName(scene.Name),
							scene.Amount.ToString()
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtility.DescriptionColor
					});	
				}
			}
		}
	}
}