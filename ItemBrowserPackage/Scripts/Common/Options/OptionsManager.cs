using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ItemBrowser.Common.Options {
	[HarmonyPatch]
	public class OptionsManager {
		private const int CurrentVersion = 2;
		private const float AutosaveInterval = 10f;
		
		public static OptionsManager Instance { get; private set; } = new();
		private static string _currentCharacterGuid;

		public delegate void OnTagChangedDelegate(ObjectDataCD objectData, ObjectTagType tag, bool wasAdded);
		public event OnTagChangedDelegate OnTagChanged;

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

		public bool ShowChecklist {
			get => GetActiveCharacterSpecificData().ShowChecklist;
			set {
				GetActiveCharacterSpecificData().ShowChecklist = value;
				_isDirty = true;
			}
		}
		
		public bool AutoMarkDiscoveredAsCollected {
			get => GetActiveCharacterSpecificData().AutoMarkDiscoveredAsCollected;
			set {
				GetActiveCharacterSpecificData().AutoMarkDiscoveredAsCollected = value;
				_isDirty = true;
			}
		}
		
		public bool HideNotCollectedIcons {
			get => GetActiveCharacterSpecificData().HideNotCollectedIcons;
			set {
				GetActiveCharacterSpecificData().HideNotCollectedIcons = value;
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
			_data.Characters[guid] = characterData;

			return characterData;
		}
		private CharacterSpecificOptionsData GetActiveCharacterSpecificData() {
			return GetCharacterSpecificData(_currentCharacterGuid);
		}

		public bool HasTag(ObjectDataCD objectData, ObjectTagType tag) {
			if (!_tags.TryGetValue(_currentCharacterGuid, out var objectToTags))
				return false;
			
			return objectToTags.TryGetValue(objectData, out var tags) && tags.Contains(tag);
		}
		
		public bool AddTag(ObjectDataCD objectData, ObjectTagType tag) {
			if (objectData.objectID == ObjectID.None)
				return false;
			
			if (!_tags.TryGetValue(_currentCharacterGuid, out var objectToTags)) {
				objectToTags = new Dictionary<ObjectDataCD, HashSet<ObjectTagType>>();
				_tags[_currentCharacterGuid] = objectToTags;
			}
			
			if (!objectToTags.TryGetValue(objectData, out var tags)) {
				tags = new HashSet<ObjectTagType>();
				objectToTags[objectData] = tags;
			}
			
			var isAdded = tags.Add(tag);
			if (isAdded) {
				OnTagChanged?.Invoke(objectData, tag, true);
				_isDirty = true;
			}

			return isAdded;
		}
		
		public bool RemoveTag(ObjectDataCD objectData, ObjectTagType tag) {
			if (!_tags.TryGetValue(_currentCharacterGuid, out var objectToTags))
				return false;

			if (!objectToTags.TryGetValue(objectData, out var tags))
				return false;

			var isRemoved = tags.Remove(tag);
			if (isRemoved) {
				OnTagChanged?.Invoke(objectData, tag, false);
				_isDirty = true;
			}

			return isRemoved;
		}

		public void RemoveTagFromAll(ObjectTagType tag) {
			if (!_tags.TryGetValue(_currentCharacterGuid, out var objectToTags))
            	return;

			foreach (var (objectData, tags) in objectToTags) {
				if (tags.Remove(tag))
					OnTagChanged?.Invoke(objectData, tag, false);
			}

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
			
			try {
				FileUtility.WriteData("Options", _data);
			} catch (Exception ex) {
				Logger.LogWarning("Error while saving options file");
				Logger.LogException(ex);
			}
			
			_isDirty = false;
			_lastSavedTime = Time.unscaledTime;
		}
		
		private void Load() {
			try {
				SetData(FileUtility.ReadData<OptionsData>("Options"));
			} catch (Exception ex) {
				Logger.LogWarning("Error while loading options file, using defaults");
				Logger.LogException(ex);
				SetDefaults();
			}
		}
		
		[HarmonyPatch]
		[HarmonyPatch(typeof(SaveManager), "SetCharacterId")]
		[HarmonyPostfix]
		private static void SetCharacterGuid(SaveManager __instance, int id) {
			_currentCharacterGuid = id == -1 ? null : Manager.saves.GetCharacterGuid().ToString();
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
			public bool ShowChecklist { get; set; }
			public bool AutoMarkDiscoveredAsCollected { get; set; } = true;
			public bool HideNotCollectedIcons { get; set; } = true;
			public List<TagObjectData> TaggedObjects { get; set; } = new();
		}
		
		private record TagObjectData {
			public string InternalName { get; set; }
			public int Variation { get; set; }
			public List<ObjectTagType> Tags { get; set; } = new();
		}
	}
}