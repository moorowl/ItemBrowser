using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record OreBoulderExtraction : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/OreBoulderExtraction", ObjectID.GoldOreBoulder, VanillaPriorities.OreBoulderExtraction);
		
		public ObjectID Result { get; set; }
		public ObjectID OreBoulder { get; set; }
		public int TotalOre { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<RequiresDrillCD>(objectData) || !PugDatabase.TryGetComponent<DropsLootWhenDamagedCD>(objectData, out var dropsLootWhenDamagedCD) || !PugDatabase.TryGetComponent<HealthCD>(objectData, out var healthCD))
						continue;
					
					if (dropsLootWhenDamagedCD.minHealthToDropLoot != 0)
						continue;

					var entry = new OreBoulderExtraction {
						Result = dropsLootWhenDamagedCD.dropsLoot,
						OreBoulder = objectData.objectID,
						TotalOre = (int) math.floor(healthCD.maxHealth / (float) dropsLootWhenDamagedCD.damageToDealToDropLoot)
					};
					registry.Register(ObjectEntryType.Source, entry.Result, 0, entry);
					registry.Register(ObjectEntryType.Usage, entry.OreBoulder, 0, entry);
				}
			}
		}
	}
}