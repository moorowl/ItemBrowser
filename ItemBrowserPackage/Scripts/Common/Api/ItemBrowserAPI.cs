using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.Api {
	public static class ItemBrowserAPI {
		private const string BrowserPrefabPath = "Assets/ItemBrowser/ItemBrowserPackage/Prefabs/Browser/ItemBrowserUI.prefab";
		
		public static ItemBrowserUI ItemBrowserUI { get; private set; }
		public static string CurrentCharacterGuid { get; private set; }

		internal static readonly ItemBrowserRegistry Registry = new();
		internal static readonly ObjectEntryRegistry ObjectEntryRegistry = new();
		private static readonly List<ItemBrowserPlugin> Plugins = new();

		private static bool _hasRegisteredPluginContent;
		private static string _lastLanguage;
		private static bool _shouldDiscoverTilesAndObjects;

		public static void AddPlugin(ItemBrowserPlugin instance, IMod sourceMod) {
			var modInfo = API.ModLoader.LoadedMods.First(modInfo => modInfo.Handlers.Contains(sourceMod));
			instance.AssociatedLoadedMod = modInfo;
			
			Main.Log(nameof(ItemBrowserAPI), $"Added plugin {instance.GetType().GetNameChecked()} from {modInfo.Metadata.name}");
			Plugins.Add(instance);
		}
		
		private static void InitBrowserUI(bool reloadWorldSpecificContent, bool reloadLanguageSpecificContent) {
			var prefab = Main.AssetBundle.LoadAsset<GameObject>(BrowserPrefabPath);
			if (prefab == null)
				throw new NullReferenceException($"Failed to load BrowserUI prefab at {BrowserPrefabPath}");
			
			ItemBrowserUI = Object.Instantiate(prefab, API.Rendering.UICamera.transform).GetComponent<ItemBrowserUI>();

			if (!_hasRegisteredPluginContent) {
				RegisterPluginContent();
				_hasRegisteredPluginContent = true;
			}

			if (reloadLanguageSpecificContent)
				ReloadLanguageSpecificContent();
			if (reloadWorldSpecificContent)
				ReloadWorldSpecificContent();

			_shouldDiscoverTilesAndObjects = false;
			CurrentCharacterGuid = Manager.saves.GetCharacterGuid().ToString();
			
			if (Manager.load.IsScreenBlack())
				Manager.main.StartCoroutine(TemporarilyShowBrowserToAvoidLagSpikes());
			
			API.Client.OnObjectSpawnedOnClient += DiscoverNearbyObjects;
		}

		private static void UninitBrowserUI() {
			if (ItemBrowserUI != null)
				Object.Destroy(ItemBrowserUI.gameObject);
			
			API.Client.OnObjectSpawnedOnClient -= DiscoverNearbyObjects;
		}

		private static void RegisterPluginContent() {
			Main.Log(nameof(ItemBrowserAPI), "RegisterPluginContent");
			
			foreach (var plugin in Plugins) {
				if (plugin.AutomaticallyRegisterFromAssets)
					plugin.OnAutomaticallyRegisterFromAssets(Registry);
			}
			
			ObjectUtility.Bake();
			
			foreach (var plugin in Plugins)
				plugin.OnEarlyRegister(Registry);

			foreach (var plugin in Plugins)
				plugin.OnRegister(Registry);

			DebugUtility.ExportData("NonPrimaryVariations", ObjectUtility.GetAllObjects()
				.Where(objectData => !ObjectUtility.IsPrimaryVariation(objectData))
				.OrderBy(objectData => (int) objectData.objectID * 10000 + objectData.variation)
				.Select(objectData => new {
					Id = ObjectUtility.GetInternalName(objectData),
					Variation = objectData.variation,
					AuthoringPrefabName = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).prefabInfos[0].ecsPrefab.name
				})
				.ToList()
			);
			DebugUtility.ExportData("Indestructibles", ObjectUtility.GetAllObjects()
				.Where(ObjectUtility.IsIndestructible)
				.OrderBy(objectData => (int) objectData.objectID * 10000 + objectData.variation)
				.Select(objectData => new {
					Id = ObjectUtility.GetInternalName(objectData),
					Variation = objectData.variation
				})
				.ToList()
			);
		}
		
		private static void ReloadLanguageSpecificContent() {
			Main.Log(nameof(ItemBrowserAPI), "ReloadLanguageSpecificContent");
			
			ObjectUtility.Bake();
		}
		
		private static void ReloadWorldSpecificContent() {
			Main.Log(nameof(ItemBrowserAPI), "ReloadWorldSpecificContent");
			
			StructureUtility.Bake();
			
			var startTime = DateTime.UtcNow;
			ObjectEntryRegistry.RegisterFromProviders(Registry.EntryProviders);
			Main.Log(nameof(ItemBrowserAPI), $"Registered entries from {Registry.EntryProviders.Count} providers in {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");	
		}
		
		private static IEnumerator TemporarilyShowBrowserToAvoidLagSpikes() {
			ItemBrowserUI.IsShowing = true;

			yield return new WaitForSeconds(0.1f);

			ItemBrowserUI.IsShowing = false;
		}
		
		public static bool IsItemIndexed(ObjectDataCD objectData) {
			return Registry.Items.Contains(objectData);
		}
		
		public static bool IsItemIndexed(ObjectID id, int variation = 0) {
			return IsItemIndexed(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}

		public static bool IsCreatureIndexed(ObjectDataCD objectData) {
			return Registry.Creatures.Contains(objectData);
		}
		
		public static bool IsCreatureIndexed(ObjectID id, int variation = 0) {
			return IsCreatureIndexed(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}

		public static bool IsTechnicalObject(ObjectDataCD objectData) {
			return Registry.TechnicalObjects.Contains(objectData);
		}
		
		public static bool IsTechnicalObject(ObjectID id, int variation = 0) {
			return IsTechnicalObject(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}

		public static bool IsDeprecatedObject(ObjectDataCD objectData) {
			return Registry.DeprecatedObjects.Contains(objectData);
		}
		
		public static bool IsDeprecatedObject(ObjectID id, int variation = 0) {
			return IsDeprecatedObject(new ObjectDataCD {
				objectID = id,
				variation = variation
			});
		}
		
		public static UIelement GetPooledElement(Type type) {
			if (Registry.ElementPools.TryGetValue(type, out var pool)) {
				var element = (UIelement) pool.GetFreeComponent(true, true);
				// Fix for scale being set to zero sometimes?
				if (Mathf.Approximately(element.transform.localScale.x + element.transform.localScale.y, 0f))
					element.transform.localScale = Vector3.one;
				
				return element;
			}

			return null;
		}
		
		public static T GetPooledElement<T>() where T : UIelement {
			return (T) GetPooledElement(typeof(T));
		}
		
		public static void FreePooledElement(UIelement element) {
			if (Registry.ElementPools.TryGetValue(element.GetType(), out var pool))
				pool.Free(element);
		}
		
		public static bool IsPooledElement(UIelement element) {
			return Registry.ElementPools.ContainsKey(element.GetType());
		}

		private static void ReloadIfLanguageChanged() {
			if (_lastLanguage != LocalizationManager.CurrentLanguage) {
				if (ItemBrowserUI != null) {
					UninitBrowserUI();
					InitBrowserUI(false, true);
				}
				
				_lastLanguage = LocalizationManager.CurrentLanguage;	
			}
		}

		private static void DiscoverNearbyTiles() {
			if (!_shouldDiscoverTilesAndObjects || Time.frameCount % 60 != 0)
				return;
			
			Manager.audio.ambientSoundsHandler.GetNearbyTileData(out var tileCounts).Complete();

			foreach (var entry in tileCounts) {
				if (entry.Value == 0)
					continue;

				if (TileUtility.TryGetAssociatedObject(entry.Key.TileType, entry.Key.Tileset, out var associatedObjectData) && associatedObjectData.objectID != ObjectID.None) {
					associatedObjectData.variation = ObjectUtility.GetPrimaryVariation(associatedObjectData);
							
					if (Manager.saves.SetObjectAsDiscovered(associatedObjectData))
						Main.Log(nameof(ItemBrowserAPI), $"Discovered tile {associatedObjectData.objectID}:{associatedObjectData.variation}");
				}
			}
		}

		private static void DiscoverNearbyObjects(Entity entity, EntityManager entityManager, GameObject graphicalObject) {
			if (!_shouldDiscoverTilesAndObjects)
				return;
			
			// Discover placed objects and creatures
			var objectData = entityManager.GetComponentData<ObjectDataCD>(entity);
			objectData.variation = 0;
			objectData.amount = 0;

			if (objectData.objectID != ObjectID.None)
				Manager.saves.SetObjectAsDiscovered(objectData);
		}
		
		private static IEnumerator InitBrowserOnWorldEnteredRoutine() {
			// Have to wait for active content bundles to be synced
			yield return new WaitUntil(() => ClientWorldStateSystem.HasRunAtLeastOnce);

			InitBrowserUI(true, true);
		}
		
		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(PlayerController), "ManagedUpdate")]
			[HarmonyPostfix]
			private static void PlayerController_ManagedUpdate(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				ReloadIfLanguageChanged();
				DiscoverNearbyTiles();

				_shouldDiscoverTilesAndObjects = OptionsManager.Instance.DiscoveryMode;
			}

			[HarmonyPatch(typeof(PlayerController), "OnOccupied")]
			[HarmonyPostfix]
			private static void PlayerController_OnOccupied(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				_lastLanguage = LocalizationManager.CurrentLanguage;
				__instance.StartCoroutine(InitBrowserOnWorldEnteredRoutine());
			}

			[HarmonyPatch(typeof(PlayerController), "OnFree")]
			[HarmonyPostfix]
			private static void PlayerController_OnFree(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				UninitBrowserUI();
			}
		}
	}
}