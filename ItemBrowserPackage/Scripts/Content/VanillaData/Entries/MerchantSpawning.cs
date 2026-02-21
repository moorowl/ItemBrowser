using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record MerchantSpawning : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/MerchantSpawning", ObjectID.WallWoodBlock, VanillaPriorities.MerchantSpawning);
		
		public ObjectID Merchant { get; set; }
		public ObjectID Idol  { get; set; }
		
		public class Provider : ObjectEntryProvider {
			private static readonly MemberInfo MiGetRequiredObjectForMerchant = typeof(SpawnMerchantSystem).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "GetRequiredObjectForMerchant");
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.HasComponent<MerchantCD>(objectData))
						continue;

					var summoningItem = (ObjectID) API.Reflection.Invoke(MiGetRequiredObjectForMerchant, null, objectData.objectID);
					if (summoningItem == ObjectID.None)
						continue;

					var entry = new MerchantSpawning {
						Merchant = objectData.objectID,
						Idol = summoningItem
					};
					registry.Register(ObjectEntryType.Source, entry.Merchant, 0, entry);
					registry.Register(ObjectEntryType.Usage, entry.Idol, 0, entry);
				}
			}
		}
	}
}