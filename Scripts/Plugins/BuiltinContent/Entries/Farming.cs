using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using PugProperties;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record Farming : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Farming", ObjectID.HeartBerrySeed, Priorities.Farming);
		
		public ObjectID Result { get; set; }
		public ObjectID Seed { get; set; }
		public bool HasGoldSeed  { get; set; }
		public bool RequiresGoldSeed { get; set; }
		public float GrowthTime { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (objectData.variation != 0 || !PugDatabase.TryGetComponent<ObjectPropertiesCD>(objectData, out var objectPropertiesCD))
						continue;

					if (!objectPropertiesCD.Has(PropertyID.isSeed))
						continue;

					var turnsIntoPlant = objectPropertiesCD.Get<ObjectID>(PropertyID.Seed.turnsIntoPlantID);
					var turnsIntoPlantVariationRare = objectPropertiesCD.Get<int>(PropertyID.Seed.rarePlantVariation);

					var timeBetweenStages = objectPropertiesCD.Get<float>(PropertyID.Growing.timeBetweenStages);;
					var highestStage = objectPropertiesCD.Get<int>(PropertyID.Growing.highestStage);
					var growthTime = timeBetweenStages * highestStage;
					
					if (!PugDatabase.TryGetComponent<PlantCD>(turnsIntoPlant, out var plantCD) || !PugDatabase.TryGetComponent<ObjectPropertiesCD>(turnsIntoPlant, out var plantObjectPropertiesCD))
						continue;
					
					var plantTimeBetweenStages = plantObjectPropertiesCD.Get<float>(PropertyID.Growing.timeBetweenStages);;
					var plantHighestStage = plantObjectPropertiesCD.Get<int>(PropertyID.Growing.highestStage);
					growthTime += plantTimeBetweenStages * plantHighestStage;
					
					var normalEntry = new Farming {
						Result = plantCD.objectToDropWhenHarvested,
						Seed = objectData.objectID,
						RequiresGoldSeed = false,
						HasGoldSeed = turnsIntoPlantVariationRare > 0,
						GrowthTime = growthTime
					};
					registry.Register(ObjectEntryType.Source, normalEntry.Result, 0, normalEntry);
					registry.Register(ObjectEntryType.Usage, normalEntry.Seed, 0, normalEntry);

					if (turnsIntoPlantVariationRare > 0 && PugDatabase.TryGetComponent<PlantCD>(new ObjectData { objectID = turnsIntoPlant, variation = turnsIntoPlantVariationRare }, out var rarePlantCD)) {
						var goldEntry = new Farming {
							Result = rarePlantCD.objectToDropWhenHarvested,
							Seed = objectData.objectID,
							RequiresGoldSeed = true,
							HasGoldSeed = true,
							GrowthTime = growthTime
						};
						registry.Register(ObjectEntryType.Source, goldEntry.Result, 0, goldEntry);
						registry.Register(ObjectEntryType.Usage, goldEntry.Seed, 0, goldEntry);
					}
				}
			}
		}
	}
}