using System.Collections.Generic;
using HarmonyLib;
using ItemBrowser.Common.Api;
using Pug.UnityExtensions;

// ReSharper disable once InconsistentNaming

namespace ItemBrowser.Common.UserInterface.Highlights {
	public static class HighlightSlots {
		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(UIMouse), "UpdateSlotHighlights")]
			[HarmonyPostfix]
			private static void UIMouse_UpdateSlotHighlights(UIMouse __instance) {
				if (ItemBrowserAPI.ItemBrowserUI != null) {
					var objectsToHighlight = ItemBrowserAPI.ItemBrowserUI.ObjectsToHighlightInInventory;

					if (Manager.ui.isChestInventoryUIShowing)
						TryHighlightItemSlots(Manager.ui.chestInventoryUI.itemSlots, objectsToHighlight);

					if (Manager.ui.isPlayerInventoryShowing) {
						TryHighlightItemSlots(Manager.ui.playerInventoryUI.itemSlots, objectsToHighlight);

						foreach (var pouchInventory in ((InventoryUI)Manager.ui.playerInventoryUI).pouchSlotsContainers)
							TryHighlightItemSlots(pouchInventory.itemSlots, objectsToHighlight);
					}

					if (Manager.ui.itemSlotsBar.isShowing)
						TryHighlightItemSlots(Manager.ui.itemSlotsBar.itemSlots, objectsToHighlight);
				}
			}

			private static void TryHighlightItemSlots(List<SlotUIBase> itemSlots, HashSet<ObjectID> objectsToHighlight) {
				foreach (var slot in itemSlots) {
					if (slot.highlightBorder == null || slot.icon == null || !slot.isShowing)
						continue;

					var highlightAnyItem = objectsToHighlight.Count > 0;
					var highlightThisItem = objectsToHighlight.Contains(slot.GetContainedObject().objectID);

					if (highlightAnyItem) {
						slot.highlightBorder.gameObject.SetActive(highlightThisItem);
						slot.icon.SetAlpha(highlightThisItem ? 1f : 0.2f);
					}
					else {
						slot.highlightBorder.gameObject.SetActive(false);
						slot.icon.SetAlpha(1f);
					}
				}
			}
		}
	}
}