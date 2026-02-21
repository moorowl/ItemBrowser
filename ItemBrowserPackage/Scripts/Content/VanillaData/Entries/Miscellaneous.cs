using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Miscellaneous : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Miscellaneous", ObjectID.GrimyStoneFloor, VanillaPriorities.Miscellaneous);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; set; }
		public (ObjectID Id, int Variation) Source { get; set; }
		public string Term { get; set; }

		public bool HasSource => Source.Id != ObjectID.None;

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// Creating a character with name Greggy
				AddEntry(new Miscellaneous {
					Result = (ObjectID.WallDirtBlock, 0, 10),
					Term = "ItemBrowser-Miscellaneous/Greggy"
				});
				
				// Creating a creative character
				AddEntry(new Miscellaneous {
					Result = (ObjectID.OrbLantern, 0, 1),
					Term = "ItemBrowser-Miscellaneous/CreativeOrbLantern"
				});
				
				// Fallen rock event
				AddEntry(new Miscellaneous {
					Result = (ObjectID.FallingRockDestructible, 0, 1),
					Term = "ItemBrowser-Miscellaneous/FallenRockEvent"
				});
				AddEntry(new Miscellaneous {
					Result = (ObjectID.FallingExcavationRockDestructible, 0, 1),
					Term = "ItemBrowser-Miscellaneous/FallenRockEvent"
				});
				
				// Omoroth's Tentacle event
				AddEntry(new Miscellaneous {
					Result = (ObjectID.OctopusTentacle, 0, 1),
					Term = "ItemBrowser-Miscellaneous/OctopusTentacleEvent"
				});
				
				// Bomb scarab event
				AddEntry(new Miscellaneous {
					Result = (ObjectID.BombScarab, 0, 1),
					Term = "ItemBrowser-Miscellaneous/BombScarabEvent"
				});
				
				// Snare plant
				AddEntry(new Miscellaneous {
					Result = (ObjectID.SnarePlant, 0, 1),
					Source = (ObjectID.CavelingGardener, 0),
					Term = "ItemBrowser-Miscellaneous/SnarePlant"
				});
				
				// Core commander
				AddEntry(new Miscellaneous {
					Result = (ObjectID.CoreBoss, 0, 1),
					Term = "ItemBrowser-Miscellaneous/CoreBoss"
				});
				
				// Urschleim
				AddEntry(new Miscellaneous {
					Result = (ObjectID.WallBoss, 0, 1),
					Term = "ItemBrowser-Miscellaneous/WallBoss"
				});
				
				// Boss adds
				// Hive Mother
				foreach (var boss in new[] { ObjectID.LarvaHiveBoss, ObjectID.LarvaHiveHalloweenBoss }) {
					AddEntry(new Miscellaneous {
						Result = (ObjectID.Larva, 1, 1),
						Source = (boss, 0),
						Term = "ItemBrowser-Miscellaneous/BossAdds"
					});
					AddEntry(new Miscellaneous {
						Result = (ObjectID.BigLarva, 1, 1),
						Source = (boss, 0),
						Term = "ItemBrowser-Miscellaneous/BossAdds"
					});
					AddEntry(new Miscellaneous {
						Result = (ObjectID.AcidLarva, 0, 1),
						Source = (boss, 0),
						Term = "ItemBrowser-Miscellaneous/BossAdds"
					});
				}
				
				// Ivy
				AddEntry(new Miscellaneous {
					Result = (ObjectID.PoisonSlimeBlob, 0, 1),
					Source = (ObjectID.PoisonSlimeBoss, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});
				
				// Omoroth
				AddEntry(new Miscellaneous {
					Result = (ObjectID.OctopusTentacle, 0, 1),
					Source = (ObjectID.OctopusBoss, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});
				
				// Ra-Akar
				AddEntry(new Miscellaneous {
					Result = (ObjectID.BombScarab, 0, 1),
					Source = (ObjectID.ScarabBoss, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});
				
				// Druidra
				AddEntry(new Miscellaneous {
					Result = (ObjectID.NatureWormSegment, 0, 1),
					Source = (ObjectID.HydraBossNature, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});
				
				// Core Commander
				AddEntry(new Miscellaneous {
					Result = (ObjectID.CoreBossOrb, 0, 3),
					Source = (ObjectID.CoreBoss, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});
				
				// Nimruza
				AddEntry(new Miscellaneous {
					Result = (ObjectID.CicadaNymph, 0, 1),
					Source = (ObjectID.GiantCicadaBoss, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});
				AddEntry(new Miscellaneous {
					Result = (ObjectID.DesertCicadaEnemy, 0, 1),
					Source = (ObjectID.GiantCicadaBoss, 0),
					Term = "ItemBrowser-Miscellaneous/BossAdds"
				});

				void AddEntry(Miscellaneous entry) {
					registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
				}
			}
		}
	}
}