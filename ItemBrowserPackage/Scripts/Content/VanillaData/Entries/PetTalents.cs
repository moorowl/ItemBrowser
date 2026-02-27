using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record PetTalents : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/PetTalents", ObjectID.PetCandyRare, VanillaPriorities.PetTalents);
		
		public ObjectID Pet { get; set; }
		public PetTalent Talent { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<PetTalentPoolBuffer>(objectData))
						continue;

					foreach (var petTalentPool in PugDatabase.GetBuffer<PetTalentPoolBuffer>(objectData)) {
						var entry = new PetTalents {
							Pet = objectData.objectID,
							Talent = petTalentPool.petTalentID
						};
						registry.Register(ObjectEntryType.Usage, objectData, entry);
					}
				}
			}
		}
	}
}