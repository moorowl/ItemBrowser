using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ItemBrowser.Api {
	public static class ItemBrowserAPI {
		private const string BrowserPrefabPath = "Assets/ItemBrowser/ItemBrowserPackage/Prefabs/Browser/ItemBrowserUI.prefab";
		
		public static event Action OnClientLanguageChanged;
		public static ItemBrowserUI ItemBrowserUI { get; private set; }

		internal static readonly ItemBrowserRegistry Registry = new();
		internal static readonly ObjectEntryRegistry ObjectEntryRegistry = new();
		private static readonly List<ItemBrowserPlugin> Plugins = new();
		
		private static bool _hasRegistered;

		internal static void Init() {
			InitPlugins();
		}

		private static void InitPlugins() {
			foreach (var type in API.Reflection.GetTypes(0)) {
				if (type.IsAbstract || !typeof(ItemBrowserPlugin).IsAssignableFrom(type))
					continue;
				
				var instance = (ItemBrowserPlugin) Activator.CreateInstance(type);
				var loadedAssociatedMod = API.ModLoader.LoadedMods.FirstOrDefault(loadedMod => loadedMod.Metadata.name == instance.AssociatedMod);
				if (loadedAssociatedMod == null)
					continue;
					
				instance.AssociatedLoadedMod = loadedAssociatedMod;

				if (instance.IsEnabled) {
					Plugins.Add(instance);
					Main.Log(nameof(ItemBrowserAPI), $"Added plugin {type.GetNameChecked()} from {instance.AssociatedMod}");
				} else {
					Main.Log(nameof(ItemBrowserAPI), $"Skipped disabled plugin {type.GetNameChecked()} from {instance.AssociatedMod}");
				}
			}
		}
		
		private static void InitBrowserUI() {
			var prefab = Main.AssetBundle.LoadAsset<GameObject>(BrowserPrefabPath);
			if (prefab == null)
				throw new NullReferenceException($"Failed to load BrowserUI prefab at {BrowserPrefabPath}");
			
			ItemBrowserUI = Object.Instantiate(prefab, API.Rendering.UICamera.transform).GetComponent<ItemBrowserUI>();
			
			if (!_hasRegistered) {
				foreach (var plugin in Plugins) {
					if (plugin.AutomaticallyRegisterFromAssets)
						plugin.OnAutomaticallyRegisterFromAssets(Registry);
				}
			}
			
			ObjectUtils.InitOnWorldLoad();
			StructureUtils.InitOnWorldLoad();

			if (!_hasRegistered) {
				foreach (var plugin in Plugins)
					plugin.OnEarlyRegister(Registry);
				
				foreach (var plugin in Plugins)
					plugin.OnRegister(Registry);
			}
			
			_hasRegistered = true;
			ObjectEntryRegistry.RegisterFromProviders(Registry.EntryProviders);

			SetupSortingAndFilteringIndexes();
			OnClientLanguageChanged += SetupSortingAndFilteringIndexes;

			Manager.main.StartCoroutine(TemporarilyShowBrowserToAvoidLagSpikes());
		}

		private static void UninitBrowserUI() {
			if (ItemBrowserUI != null)
				Object.Destroy(ItemBrowserUI);

			OnClientLanguageChanged -= SetupSortingAndFilteringIndexes;
		}

		private static IEnumerator TemporarilyShowBrowserToAvoidLagSpikes() {
			ItemBrowserUI.IsShowing = true;

			yield return new WaitForSeconds(0.1f);

			ItemBrowserUI.IsShowing = false;
		}

		private static void SetupSortingAndFilteringIndexes() {
			var allObjects = ObjectUtils.GetAllObjects().ToHashSet();
			
			foreach (var entry in Registry.ItemFilters)
				entry.Filter.SetupIndexedMatches(allObjects);
			
			foreach (var entry in Registry.CreatureFilters)
				entry.Filter.SetupIndexedMatches(allObjects);
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
			if (Registry.ElementPools.TryGetValue(type, out var pool))
				return (UIelement) pool.GetFreeComponent(true, true);
			
			return null;
		}
		
		public static T GetPooledElement<T>() where T : UIelement {
			if (Registry.ElementPools.TryGetValue(typeof(T), out var pool))
				return (T) pool.GetFreeComponent(true, true);
			
			return null;
		}
		
		public static void FreePooledElement(UIelement element) {
			if (Registry.ElementPools.TryGetValue(element.GetType(), out var pool)) {
				pool.Free(element);

				if (Manager.ui.currentSelectedUIElement == element) {
					Manager.ui.DeselectAnySelectedUIElement();
					Manager.ui.mouse.UpdateMouseUIInput(out _, out _);
				}
			}
		}
		
		public static bool IsPooledElement(UIelement element) {
			return Registry.ElementPools.ContainsKey(element.GetType());
		}
		
		[HarmonyPatch]
		public static class Patches {
			private static string _lastLanguage;

			[HarmonyPatch(typeof(PlayerController), "ManagedUpdate")]
			[HarmonyPostfix]
			private static void PlayerController_ManagedUpdate(PlayerController __instance) {
				if (_lastLanguage == LocalizationManager.CurrentLanguage)
					return;

				_lastLanguage = LocalizationManager.CurrentLanguage;
				OnClientLanguageChanged?.Invoke();
			}

			[HarmonyPatch(typeof(PlayerController), "OnOccupied")]
			[HarmonyPostfix]
			private static void PlayerController_OnOccupied(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				Manager.main.StartCoroutine(InitBrowserUICoroutine());
			}

			[HarmonyPatch(typeof(PlayerController), "OnFree")]
			[HarmonyPostfix]
			private static void PlayerController_OnFree(PlayerController __instance) {
				if (!__instance.isLocal)
					return;

				UninitBrowserUI();
			}

			private static IEnumerator InitBrowserUICoroutine() {
				// Have to wait for active content bundles to be synced
				yield return new WaitUntil(() => ClientWorldStateSystem.HasRunAtLeastOnce);
				
				InitBrowserUI();
			}
		}
	}
}