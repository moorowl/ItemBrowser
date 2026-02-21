using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Unlocking : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Unlocking", ObjectID.CopperKey, VanillaPriorities.Unlocking);
		
		public (ObjectID Id, int Variation) Key { get; set; }
		public (ObjectID Id, int Variation) OutputObject { get; set; }
		public (ObjectID Id, int Variation) InputObject { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!ObjectUtils.IsPrimaryVariation(objectData.objectID, objectData.variation))
						continue;
					
					if (!PugDatabase.TryGetComponent<ChangeVariationWhenContainingObjectCD>(objectData, out var changeVariationWhenContainingObjectCD))
						continue;

					if (changeVariationWhenContainingObjectCD.reinstantiateToNewObjectId == ObjectID.None)
						continue;

					var entry = new Unlocking {
						Key = (changeVariationWhenContainingObjectCD.objectID, 0),
						OutputObject = (
							changeVariationWhenContainingObjectCD.reinstantiateToNewObjectId,
							ObjectUtils.GetPrimaryVariation(changeVariationWhenContainingObjectCD.reinstantiateToNewObjectId, changeVariationWhenContainingObjectCD.variationToChangeTo)
						),
						InputObject = (objectData.objectID, objectData.variation),
					};
					registry.Register(ObjectEntryType.Source, entry.OutputObject.Id, entry.OutputObject.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.InputObject.Id, entry.InputObject.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.Key.Id, entry.Key.Variation, entry);
				}
			}
		}
	}
}