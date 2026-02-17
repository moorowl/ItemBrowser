using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record VendingMachine : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/VendingMachine", ObjectID.AFVendingMachine, VanillaPriorities.VendingMachine);
		
		public ObjectID Result { get; set; }
		public ObjectID Vendor { get; set; }
		public int Stock { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<VendingMachineItemBuffer>(objectData))
						continue;

					foreach (var info in PugDatabase.GetBuffer<VendingMachineItemBuffer>(objectData)) {
						if (info.objectID == ObjectID.None)
							continue;
						
						var entry = new VendingMachine {
							Result = info.objectID,
							Vendor = objectData.objectID,
							Stock = math.max(objectData.amount, 1)
						};
						registry.Register(ObjectEntryType.Source, entry.Result, 0, entry);
						registry.Register(ObjectEntryType.Usage, entry.Vendor, 0, entry);
					}
				}
			}
		}
	}
}