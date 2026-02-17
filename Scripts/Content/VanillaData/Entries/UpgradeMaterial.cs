using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record UpgradeMaterial : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/UpgradeMaterial", ObjectID.UpgradeForge, VanillaPriorities.UpgradeMaterial);
		
		public (int From, int To) Level { get; set; }
		public (ObjectID Id, int Amount) PrimaryMaterial { get; set; }
		public List<(ObjectID Id, int Amount)> OtherMaterials { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				var upgradeCostsTable = API.Client.GetEntityQuery(typeof(UpgradeCostsTableCD)).GetSingleton<UpgradeCostsTableCD>();

				for (var i = 2; i <= LevelScaling.GetMaxLevel(); i++) {
					ref var upgradeCosts = ref upgradeCostsTable.GetUpgradeCost(i);
					
					var allMaterials = upgradeCosts.ConvertToList().Select(blob => (blob.item, blob.amount)).ToList();
					
					foreach (var material in allMaterials) {
						var entry = new UpgradeMaterial {
							Level = (i - 1, i),
							PrimaryMaterial = material,
							OtherMaterials = allMaterials.Where(x => x != material).ToList()
						};
						registry.Register(ObjectEntryType.Usage, entry.PrimaryMaterial.Id, 0, entry);
					}
				}
			}
		}
	}
}