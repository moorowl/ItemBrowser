using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record BackgroundPerks : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/BackgroundPerks", ObjectID.FoodRation, VanillaPriorities.BackgroundPerks);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; set; }
		public CharacterRole Background { get; set; }
		public SkillID BackgroundSkill { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var rolePerksTable = ((CharacterCustomizationMenu) Manager.menu.characterCustomizationMenu).roleSelection.perksTable;
				
				foreach (var entry in rolePerksTable.perks.GroupBy(entry => entry.role).Select(group => group.First())) {
					foreach (var starterItem in entry.starterItems) {
						registry.Register(ObjectEntryType.Source, starterItem.objectID, 0, new BackgroundPerks {
							Result = (starterItem.objectID, starterItem.variation, starterItem.amount),
							Background = entry.role,
							BackgroundSkill = entry.starterSkill
						});
					}
				}
			}
		}
	}
}