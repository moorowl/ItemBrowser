using System;
using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Api.Themes;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Api {
	public class ItemBrowserRegistry {
		internal readonly HashSet<ObjectDataCD> Items = new();
		internal readonly HashSet<ObjectDataCD> Creatures = new();
		internal readonly HashSet<ObjectDataCD> TechnicalObjects = new();
		internal readonly HashSet<ObjectDataCD> DeprecatedObjects = new();
		
		internal readonly List<(string Group, Filter<ObjectDataCD> Filter)> ItemFilters = new();
		internal readonly List<(string Group, Filter<ObjectDataCD> Filter)> CreatureFilters = new();

		internal readonly List<Sorter<ObjectDataCD>> ItemSorters = new();
		internal readonly List<Sorter<ObjectDataCD>> CreatureSorters = new();

		internal readonly List<ObjectEntryProvider> EntryProviders = new();
		internal readonly Dictionary<Type, ObjectEntryDisplayBase> EntryToDisplayComponent = new();

		internal readonly Dictionary<Type, PoolSystem> ElementPools = new();

		public void AddItem(ObjectDataCD item) {
			Items.Add(item);
		}

		public void RemoveItem(ObjectDataCD item) {
			Items.Remove(item);
		}
		
		public void AddCreature(ObjectDataCD creature) {
			Creatures.Add(creature);
		}
		
		public void RemoveCreature(ObjectDataCD creature) {
			Creatures.Remove(creature);
		}
		
		public void AddTechnicalObject(ObjectDataCD objectData) {
			TechnicalObjects.Add(objectData);
		}

		public void RemoveTechnicalObject(ObjectDataCD objectData) {
			TechnicalObjects.Remove(objectData);
		}
		
		public void AddDeprecatedObject(ObjectDataCD objectData) {
			DeprecatedObjects.Add(objectData);
		}
		
		public void RemoveDeprecatedObject(ObjectDataCD objectData) {
			DeprecatedObjects.Remove(objectData);
		}
		
		public void AddEntryProvider(ObjectEntryProvider provider) {
			EntryProviders.Add(provider);
		}

		public void AddEntryDisplay(ObjectEntryDisplayBase component) {
			if (component == null)
				return;

			var gameObject = component.gameObject;
			foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>())
				sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			foreach (var pugText in gameObject.GetComponentsInChildren<PugText>())
				pugText.style.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

			EntryToDisplayComponent.TryAdd(component.AssociatedEntry, component);
		}
		
		public void AddItemFilter(string group, Filter<ObjectDataCD> filter) {
			ItemFilters.Add((group, filter));
		}

		public void AddCreatureFilter(string group, Filter<ObjectDataCD> filter) {
			CreatureFilters.Add((group, filter));
		}
		
		public void AddItemSorter(Sorter<ObjectDataCD> sorter) {
			ItemSorters.Add(sorter);
		}

		public void AddCreatureSorter(Sorter<ObjectDataCD> sorter) {
			CreatureSorters.Add(sorter);
		}
		
		public void AddPooledElement(PooledElement element) {
			if (!element.gameObject.TryGetComponent<UIelement>(out var attachedUIElement))
				return;

			var attachedUIElementType = attachedUIElement.GetType();
			if (ElementPools.ContainsKey(attachedUIElementType))
				return;
				
			if (element.overrideMaskInteractionAtRuntime) {
				foreach (var text in attachedUIElement.gameObject.GetComponentsInChildren<PugText>(true))
					text.style.maskInteraction = element.maskInteraction;
					
				foreach (var sr in attachedUIElement.gameObject.GetComponentsInChildren<SpriteRenderer>(true))
					sr.maskInteraction = element.maskInteraction;
			}

			var parent = new GameObject($"ElementPool {attachedUIElementType.GetNameChecked()}");
			UnityEngine.Object.DontDestroyOnLoad(parent);
			
			var poolSystem = new PoolSystem(attachedUIElement.gameObject, attachedUIElementType, parent.transform, autoEnable: true, element.initialSize, element.maxSize, element.maxFreeSize);
			ElementPools[attachedUIElementType] = poolSystem;
		}
	}
}