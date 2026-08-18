using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Content.VanillaData.Entries {
	public class UpgradeMaterialDisplay : ObjectEntryDisplay<UpgradeMaterial> {
		public ItemBrowserSlot bigMaterialSlot;
		public ItemBrowserSlot[] smallMaterialSlots;
		public PugText levelText;
		
		protected override void OnRender(UpgradeMaterial entry) {
			var primaryMaterial = entry.PrimaryMaterial;
			bigMaterialSlot.Icon = new BasicSlotIcon(new ObjectDataCD {
				objectID = primaryMaterial.Id,
				amount = primaryMaterial.Amount
			});

			foreach (var slot in smallMaterialSlots)
				slot.gameObject.SetActive(false);

			var slotIndex = 0;
			foreach (var material in entry.OtherMaterials) {
				var slot = smallMaterialSlots[slotIndex];
				slot.gameObject.SetActive(true);
				slot.Icon = new BasicSlotIcon(new ObjectDataCD {
					objectID = material.Id,
					amount = material.Amount
				});
				
				slotIndex++;
				if (slotIndex >= smallMaterialSlots.Length)
					break;
			}
			
			levelText.Render($"{entry.Level.From} -> {entry.Level.To}");
		}

		protected override void OnRenderDescription(UpgradeMaterial entry, EntryDescriptionButton description) {
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/UpgradeMaterial_0",
				formatFields = new[] {
					entry.Level.From.ToString(),
					entry.Level.To.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});

			description.AddMaterials(GetMaterialsAsObjectWithAmountArray(entry));
		}

		private static ObjectWithAmount[] GetMaterialsAsObjectWithAmountArray(UpgradeMaterial entry) {
			var materials = new ObjectWithAmount[entry.OtherMaterials.Count + 1];
			materials[0] = new ObjectWithAmount {
				objectID = entry.PrimaryMaterial.Id,
				amount = entry.PrimaryMaterial.Amount
			};
			for (var i = 0; i < entry.OtherMaterials.Count; i++) {
				materials[i + 1] = new ObjectWithAmount {
					objectID = entry.OtherMaterials[i].Id,
					amount = entry.OtherMaterials[i].Amount
				};
			}

			return materials;
		}
	}
}