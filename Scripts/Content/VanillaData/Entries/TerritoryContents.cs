using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record TerritoryContents : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/TerritoryContents", "ItemBrowser-ObjectEntryNames/TerritoryContents_NonObtainable", ObjectID.GroundSlipperySlime, VanillaPriorities.TerritoryContents);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public TerritoryType Territory { get; set; }

		public enum TerritoryType {
			OrangeSlime,
			PurpleSlime,
			BlueSlime,
			Larva,
			Caveling,
			NatureCaveling
		}
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				AddEntry(TerritoryType.OrangeSlime, ObjectID.GroundSlime, ObjectID.SlimeBlob);
				AddEntry(TerritoryType.PurpleSlime, ObjectID.GroundPoisonSlime, ObjectID.PoisonSlimeBlob);
				AddEntry(TerritoryType.BlueSlime, ObjectID.GroundSlipperySlime, ObjectID.SlipperySlimeBlob);
				AddEntry(TerritoryType.Larva, ObjectID.Chrysalis, ObjectID.Cocoon, ObjectID.Larva, ObjectID.BigLarva);
				AddEntry(TerritoryType.Caveling, ObjectID.StoneCavelingMoss, ObjectID.Caveling, ObjectID.CavelingShaman, ObjectID.CavelingBrute);
				AddEntry(TerritoryType.NatureCaveling, ObjectID.NatureCavelingMoss, ObjectID.CavelingGardener, ObjectID.CavelingHunter);

				return;

				void AddEntry(TerritoryType territory, params ObjectID[] contents) {
					foreach (var id in contents) {
						var entry = new TerritoryContents {
							Result = (id, 0),
							Territory = territory
						};
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					}
				}
			}
		}
	}
}