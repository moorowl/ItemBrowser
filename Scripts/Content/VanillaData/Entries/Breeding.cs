using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Breeding : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Breeding", ObjectID.ValentineWallHearts, VanillaPriorities.Breeding);
		
		public ObjectID ParentType { get; set; }
		public ObjectID ChildType { get; set; }
		public int MealsRequired { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<BreedStateCD>(objectData, out var breedStateCD))
						continue;

					var entry = new Breeding {
						ParentType = objectData.objectID,
						ChildType = breedStateCD.babyType,
						MealsRequired = breedStateCD.mealsToTrigger
					};
					registry.Register(ObjectEntryType.Source, entry.ChildType, 0, entry);
					registry.Register(ObjectEntryType.Usage, entry.ParentType, 0, entry);
				}
			}
		}
	}
}