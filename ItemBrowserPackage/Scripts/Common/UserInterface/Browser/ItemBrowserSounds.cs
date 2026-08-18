using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public static class ItemBrowserSounds {
		public static void PlayGenericOpen() {
			AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.6f, false, 1f, 0f);
		}

		public static void PlayGenericClose() {
			AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f, false, 1f, 0f);
		}

		public static void PlayChangeTabOrCategory(Component source) {
			AudioManager.Sfx(SfxTableID.inventorySFXCreativeModeCategory, source.transform.position);
		}

		public static void PlayAddObjectToInventory(Component source) {
			AudioManager.Sfx(SfxID.twitch, source.transform.position, 0.1f, 0.55f, 0.1f, true);
		}

		public static void PlayFavorited(Component source) {
			AudioManager.Sfx(SfxTableID.inventorySFXSlotUnlock, source.transform.position);
		}

		public static void PlayUnfavorited(Component source) {
			AudioManager.Sfx(SfxTableID.inventorySFXSlotLock, source.transform.position);
		}

		public static void PlayToggleBrowser() {
			AudioManager.Sfx(SfxTableID.inventorySFXInfoTab, Manager.main.player.transform.position);
		}

		public static void PlayError() {
			AudioManager.SfxUI(SfxID.menu_denied, 1.15f, false, 0.5f, 0.05f);
		}

		public static void PlayChangeMainTab(Component source, MainTab tab) {
			AudioManager.Sfx(SfxTableID.inventorySFXCreativeModeCategory, source.transform.position);

			switch (tab) {
				case MainTab.Items:
					AudioManager.Sfx(SfxTableID.turfDamage, source.transform.position, volumeMultiplier: 0.3f);
					break;
				case MainTab.Cooking:
					AudioManager.Sfx(SfxTableID.cattleEating, source.transform.position, volumeMultiplier: 0.15f);
					break;
				case MainTab.Checklist:
					AudioManager.Sfx(SfxTableID.cavelingBruteScratch, source.transform.position, volumeMultiplier: 0.75f);
					break;
				case MainTab.History:
					AudioManager.Sfx(SfxTableID.cavelingBruteScratch, source.transform.position, volumeMultiplier: 0.75f);
					break;
				case MainTab.Creatures:
					AudioManager.Sfx(SfxTableID.slimeBlobDeath, source.transform.position, volumeMultiplier: 0.1f);
					break;
				case MainTab.Options:
					AudioManager.Sfx(SfxTableID.inventorySFXRepairOn, source.transform.position, volumeMultiplier: 0.1f);
					break;
			}
		}
	}
}