using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record ArchaeologistDrops : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/ArchaeologistDrops", ObjectID.WallStoneBlock, VanillaPriorities.ArchaeologistDrops);
		
		public ObjectID Result { get; set; }
		public (float Min, float Max) Chance { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var chanceAtMin = Manager.mod.SkillTalentsTable.skillTalentTrees.SelectMany(tree => tree.skillTalents)
					.FirstOrDefault(talent => talent.givesCondition == ConditionID.ChanceForRandomLootFromWall).conditionValuePerPoint / 1000f;
				var chanceAtMax = chanceAtMin * Constants.kSkillPointsPerTalentPoint;
				
				foreach (var drop in LootUtility.GetLootTableHelper(LootTableID.ArcheologistWallLoot).RandomPool) {
					registry.Register(ObjectEntryType.Source, drop.Item, 0, new ArchaeologistDrops {
						Result = drop.Item,
						Chance = (chanceAtMin * drop.Chance, chanceAtMax * drop.Chance)
					});
				}
			}
		}
	}
}