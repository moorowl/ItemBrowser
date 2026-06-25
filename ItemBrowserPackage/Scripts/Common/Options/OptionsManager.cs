using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using Newtonsoft.Json;
using PugMod;
using Unity.Collections;
using UnityEngine;

namespace ItemBrowser.Common.Options {
	public class OptionsManager {
		public static OptionsManager Instance { get; private set; } = new();
		
		private const string FilePath = Main.InternalName + "/Options.json";
		private const int CurrentVersion = 2;
		private const float AutosaveInterval = 10f;

		public bool CheatMode {
			get => GetActiveCharacterSpecificData().CheatMode;
			set {
				GetActiveCharacterSpecificData().CheatMode = value;
				_isDirty = true;
			}
		}
		
		public bool DiscoveryMode {
			get => GetActiveCharacterSpecificData().DiscoveryMode;
			set {
				GetActiveCharacterSpecificData().DiscoveryMode = value;
				_isDirty = true;
			}
		}
		
		public bool ShowSourceMod {
			get => _data.ShowSourceMod;
			set {
				_data.ShowSourceMod = value;
				_isDirty = true;
			}
		}
		
		public bool ShowButtonHints {
			get => _data.ShowButtonHints;
			set {
				_data.ShowButtonHints = value;
				_isDirty = true;
			}
		}
		
		public bool AlwaysShowTechnicalInfo {
			get => _data.AlwaysShowTechnicalInfo;
			set {
				_data.AlwaysShowTechnicalInfo = value;
				_isDirty = true;
			}
		}

		public bool PanelsShiftLayout {
			get => _data.PanelsShiftLayout;
			set {
				_data.PanelsShiftLayout = value;
				_isDirty = true;
			}
		}
		
		public bool SearchByEffect {
			get => _data.SearchByEffect;
			set {
				_data.SearchByEffect = value;
				_isDirty = true;
			}
		}
		
		public bool SearchByDescription {
			get => _data.SearchByDescription;
			set {
				_data.SearchByDescription = value;
				_isDirty = true;
			}
		}
		
		public bool SearchById {
			get => _data.SearchById;
			set {
				_data.SearchById = value;
				_isDirty = true;
			}
		}
		
		public VirtualObjectListLayout ListLayout {
			get => _data.ListLayout;
			set {
				_data.ListLayout = value;
				_isDirty = true;
			}
		}
		
		public DataBlockAddress Theme {
			get => _data.Theme;
			set {
				_data.Theme = value;
				_isDirty = true;
			}
		}
		
		private readonly Dictionary<string, Dictionary<ObjectDataCD, HashSet<ObjectTagType>>> _tags = new();
		
		private OptionsData _data;
		private bool _hasInit;
		private bool _isDirty;
		private float _lastSavedTime;

		
		private CharacterSpecificOptionsData GetCharacterSpecificData(string guid) {
			if (_data.Characters.TryGetValue(guid, out var characterData))
				return characterData;

			characterData = new CharacterSpecificOptionsData();
			_data.Characters[ItemBrowserAPI.CurrentCharacterGuid] = characterData;

			return characterData;
		}
		private CharacterSpecificOptionsData GetActiveCharacterSpecificData() {
			return GetCharacterSpecificData(ItemBrowserAPI.CurrentCharacterGuid);
		}

		public bool HasTag(ObjectDataCD objectData, ObjectTagType tag) {
			if (!_tags.TryGetValue(ItemBrowserAPI.CurrentCharacterGuid, out var objectToTags))
				return false;
			
			return objectToTags.TryGetValue(objectData, out var tags) && tags.Contains(tag);
		}
		
		public bool AddTag(ObjectDataCD objectData, ObjectTagType tag) {
			if (!_tags.TryGetValue(ItemBrowserAPI.CurrentCharacterGuid, out var objectToTags)) {
				objectToTags = new Dictionary<ObjectDataCD, HashSet<ObjectTagType>>();
				_tags[ItemBrowserAPI.CurrentCharacterGuid] = objectToTags;
			}
			
			if (!objectToTags.TryGetValue(objectData, out var tags)) {
				tags = new HashSet<ObjectTagType>();
				objectToTags[objectData] = tags;
			}

			if (!tags.Add(tag))
				return false;

			_isDirty = true;
			return true;
		}
		
		public bool RemoveTag(ObjectDataCD objectData, ObjectTagType tag) {
			if (!_tags.TryGetValue(ItemBrowserAPI.CurrentCharacterGuid, out var objectToTags))
				return false;

			if (!objectToTags.TryGetValue(objectData, out var tags) || !tags.Remove(tag))
				return false;

			_isDirty = true;
			return true;
		}

		public void RemoveTagFromAll(ObjectTagType tag) {
			if (!_tags.TryGetValue(ItemBrowserAPI.CurrentCharacterGuid, out var objectToTags))
            	return;
			
			foreach (var tags in objectToTags.Values)
				tags.Remove(tag);

			_isDirty = true;
		}

		private void OnPostDeserialize() {
			_tags.Clear();
			
			foreach (var (guid, characterSpecificOptions) in _data.Characters) {
				_tags[guid] = characterSpecificOptions.TaggedObjects
					.Where(objectData => API.Authoring.GetObjectID(objectData.InternalName) != ObjectID.None)
					.ToDictionary(
						objectData => new ObjectDataCD { objectID = API.Authoring.GetObjectID(objectData.InternalName), variation = objectData.Variation },
						objectData => objectData.Tags.ToHashSet()
					);
			}
		}

		private void OnPreSerialize() {
			foreach (var (guid, taggedObjects) in _tags) {
				GetCharacterSpecificData(guid).TaggedObjects = taggedObjects
					.Where(objectAndTags => objectAndTags.Value.Count > 0)
					.Select(objectAndTags => new TagObjectData {
						InternalName = ObjectUtility.GetInternalName(objectAndTags.Key.objectID),
						Variation = objectAndTags.Key.variation,
						Tags = objectAndTags.Value.ToList()
					})
					.ToList();
			}
		}
		
		private void SetData(OptionsData data) {
			_data = data;
			_isDirty = false;

			OnPostDeserialize();
		}
		
		public void Init() {
			Load();
			_hasInit = true;
		}

		public void Update() {
			if (!_hasInit)
				return;
			
			if (_isDirty && Time.unscaledTime > _lastSavedTime + AutosaveInterval)
				Save();
		}

		public void SetDefaults() {
			SetData(new OptionsData());
			Save();
		}
		
		public void Save() {
			OnPreSerialize();
			
			if (!API.ConfigFilesystem.DirectoryExists(Main.InternalName))
				API.ConfigFilesystem.CreateDirectory(Main.InternalName);
			
			try {
				var serializedData = JsonConvert.SerializeObject(_data);
				API.ConfigFilesystem.Write(FilePath, Encoding.UTF8.GetBytes(serializedData));
			} catch (Exception ex) {
				Main.Log(nameof(OptionsManager), "Error while saving file");
				Main.Log(ex);
			}
			
			_isDirty = false;
			_lastSavedTime = Time.unscaledTime;
		}
		
		private void Load() {
			if (!API.ConfigFilesystem.FileExists(FilePath)) {
				SetDefaults();
				return;
			}
			
			try {
				var deserializedData = JsonConvert.DeserializeObject<OptionsData>(Encoding.UTF8.GetString(API.ConfigFilesystem.Read(FilePath)));
				SetData(deserializedData);
			} catch (Exception ex) {
				Main.Log(nameof(OptionsManager), "Error while loading file, using defaults");
				Main.Log(ex);
				SetDefaults();
			}
		}

		private record OptionsData {
			public int Version { get; set; } = CurrentVersion;
			public bool ShowSourceMod { get; set; } = true;
			public bool ShowButtonHints { get; set; } = true;
			public bool AlwaysShowTechnicalInfo { get; set; }
			public bool PanelsShiftLayout { get; set; }
			public bool SearchByEffect { get; set; } = true;
			public bool SearchByDescription { get; set; } = true;
			public bool SearchById { get; set; } = true;
			public VirtualObjectListLayout ListLayout { get; set; } = VirtualObjectListLayout.Grid;
			public Guid Theme { get; set; }
			public Dictionary<string, CharacterSpecificOptionsData> Characters { get; set; } = new();
		}

		private record CharacterSpecificOptionsData {
			public bool CheatMode { get; set; }
			public bool DiscoveryMode { get; set; }
			public List<TagObjectData> TaggedObjects { get; set; } = new();
		}
		
		private record TagObjectData {
			public string InternalName { get; set; }
			public int Variation { get; set; }
			public List<ObjectTagType> Tags { get; set; } = new();
		}
	}
}