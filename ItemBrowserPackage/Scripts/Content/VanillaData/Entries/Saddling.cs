using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Saddling : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Saddling", ObjectID.Saddle, VanillaPriorities.Saddling);
		
		public ObjectID NormalType { get; set; }
		public ObjectID SaddledType { get; set; }
		public List<ObjectID> Saddles { get; set; } = new();

		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var saddleTypes = allObjects
					.Where(entry => entry.ObjectData.variation == 0 && PugDatabase.HasComponent<SaddleSpecificMountModifierCD>(entry.ObjectData))
					.Select(entry => entry.ObjectData.objectID)
					.ToList();
				
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<SaddleableCD>(objectData, out var saddleableCD) || saddleableCD.turnsIntoObjectID == ObjectID.None)
						continue;

					var entry = new Saddling {
						NormalType = objectData.objectID,
						SaddledType = saddleableCD.turnsIntoObjectID,
						Saddles = saddleTypes
					};
					registry.Register(ObjectEntryType.Usage, entry.NormalType, 0, entry);
					registry.Register(ObjectEntryType.Source, entry.SaddledType, 0, entry);
					
					foreach (var saddleType in saddleTypes)
						registry.Register(ObjectEntryType.Usage, saddleType, 0, entry);
				}
			}
		}
	}
}