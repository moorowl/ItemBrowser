using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Api.Entries {
	public class ObjectEntryRegistry {
		private readonly Dictionary<ObjectDataCD, EntryLookup>[] _entries = new[] {
			new Dictionary<ObjectDataCD, EntryLookup>(),
			new Dictionary<ObjectDataCD, EntryLookup>()
		};
		
		public IEnumerable<ObjectEntry> GetAllEntries(ObjectEntryType type, ObjectID id, int variation) {
			var objectData = new ObjectDataCD {
				objectID = id,
				variation = ObjectUtils.GetPrimaryVariation(id, variation),
			};
			return !_entries[(int) type].TryGetValue(objectData, out var entries) ? Array.Empty<ObjectEntry>() : entries.GetEntries();
		}

		public IEnumerable<ObjectEntry> GetAllEntries(ObjectEntryType type, ObjectDataCD objectData) {
			return GetAllEntries(type, objectData.objectID, objectData.variation);
		}
		
		public IEnumerable<T> GetEntries<T>(ObjectEntryType type, ObjectID id, int variation) where T : ObjectEntry {
			var objectData = new ObjectDataCD {
				objectID = id,
				variation = ObjectUtils.GetPrimaryVariation(id, variation),
			};
			return !_entries[(int) type].TryGetValue(objectData, out var entries) ? Array.Empty<T>() : entries.GetEntriesOfType<T>();
		}
		
		public IEnumerable<T> GetEntries<T>(ObjectEntryType type, ObjectDataCD objectData) where T : ObjectEntry {
			return GetEntries<T>(type, objectData.objectID, objectData.variation);
		}
		
		public void Register(ObjectEntryType type, ObjectID id, int variation, ObjectEntry entry) {
			id = TryReplaceObjectID(id);
			if (id == ObjectID.None || !ObjectUtils.IsPrimaryVariation(id, variation) || (type == ObjectEntryType.Source && ObjectUtils.GetCategories(id).Contains("NonObtainable/Deprecated")))
				return;
			
			var objectData = new ObjectDataCD {
				objectID = id,
				variation = variation,
			};
			if (!_entries[(int) type].ContainsKey(objectData))
				_entries[(int) type][objectData] = new EntryLookup();
			
			_entries[(int) type][objectData].Add(entry);
		}
		
		public void Register(ObjectEntryType type, ObjectDataCD objectData, ObjectEntry entry) {
			Register(type, objectData.objectID, objectData.variation, entry);
		}
		
		internal void RegisterFromProviders(List<ObjectEntryProvider> providers) {
			foreach (var entries in _entries)
				entries.Clear();

			var allObjects = DatabaseConversionUtility.GetPrefabList(Manager.ecs.pugDatabase).Select(prefabData => {
				var objectData = new ObjectData {
					objectID = prefabData.ObjectInfo.objectID,
					variation = prefabData.ObjectInfo.variation
				};

				return (objectData, prefabData.ObjectInfo.prefabInfos[0].ecsPrefab);
			}).Where(entry => ObjectUtils.IsPrimaryVariation(entry.objectData.objectID, entry.objectData.variation) && !ObjectUtils.GetCategories(entry.objectData.objectID).Contains("NonObtainable/Deprecated")).ToList();

			foreach (var provider in providers) {
				try {
					provider.Register(this, allObjects);
				} catch (Exception ex) {
					Main.Log("ObjectEntryRegistry", $"Error while registering entries from provider {provider.GetType().GetNameChecked()}");
					Debug.LogException(ex);
				}
			}
		}

		private static ObjectID TryReplaceObjectID(ObjectID id) {
			return id switch {
				ObjectID.GiantMushroom => ObjectID.GiantMushroom2,
				ObjectID.AmberLarva => ObjectID.AmberLarva2,
				ObjectID.OldRebreather => ObjectID.OldSporeMask,
				ObjectID.Gravestone => ObjectID.PlayerGrave,
				_ => id
			};
		}
		
		private class EntryLookup {
			private readonly List<ObjectEntry> _entries = new();
			private readonly Dictionary<Type, List<ObjectEntry>> _entriesByType = new();

			public void Add(ObjectEntry entry) {
				_entries.Add(entry);

				var type = entry.GetType();
				if (!_entriesByType.ContainsKey(type))
					_entriesByType[type] = new List<ObjectEntry>();
				
				_entriesByType[type].Add(entry);
			}

			public IEnumerable<ObjectEntry> GetEntries() {
				return _entries;
			}

			public IEnumerable<T> GetEntriesOfType<T>() where T : ObjectEntry {
				return !_entriesByType.ContainsKey(typeof(T)) ? Array.Empty<T>() : _entriesByType[typeof(T)].Cast<T>();
			}
		}
	}
}