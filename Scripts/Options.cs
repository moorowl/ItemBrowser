using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemBrowser.UserInterface;
using ItemBrowser.Utilities;
using Newtonsoft.Json;
using PugMod;

namespace ItemBrowser {
	public static class Options {
		private const string FilePath = Main.InternalName + "/Config.json";
		private const int CurrentVersion = 0;
		
		private static RadicalMenu _settingsMenu;
		
		public static bool CheatMode { get; set; }
		public static bool ShowSourceMod { get; set; }
		public static bool DefaultDiscoveredFilter { get; set; }
		public static bool DefaultTechnicalFilter { get; set; }
		public static HashSet<ObjectDataCD> FavoritedObjects { get; set; }

		public static void EarlyInit() {
			/*MenuAdder.OnInit += () => {
				_settingsMenu = MenuAdder.AddMenu(19900, "ItemBrowser:Options");
				_settingsMenu.AddOptionFromPath(Main.AssetBundle, "Assets/ItemBrowser/Prefabs/MenuOptions.prefab");
			};
			MenuAdder.OnMenuClosed += (menu) => {
				if (menu == _settingsMenu)
					Save();
			};*/
		}
		
		public static void Init() {
			Load();
		}
		
		public static void ResetToDefault() {
			CheatMode = true;
			ShowSourceMod = true;
			DefaultDiscoveredFilter = false;
			DefaultTechnicalFilter = true;
			FavoritedObjects = new HashSet<ObjectDataCD>();

			Main.Log(nameof(Options), "Reset to defaults");
			Save();
		}
		
		public static void Save() {
			Main.Log(nameof(Options), "Saving file...");
			
			if (!API.ConfigFilesystem.DirectoryExists(Main.InternalName))
				API.ConfigFilesystem.CreateDirectory(Main.InternalName);
			
			try {
				var serializedData = JsonConvert.SerializeObject(SerializeData());
				API.ConfigFilesystem.Write(FilePath, Encoding.UTF8.GetBytes(serializedData));
			} catch (Exception ex) {
				Main.Log(nameof(Options), "Error while saving file");
				Main.Log(ex);
			}
		}
		
		private static void Load() {
			Main.Log(nameof(Options), "Loading file...");
			
			if (!API.ConfigFilesystem.FileExists(FilePath)) {
				ResetToDefault();
				return;
			}
			
			try {
				var serializedData = JsonConvert.DeserializeObject<SerializedData>(Encoding.UTF8.GetString(API.ConfigFilesystem.Read(FilePath)));
				DeserializeData(serializedData);
			} catch (Exception ex) {
				Main.Log(nameof(Options), "Error while loading file, using defaults");
				Main.Log(ex);
				ResetToDefault();
			}
		}

		private static SerializedData SerializeData() {
			return new SerializedData {
				Version = CurrentVersion,
				CheatMode = CheatMode,
				ShowSourceMod = ShowSourceMod,
				DefaultDiscoveredFilter = DefaultDiscoveredFilter,
				DefaultTechnicalFilter = DefaultTechnicalFilter,
				FavoritedObjects = FavoritedObjects.Select(x => new SerializedData.FavoritedObject {
					InternalName = ObjectUtils.GetInternalName(x.objectID),
					Variation = x.variation,
				}).ToList()
			};
		}

		private static void DeserializeData(SerializedData data) {
			CheatMode = data.CheatMode;
			ShowSourceMod = data.ShowSourceMod;
			DefaultDiscoveredFilter = data.DefaultDiscoveredFilter;
			DefaultTechnicalFilter = data.DefaultTechnicalFilter;
			FavoritedObjects = data.FavoritedObjects.Select(x => new ObjectDataCD {
				objectID = API.Authoring.GetObjectID(x.InternalName),
				variation = x.Variation
			}).ToHashSet();
		}

		private class SerializedData {
			public class FavoritedObject {
				public string InternalName;
				public int Variation;
			}

			public int Version;
			public bool CheatMode;
			public bool ShowSourceMod;
			public bool DefaultDiscoveredFilter;
			public bool DefaultTechnicalFilter;
			public List<FavoritedObject> FavoritedObjects;
		}
	}
}