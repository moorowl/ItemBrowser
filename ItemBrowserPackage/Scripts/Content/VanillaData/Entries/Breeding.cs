using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Breeding : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Breeding", ObjectID.ValentineWallHearts, VanillaPriorities.Breeding);
		
		public ObjectID ParentType { get; set; }
		public ObjectID ChildType { get; set; }
		public int MealsRequired { get; set; }
		public float MutationChance { get; set; }
		public Dictionary<int, float> MutationOptions { get; set; } = new();

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, authoring) in allObjects) {
					if (!PugDatabase.TryGetComponent<BreedStateCD>(objectData, out var breedStateCD) || !authoring.TryGetComponent<BreedStateAuthoring>(out var breedStateAuthoring))
						continue;
					
					var mutationTotalWeight = breedStateAuthoring.mutationWeights.Sum(mutation => mutation.weight);

					var entry = new Breeding {
						ParentType = objectData.objectID,
						ChildType = breedStateCD.babyType,
						MealsRequired = breedStateCD.mealsToTrigger,
						MutationChance = breedStateAuthoring.mutationChance,
						MutationOptions = breedStateAuthoring.mutationWeights.ToDictionary(mutationWeight => mutationWeight.variation, mutationWeight => mutationWeight.weight / mutationTotalWeight)
					};
					registry.Register(ObjectEntryType.Source, entry.ChildType, 0, entry);
					registry.Register(ObjectEntryType.Usage, entry.ParentType, 0, entry);
				}
			}
		}
	}
}