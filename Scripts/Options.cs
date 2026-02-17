using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using Newtonsoft.Json;
using PugMod;
using Unity.Collections;
using UnityEngine;

namespace ItemBrowser {
	public class Options {
		public static Options Instance { get; private set; } = new();
		
		private const string FilePath = Main.InternalName + "/Options.json";
		private const int CurrentVersion = 1;
		private const float AutosaveInterval = 10f;

		public bool CheatMode {
			get => _data.CheatMode;
			set {
				_data.CheatMode = value;
				_isDirty = true;
			}
		}
		
		public bool DiscoveryMode {
			get => _data.DiscoveryMode;
			set {
				_data.DiscoveryMode = value;
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

		public int FavoritesCount => _favorites.Count;

		private HashSet<ObjectDataCD> _favorites;
		
		private OptionsData _data;
		private bool _hasInit;
		private bool _isDirty;
		private float _lastSavedTime;

		public bool IsFavorited(ObjectDataCD objectData) {
			return _favorites.Contains(objectData);
		}
		
		public bool AddFavorite(ObjectDataCD objectData) {
			if (!IsFavorited(objectData)) {
				_favorites.Add(objectData);
				_data.Favorites.Add(new OptionsObjectData {
					InternalName = ObjectUtils.GetInternalName(objectData),
					Variation = objectData.variation
				});
				
				_isDirty = true;
			}
			
			return false;
		}

		public bool RemoveFavorite(ObjectDataCD objectData) {
			if (_favorites.Remove(objectData)) {
				var internalName = ObjectUtils.GetInternalName(objectData);
				for (var i = 0; i < _data.Favorites.Count; i++) {
					if (_data.Favorites[i].InternalName == internalName && _data.Favorites[i].Variation == objectData.variation) {
						_data.Favorites.RemoveAtSwapBack(i);
						break;
					}
				}
				
				_isDirty = true;
			}

			return false;
		}

		public void RemoveAllFavorites() {
			_favorites.Clear();
			_data.Favorites.Clear();
			_isDirty = true;
		}
		
		private void SetData(OptionsData data) {
			_data = data;
			_isDirty = false;
			
			_favorites = _data.Favorites.Select(favoriteData => new ObjectDataCD {
				objectID = API.Authoring.GetObjectID(favoriteData.InternalName),
				variation = favoriteData.Variation
			}).Where(objectData => objectData.objectID != ObjectID.None).ToHashSet();
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

		private void SetDefaults() {
			SetData(new OptionsData());
			Save();
		}
		
		public void Save() {
			if (!API.ConfigFilesystem.DirectoryExists(Main.InternalName))
				API.ConfigFilesystem.CreateDirectory(Main.InternalName);
			
			try {
				var serializedData = JsonConvert.SerializeObject(_data);
				API.ConfigFilesystem.Write(FilePath, Encoding.UTF8.GetBytes(serializedData));
			} catch (Exception ex) {
				Main.Log(nameof(Options), "Error while saving file");
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
				Main.Log(nameof(Options), "Error while loading file, using defaults");
				Main.Log(ex);
				SetDefaults();
			}
		}
		
		private class OptionsData {
			public int Version { get; set; } = CurrentVersion;
			public bool CheatMode { get; set; }
			public bool DiscoveryMode { get; set; }
			public bool ShowSourceMod { get; set; } = true;
			public bool ShowButtonHints { get; set; } = true;
			public List<OptionsObjectData> Favorites { get; set; } = new();
			public List<OptionsDetailsHistoryData> DetailsHistory { get; set; } = new();
		}
		
		private class OptionsObjectData {
			public string InternalName { get; set; }
			public int Variation { get; set; }
		}
		
		private class OptionsDetailsHistoryData {
			public ulong Timestamp { get; set; }
			public DetailsState State { get; set; }
		}
	}
}