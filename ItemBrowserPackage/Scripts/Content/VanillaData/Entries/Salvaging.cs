using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Salvaging : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Salvaging", ObjectID.SalvageAndRepairStation, VanillaPriorities.Salvaging);
		
		public ObjectID Result { get; set; }
		public (int Min, int Max) ResultAmount { get; set; }
		public ObjectID ItemSalvaged { get; set; }

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
					if (objectInfo == null)
						continue;

					var hasDurability = PugDatabase.TryGetComponent<DurabilityCD>(objectData, out var durabilityCD);
					var hasLevel = PugDatabase.TryGetComponent<LevelCD>(objectData, out var levelCD);

					if (!objectInfo.tags.Contains(ObjectCategoryTag.CanBeSalvaged) || objectInfo.rarity == Rarity.Legendary)
						continue;

					var totalScrapParts = 1;
					if (hasDurability && hasLevel) {
						var scrapPartsMultiplier = PugDatabase.GetComponent<DurabilityCD>(objectData).repairCostMultiplier * 2f;
						totalScrapParts = (int) math.max(1f, math.round(levelCD.level * 2 * scrapPartsMultiplier));
					}
					
					var scrapPartsEntry = new Salvaging {
						Result = ObjectID.ScrapPart,
						ResultAmount = (totalScrapParts, totalScrapParts),
						ItemSalvaged = objectData.objectID
					};
					registry.Register(ObjectEntryType.Usage, scrapPartsEntry.ItemSalvaged, 0, scrapPartsEntry);

					for (var i = 0; i < objectInfo.requiredObjectsToCraft.Count; i++) {
						var craftingObject = objectInfo.requiredObjectsToCraft[i];

						var minAmount = (int) (craftingObject.amount * (Constants.minMaterialToGainFromSalvage * objectInfo.salvageMultiplier));
						var maxAmount = (int) (craftingObject.amount * (Constants.maxMaterialToGainFromSalvage * objectInfo.salvageMultiplier));
						if (!hasDurability || !hasLevel)
							minAmount = maxAmount;

						maxAmount++;

						var materialEntry = new Salvaging {
							Result = craftingObject.objectID,
							ResultAmount = (minAmount, maxAmount),
							ItemSalvaged = objectData.objectID
						};
						registry.Register(ObjectEntryType.Source, materialEntry.Result, 0, materialEntry);
						registry.Register(ObjectEntryType.Usage, materialEntry.ItemSalvaged, 0, materialEntry);
					}
				}
			}
		}
	}
}