using System;
using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Trading : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Trading", ObjectID.ValentineLetter, VanillaPriorities.Trading);
		
		public (ObjectID Id, int Variation) Result { get; set; }
		public (ObjectID Id, int Variation) Vendor { get; set; }
		public int Amount { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CraftingCD>(objectData, out var craftingCD) || !PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData))
						continue;

					if (craftingCD.craftingType != CraftingType.Simple)
						continue;

					if (PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType != ObjectType.Creature)
						continue;
					
					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					foreach (var canCraftObject in canCraftObjects) {
						var objectInfo = PugDatabase.GetObjectInfo(canCraftObject.objectID);
						if (objectInfo == null)
							continue;

						var entry = new Trading {
							Result = (canCraftObject.objectID, 0),
							Vendor = (objectData.objectID, objectData.variation),
							Amount = Math.Max(canCraftObject.amount, 1)
						};
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
						registry.Register(ObjectEntryType.Usage, entry.Vendor.Id, entry.Vendor.Variation, entry);
						foreach (var ingredient in ObjectUtility.GroupAndSumObjects(objectInfo.requiredObjectsToCraft))
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
						foreach (var ingredient in ObjectUtility.GetAllObjectsWithTag(objectInfo.craftingSettings.canOnlyUseAnyMaterialsWithTag))
							registry.Register(ObjectEntryType.Usage, ingredient.objectID, 0, entry);
					}
				}
			}
		}
	}
}