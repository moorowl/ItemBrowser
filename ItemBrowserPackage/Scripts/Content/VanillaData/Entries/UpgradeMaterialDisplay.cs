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
			
			// "Materials" header
			description.AddPadding();
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/UpgradeMaterial_1",
				color = UserInterfaceUtility.DescriptionColor
			});
			
			// Materials list
			description.AddLine(new TextAndFormatFields {
				text = "ItemBrowser-ObjectEntryDescriptions/UpgradeMaterial_2",
				formatFields = new[] {
					ObjectUtility.GetLocalizedDisplayNameOrDefault(entry.PrimaryMaterial.Id),
					entry.PrimaryMaterial.Amount.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtility.DescriptionColor
			});
			
			foreach (var material in entry.OtherMaterials) {
				description.AddLine(new TextAndFormatFields {
					text = "ItemBrowser-ObjectEntryDescriptions/UpgradeMaterial_2",
					formatFields = new[] {
						ObjectUtility.GetLocalizedDisplayNameOrDefault(material.Id),
						material.Amount.ToString()
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				});
			}
		}
	}
}