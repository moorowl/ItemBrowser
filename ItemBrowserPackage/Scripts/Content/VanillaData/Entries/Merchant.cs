using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record Merchant : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/Merchant", ObjectID.SlimeOil, VanillaPriorities.Merchant);
		
		public ObjectID Result { get; set; }
		public ObjectID MerchantType { get; set; }
		public int Stock { get; set; }
		public MerchantItemRequirement Requirement { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<MerchantItemInfoBuffer>(objectData))
						continue;

					foreach (var info in PugDatabase.GetBuffer<MerchantItemInfoBuffer>(objectData)) {
						if (info.objectID == ObjectID.None)
							continue;
						
						var entry = new Merchant {
							Result = info.objectID,
							MerchantType = objectData.objectID,
							Stock = math.max(info.amount, 1),
							Requirement = info.requirementToBeAvailable
						};
						registry.Register(ObjectEntryType.Source, entry.Result, 0, entry);
						registry.Register(ObjectEntryType.Usage, entry.MerchantType, 0, entry);
					}
				}
			}
		}
	}
}