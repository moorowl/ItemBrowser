using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using Pug.Automation;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record DropsWhenDamaged : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/DropsWhenDamaged", ObjectID.SolariteOre, VanillaPriorities.DropsWhenDamaged);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public (ObjectID Id, int Variation) Entity { get; set; }
		public int DamageRequiredToDrop { get; set; }
		public int HealthRequiredToDrop { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<DropsLootWhenDamagedCD>(objectData, out var dropsLootWhenDamagedCD))
						continue;

					if (PugDatabase.HasComponent<MineableDamageDecreaseCD>(objectData))
						continue;

					var entry = new DropsWhenDamaged {
						Result = (dropsLootWhenDamagedCD.dropsLoot, 0),
						Entity = (objectData.objectID, ObjectUtils.GetPrimaryVariation(objectData)),
						DamageRequiredToDrop = dropsLootWhenDamagedCD.damageToDealToDropLoot,
						HealthRequiredToDrop = dropsLootWhenDamagedCD.minHealthToDropLoot
					};
					registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.Entity.Id, entry.Entity.Variation, entry);
				}
			}
		}
	}
}