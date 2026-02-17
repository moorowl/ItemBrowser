using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Bucketing : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Bucketing", ObjectID.Bucket, VanillaPriorities.Bucketing);
		
		public (ObjectID Id, int Variation) EmptyBucket { get; set; }
		public (ObjectID Id, int Variation) FilledBucket { get; set; }
		public Tileset LiquidType { get; set; }
		public bool IsEmptying { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (objectData.variation == 0)
						continue;
					
					var objectType = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).objectType;
					if (objectType != ObjectType.Bucket)
						continue;

					var liquidType = (Tileset) objectData.variation - 1;

					var entry = new Bucketing {
						EmptyBucket = (objectData.objectID, 0),
						FilledBucket = (objectData.objectID, objectData.variation),
						LiquidType = liquidType
					};
					registry.Register(ObjectEntryType.Source, entry.FilledBucket.Id, entry.FilledBucket.Variation, entry);
					registry.Register(ObjectEntryType.Usage, entry.EmptyBucket.Id, entry.EmptyBucket.Variation, entry);
					registry.Register(ObjectEntryType.Source, entry.EmptyBucket.Id, entry.EmptyBucket.Variation, entry with {
						IsEmptying = true
					});
					registry.Register(ObjectEntryType.Usage, entry.FilledBucket.Id, entry.FilledBucket.Variation, entry with {
						IsEmptying = true
					});
				}
			}
		}
	}
}