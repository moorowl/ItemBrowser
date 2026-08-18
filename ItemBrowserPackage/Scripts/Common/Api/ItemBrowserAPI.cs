using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Options.Discovery;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.Api {
	[HarmonyPatch]
	public static class ItemBrowserAPI {
		private const string BrowserPrefabPath = "Assets/ItemBrowser/ItemBrowserPackage/Prefabs/Browser/ItemBrowserUI.prefab";
		
		public static ItemBrowserUI ItemBrowserUI { get; private set; }

		internal static readonly ItemBrowserRegistry Registry = new();
		internal static readonly ObjectEntryRegistry ObjectEntryRegistry = new();
		private static readonly List<ItemBrowserPlugin> Plugins = new();

		public static event Action OnBrowserInit;
		public static event Action OnBrowserUpdate;
		public static event Action OnBrowserUninit;
		
		private static bool _hasRegisteredPluginContent;
		private static bool _hasRegisteredPluginContentLate;
		private static string _lastLanguage;
	
		public static void AddPlugin(ItemBrowserPlugin instance, IMod sourceMod) {
			var modInfo = API.ModLoader.LoadedMods.First(modInfo => modInfo.Handlers.Contains(sourceMod));
			instance.AssociatedLoadedMod = modInfo;

			Logger.LogInfo($"Added plugin {instance.GetType().GetNameChecked()} from {modInfo.Metadata.name}");
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

			// to avoid lag spikes when opening for first time
			ItemBrowserUI.IsShowing = true;
			ItemBrowserUI.IsShowing = false;

			if (reloadWorldSpecificContent)
				SaveDebugData();
			
			OnBrowserInit?.Invoke();
		}

		private static void UninitBrowserUI() {
			OnBrowserUninit?.Invoke();
			
			if (ItemBrowserUI != null)
				Object.Destroy(ItemBrowserUI.gameObject);
		}

		private static void RegisterPluginContent() {
			Logger.LogInfo("Registered early/mid plugin content");
			
			foreach (var plugin in Plugins) {
				if (plugin.AutomaticallyRegisterFromAssets)
					plugin.OnAutomaticallyRegisterFromAssets(Registry);
			}
			
			ObjectUtility.Bake();
			
			foreach (var plugin in Plugins)
				plugin.OnEarlyRegister(Registry);

			foreach (var plugin in Plugins)
				plugin.OnRegister(Registry);
		}

		private static void SaveDebugData() {
			FileUtility.WriteData("Debug/NonPrimaryVariations", ObjectUtility.GetAllObjects()
				.Where(objectData => !ObjectUtility.IsPrimaryVariation(objectData))
				.OrderBy(objectData => (int) objectData.objectID * 10000 + objectData.variation)
				.Select(objectData => new {
					Id = ObjectUtility.GetInternalName(objectData),
					Variation = objectData.variation,
					AuthoringPrefabName = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation).prefabInfos[0].ecsPrefab.name
				})
				.ToList()
			);
			FileUtility.WriteData("Debug/Indestructibles", ObjectUtility.GetAllObjects()
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
			Logger.LogInfo("Reloaded language-specific content");
			
			ObjectUtility.Bake();
		}
		
		private static void ReloadWorldSpecificContent() {
			Logger.LogInfo("Reloaded world-specific content");
			
			StructureUtility.Bake();
			
			var startTime = DateTime.UtcNow;
			ObjectEntryRegistry.RegisterFromProviders(Registry.EntryProviders);
			Logger.LogInfo($"Registered entries from {Registry.EntryProviders.Count} providers in {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");

			if (!_hasRegisteredPluginContentLate) {
				foreach (var plugin in Plugins)
					plugin.OnLateRegister(Registry);

				_hasRegisteredPluginContentLate = true;
				
				Logger.LogInfo("Registered late plugin content");
			}
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
		
		public static bool IsChecklistIndexed(ObjectDataCD objectData) {
			return Registry.ChecklistObjects.Contains(objectData);
		}
		
		public static bool IsChecklistIndexed(ObjectID id, int variation = 0) {
			return IsChecklistIndexed(new ObjectDataCD {
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
		
		private static IEnumerator InitBrowserOnWorldEnteredRoutine() {
			// Have to wait for active content bundles to be synced
			yield return new WaitUntil(() => ClientWorldStateSystem.HasRunAtLeastOnce);

			InitBrowserUI(true, true);
		}
		
		[HarmonyPatch]
		[HarmonyPatch(typeof(PlayerController), "ManagedUpdate")]
		[HarmonyPostfix]
		private static void UpdateBrowserFromPlayer(PlayerController __instance) {
			if (!__instance.isLocal || ItemBrowserUI == null)
				return;

			ReloadIfLanguageChanged();
				
			OnBrowserUpdate?.Invoke();
		}

		[HarmonyPatch(typeof(PlayerController), "OnOccupied")]
		[HarmonyPostfix]
		private static void InitBrowserFromPlayer(PlayerController __instance) {
			if (!__instance.isLocal)
				return;

			_lastLanguage = LocalizationManager.CurrentLanguage;
			__instance.StartCoroutine(InitBrowserOnWorldEnteredRoutine());
		}

		[HarmonyPatch(typeof(PlayerController), "OnFree")]
		[HarmonyPostfix]
		private static void UninitBrowserFromPlayer(PlayerController __instance) {
			if (!__instance.isLocal)
				return;

			UninitBrowserUI();
		}
	}
}