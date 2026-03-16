using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using Pug.Properties;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Farming : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Farming", ObjectID.HeartBerrySeed, VanillaPriorities.Farming);
		
		public ObjectID Result { get; set; }
		public (ObjectID Id, int Variation) Seed { get; set; }
		public bool HasGoldSeed  { get; set; }
		public bool RequiresGoldSeed { get; set; }
		public float GrowthTime { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!ObjectUtils.IsPrimaryVariation(objectData) || !PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var objectPropertiesCD))
						continue;

					if (!objectPropertiesCD.Has(PropertyID.isSeed))
						continue;

					var turnsIntoPlant = objectPropertiesCD.Get<ObjectID>(PropertyID.Seed.turnsIntoPlantID);
					var turnsIntoPlantVariationRare = objectPropertiesCD.Get<int>(PropertyID.Seed.rarePlantVariation);
					var isPersistentGoldenSeed = objectData.variation == 2;

					var timeBetweenStages = objectPropertiesCD.Get<float>(PropertyID.Growing.timeBetweenStages);;
					var highestStage = objectPropertiesCD.Get<int>(PropertyID.Growing.highestStage);
					var growthTime = timeBetweenStages * highestStage;
					
					if (!PugDatabase.TryGetComponent<PlantCD>(turnsIntoPlant, out var plantCD) || !PugDatabase.TryGetComponent<ObjectPropertiesCD>(turnsIntoPlant, out var plantObjectPropertiesCD))
						continue;
					
					var plantTimeBetweenStages = plantObjectPropertiesCD.Get<float>(PropertyID.Growing.timeBetweenStages);;
					var plantHighestStage = plantObjectPropertiesCD.Get<int>(PropertyID.Growing.highestStage);
					growthTime += plantTimeBetweenStages * plantHighestStage;

					if (!isPersistentGoldenSeed) {
						var normalEntry = new Farming {
							Result = plantCD.objectToDropWhenHarvested,
							Seed = (objectData.objectID, objectData.variation),
							RequiresGoldSeed = false,
							HasGoldSeed = turnsIntoPlantVariationRare > 0,
							GrowthTime = growthTime
						};
						registry.Register(ObjectEntryType.Source, normalEntry.Result, 0, normalEntry);
						registry.Register(ObjectEntryType.Usage, normalEntry.Seed.Id, normalEntry.Seed.Variation, normalEntry);
					}

					if (turnsIntoPlantVariationRare > 0 && PugDatabase.TryGetComponent<PlantCD>(new ObjectData { objectID = turnsIntoPlant, variation = turnsIntoPlantVariationRare }, out var rarePlantCD)) {
						var goldEntry = new Farming {
							Result = rarePlantCD.objectToDropWhenHarvested,
							Seed = (objectData.objectID, objectData.variation),
							RequiresGoldSeed = true,
							HasGoldSeed = !isPersistentGoldenSeed,
							GrowthTime = growthTime
						};
						registry.Register(ObjectEntryType.Source, goldEntry.Result, 0, goldEntry);
						registry.Register(ObjectEntryType.Usage, goldEntry.Seed.Id, goldEntry.Seed.Variation, goldEntry);
					}
				}
			}
		}
	}
}