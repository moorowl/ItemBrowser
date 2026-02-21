using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record CreatureSummoning : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/CreatureSummoning", ObjectID.SlimeBossSummoningItem, VanillaPriorities.CreatureSummoning);
		
		public (ObjectID Id, int Variation) Creature { get; set; }
		public MethodType SummoningMethod { get; set; }
		public (ObjectID Id, int Variation) SummoningArea { get; set; }
		public (ObjectID Id, int Variation) SummoningItem { get; set; }

		public enum MethodType {
			SummonArea,
			LureWithBait,
			CastItem,
			Azeos,
			Omoroth,
			RaAkar,
			ActivatorConsole
		}
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// Azeos
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.BirdBoss, 0),
					SummoningMethod = MethodType.Azeos,
					SummoningItem = (ObjectID.LargeShinyGlimmeringObject, 0)
				});
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.BirdBoss, 1),
					SummoningMethod = MethodType.Azeos,
					SummoningItem = (ObjectID.EasterGoldenEgg, 0)
				});
				
				// Omoroth
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.OctopusBoss, 0),
					SummoningMethod = MethodType.Omoroth,
					SummoningItem = (ObjectID.BaitOctopusBoss, 0)
				});
				
				// Ra-Akar
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.ScarabBoss, 0),
					SummoningMethod = MethodType.RaAkar,
					SummoningItem = (ObjectID.Thumper, 0)
				});
				
				// LureWithBait
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.SnakeBossSegment, 0),
					SummoningMethod = MethodType.LureWithBait,
					SummoningItem = (ObjectID.BaitOnAPole, 0)
				});
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.HydraBossNature, 0),
					SummoningMethod = MethodType.LureWithBait,
					SummoningItem = (ObjectID.HydraBossNatureBait, 0)
				});
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.HydraBossSea, 0),
					SummoningMethod = MethodType.LureWithBait,
					SummoningItem = (ObjectID.HydraBossSeaBait, 0)
				});
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.HydraBossDesert, 0),
					SummoningMethod = MethodType.LureWithBait,
					SummoningItem = (ObjectID.HydraBossDesertBait, 0)
				});
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.HydraBossVoid, 0),
					SummoningMethod = MethodType.LureWithBait,
					SummoningItem = (ObjectID.HydraBossVoidBait, 0)
				});
				RegisterEntry(new CreatureSummoning {
					Creature = (ObjectID.RobotBoss, 0),
					SummoningMethod = MethodType.ActivatorConsole,
					SummoningArea = (ObjectID.RobotBossActivatorConsole, 0),
					SummoningItem = (ObjectID.RobotBossSummoningItem, 0)
				});
				
				// CastItem
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<ScannerCD>(objectData, out var scannerCD) || !PugDatabase.TryGetComponent<CastItemCD>(objectData, out var castItemCD))
						continue;

					if (scannerCD.summonInsteadOfScan && castItemCD.useType == CastItemUseType.SummonEntity) {
						RegisterEntry(new CreatureSummoning {
							Creature = (scannerCD.objectToScan, 0),
							SummoningMethod = MethodType.CastItem,
							SummoningItem = (objectData.objectID, objectData.variation)
						});
					}
				}

				// SummonArea
				var creatureToSummonArea = GetSummonAreaLookup(allObjects);
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<SummoningItemBuffer>(objectData))
						continue;

					foreach (var summoningItem in PugDatabase.GetBuffer<SummoningItemBuffer>(objectData)) {
						if (!creatureToSummonArea.TryGetValue(summoningItem.bossToSummon, out var associatedSummonAreas))
							continue;

						foreach (var summonArea in associatedSummonAreas) {
							RegisterEntry(new CreatureSummoning {
								Creature = (summoningItem.bossToSummon, 0),
								SummoningMethod = MethodType.SummonArea,
								SummoningArea = (summonArea.ObjectData.objectID, summonArea.ObjectData.variation),
								SummoningItem = (objectData.objectID, 0)
							});
						}
					}
				}

				void RegisterEntry(CreatureSummoning entry) {
					registry.Register(ObjectEntryType.Source, entry.Creature.Id, entry.Creature.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.SummoningItem.Id, entry.SummoningItem.Variation, entry);
					if (entry.SummoningArea.Id != ObjectID.None)
						registry.Register(ObjectEntryType.Usage, entry.SummoningArea.Id, entry.SummoningArea.Variation, entry);
				}
			}

			private static Dictionary<ObjectID, List<(ObjectDataCD ObjectData, SummonAreaCD SummonArea)>> GetSummonAreaLookup(List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var creatureToSummonArea = new Dictionary<ObjectID, List<(ObjectDataCD ObjectData, SummonAreaCD SummonArea)>>();
				
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<SummonAreaCD>(objectData, out var summonAreaCD))
						continue;
					
					if (!creatureToSummonArea.ContainsKey(summonAreaCD.bossToSummon))
						creatureToSummonArea[summonAreaCD.bossToSummon] = new List<(ObjectDataCD, SummonAreaCD)>();
					creatureToSummonArea[summonAreaCD.bossToSummon].Add((objectData, summonAreaCD));

					if (summonAreaCD.optionalBossToSummon == ObjectID.None)
						continue;

					if (!creatureToSummonArea.ContainsKey(summonAreaCD.optionalBossToSummon))
						creatureToSummonArea[summonAreaCD.optionalBossToSummon] = new List<(ObjectDataCD, SummonAreaCD)>();
					creatureToSummonArea[summonAreaCD.optionalBossToSummon].Add((objectData, summonAreaCD));
				}

				return creatureToSummonArea;
			}
		}
	}
}