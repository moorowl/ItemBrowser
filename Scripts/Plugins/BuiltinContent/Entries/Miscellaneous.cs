using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record Miscellaneous : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Miscellaneous", ObjectID.GrimyStoneFloor, Priorities.Miscellaneous);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; set; }
		public (ObjectID Id, int Variation) Source { get; set; }
		public string Term { get; set; }

		public bool HasSource => Source.Id != ObjectID.None;

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// Creating a character with name Greggy
				AddEntry(new Miscellaneous {
					Result = (ObjectID.WallDirtBlock, 0, 10),
					Term = "ItemBrowser:MiscellaneousDesc/Greggy"
				});
				
				// Creating a creative character
				AddEntry(new Miscellaneous {
					Result = (ObjectID.OrbLantern, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/CreativeOrbLantern"
				});
				
				// Fallen rock event
				AddEntry(new Miscellaneous {
					Result = (ObjectID.FallingRockDestructible, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/FallenRockEvent"
				});
				
				// Omoroth's Tentacle event
				AddEntry(new Miscellaneous {
					Result = (ObjectID.OctopusTentacle, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/OctopusTentacleEvent"
				});
				
				// Bomb scarab event
				AddEntry(new Miscellaneous {
					Result = (ObjectID.BombScarab, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/BombScarabEvent"
				});
				
				// Snare plant
				AddEntry(new Miscellaneous {
					Result = (ObjectID.SnarePlant, 0, 1),
					Source = (ObjectID.CavelingGardener, 0),
					Term = "ItemBrowser:MiscellaneousDesc/SnarePlant"
				});
				
				// Core commander
				AddEntry(new Miscellaneous {
					Result = (ObjectID.CoreBoss, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/CoreBoss"
				});
				
				// Urschleim
				AddEntry(new Miscellaneous {
					Result = (ObjectID.WallBoss, 0, 1),
					Term = "ItemBrowser:MiscellaneousDesc/WallBoss"
				});
				
				// Boss adds
				// Hive Mother
				foreach (var boss in new[] { ObjectID.LarvaHiveBoss, ObjectID.LarvaHiveHalloweenBoss }) {
					AddEntry(new Miscellaneous {
						Result = (ObjectID.Larva, 1, 1),
						Source = (boss, 0),
						Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
					});
					AddEntry(new Miscellaneous {
						Result = (ObjectID.BigLarva, 1, 1),
						Source = (boss, 0),
						Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
					});
					AddEntry(new Miscellaneous {
						Result = (ObjectID.AcidLarva, 0, 1),
						Source = (boss, 0),
						Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
					});
				}
				
				// Ivy
				AddEntry(new Miscellaneous {
					Result = (ObjectID.PoisonSlimeBlob, 0, 1),
					Source = (ObjectID.PoisonSlimeBoss, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});
				
				// Omoroth
				AddEntry(new Miscellaneous {
					Result = (ObjectID.OctopusTentacle, 0, 1),
					Source = (ObjectID.OctopusBoss, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});
				
				// Ra-Akar
				AddEntry(new Miscellaneous {
					Result = (ObjectID.BombScarab, 0, 1),
					Source = (ObjectID.ScarabBoss, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});
				
				// Druidra
				AddEntry(new Miscellaneous {
					Result = (ObjectID.NatureWormSegment, 0, 1),
					Source = (ObjectID.HydraBossNature, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});
				
				// Core Commander
				AddEntry(new Miscellaneous {
					Result = (ObjectID.CoreBossOrb, 0, 3),
					Source = (ObjectID.CoreBoss, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});
				
				// Nimruza
				AddEntry(new Miscellaneous {
					Result = (ObjectID.CicadaNymph, 0, 1),
					Source = (ObjectID.GiantCicadaBoss, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});
				AddEntry(new Miscellaneous {
					Result = (ObjectID.DesertCicadaEnemy, 0, 1),
					Source = (ObjectID.GiantCicadaBoss, 0),
					Term = "ItemBrowser:MiscellaneousDesc/BossAdds"
				});

				void AddEntry(Miscellaneous entry) {
					registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
				}
			}
		}
	}
}