using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public class LootDisplay : ObjectEntryDisplay<Loot> {
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
		[SerializeField]
		private MoreInfoButton structureInfo;

		private int _lastPlayerCount;

		public override IEnumerable<Loot> SortEntries(IEnumerable<Loot> entries) {
			return entries
				// Normal -> dungeon -> scene
				.OrderByDescending(entry => entry.FoundInDungeons.Count > 0 ? 0 : (entry.FoundInScenes.Count > 0 ? 1 : 2))
				.ThenBy(entry => ObjectUtils.GetLocalizedDisplayNameOrDefault(entry.Entity.Id, entry.Entity.Variation))
				.ThenByDescending(entry => entry.IsFromGuaranteedPool ? 1 : 0);
		}
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (_lastPlayerCount != WorldUtils.ClientPlayerCount)
				Render();
		}

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
			RenderStructureInfo();
			_lastPlayerCount = WorldUtils.ClientPlayerCount;
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			}, Entry.Amount());
			sourceSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Entity.Id,
				variation = Entry.Entity.Variation
			});
			
			var showPoolTypeText = Entry.IsFromTableWithGuaranteedPool;

			chanceForOneText.Render(UserInterfaceUtils.FormatChance(Entry.ChanceForOne()) + "%");
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
			var rolls = UserInterfaceUtils.FormatAmountOrRollsRange(Entry.Rolls());
			var chanceForOne = UserInterfaceUtils.FormatChance(Entry.ChanceForOne());
			var chancePerRoll = UserInterfaceUtils.FormatChance(Entry.Chance);
			var amount = UserInterfaceUtils.FormatAmountOrRollsRange(Entry.Amount());

			MoreInfo.AddLine(new TextAndFormatFields {
				text = showPoolTypeText ? (Entry.IsFromGuaranteedPool ? "ItemBrowser:MoreInfo/Loot_0_GuaranteedPool" : "ItemBrowser:MoreInfo/Loot_0_RandomPool") : "ItemBrowser:MoreInfo/Loot_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Entity.Id, Entry.Entity.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			if (chanceForOne != chancePerRoll) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Loot_1_ForOne",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Loot_1_PerRoll",
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
					text = "ItemBrowser:MoreInfo/Loot_1",
					formatFields = new[] {
						chanceForOne,
						amount
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			
			if (Entry.OnlyDropsInBiome != Biome.None) {
				MoreInfo.AddPadding();

				if (Entry.OnlyDropsInBiome != Biome.None) {
					MoreInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Loot_2",
						formatFields = new[] {
							$"BiomeNames/{Entry.OnlyDropsInBiome}"
						},
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
			}
		}

		private void RenderStructureInfo() {
			structureInfo.gameObject.SetActive(Entry.FoundInScenes.Count > 0 || Entry.FoundInDungeons.Count > 0);
			if (!structureInfo.gameObject.activeSelf)
				return;
			
			structureInfo.Clear();
			structureInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:StructureExclusiveLoot"
			});
			
			if (Entry.FoundInDungeons.Count > 0) {
				structureInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Loot_5",
					color = UserInterfaceUtils.DescriptionColor
				});

				foreach (var dungeon in Entry.FoundInDungeons) {
					structureInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Loot_4",
						formatFields = new[] {
							dungeon.Name
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
			}
			
			if (Entry.FoundInScenes.Count > 0) {
				structureInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Loot_3",
					color = UserInterfaceUtils.DescriptionColor
				});

				foreach (var scene in Entry.FoundInScenes) {
					structureInfo.AddLine(new TextAndFormatFields {
						text = "ItemBrowser:MoreInfo/Loot_4",
						formatFields = new[] {
							StructureUtils.GetPersistentSceneName(scene.Name)
						},
						dontLocalizeFormatFields = true,
						color = UserInterfaceUtils.DescriptionColor
					});	
				}
			}
		}
	}
}