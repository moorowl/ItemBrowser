using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record SeedExtracting : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/SeedExtracting", ObjectID.SeedExtractor, VanillaPriorities.SeedExtracting);
		
		public (ObjectID Id, int Variation) Extracted { get; set; }
		public (int Min, int Max) ExtractedAmount { get; set; }
		public (ObjectID Id, int Variation) Plant { get; set; }
		public (ObjectID Id, int Variation) Extractor { get; set; }
		public float TimeToExtract { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<ExtractorCD>(objectData, out var extractorCD) || extractorCD.extractableType != ObjectCategoryTag.SeedExtractable)
						continue;

					foreach (var extractable in ObjectUtility.GetAllObjectsWithTag(ObjectCategoryTag.SeedExtractable)) {
						if (!PugDatabase.TryGetComponent<ExtractableCD>(extractable, out var extractableCD))
							continue;

						ref var extractableData = ref extractableCD.extractableData.Value;
						ref var extractableOutputs = ref extractableCD.extractedObjectOutputArray.Value;

						for (var i = 0; i < extractableOutputs.Length; i++) {
							var minMaxRandomAmountOverride = extractableOutputs[i].minMaxRandomAmountOverride;
							
							var entry = new SeedExtracting {
								Extracted = (extractableOutputs[i].objectID, extractableOutputs[i].variation),
								ExtractedAmount = minMaxRandomAmountOverride.y > 0f
									? ((int) minMaxRandomAmountOverride.x, (int) minMaxRandomAmountOverride.y)
									: ((int) extractorCD.defaultMinMaxRandomExtractedOutputAmount.x, (int) extractorCD.defaultMinMaxRandomExtractedOutputAmount.y),
								TimeToExtract = extractableData.craftingTimeOverride > 0 ? extractableData.craftingTimeOverride : extractorCD.defaultExtractionTime,
								Plant = (extractable.objectID, extractable.variation),
								Extractor = (objectData.objectID, objectData.variation)
							};	
							registry.Register(ObjectEntryType.Source, entry.Extracted.Id, entry.Extracted.Variation, entry);
							registry.Register(ObjectEntryType.Usage, entry.Plant.Id, entry.Plant.Variation, entry);
							registry.Register(ObjectEntryType.Usage, entry.Extractor.Id, entry.Extractor.Variation, entry);
						}
					}
				}
			}
		}
	}
}