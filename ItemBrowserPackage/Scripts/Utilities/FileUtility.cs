using System.Text;
using Newtonsoft.Json;
using PugMod;

namespace ItemBrowser.Utilities {
	public static class FileUtility {
		private static void TryCreateDirectoryForPath(string path) {
			var directory = "";

			var subDirectories = path.Split('/');
			for (var i = 0; i < subDirectories.Length - 1; i++) {
				var subDirectory = subDirectories[i];
				
				if (i > 0)
					directory += "/";
				
				directory += subDirectory;
				
				if (!API.ConfigFilesystem.DirectoryExists(directory))
					API.ConfigFilesystem.CreateDirectory(directory);
			}
		}
		
		public static T ReadData<T>(string path) {
			path = $"{Main.InternalName}/{path}.json";
			
			var bytes = API.ConfigFilesystem.Read(path);
			return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes).Replace("\t", "  "));
		}
		
		public static void WriteData(string path, object data) {
			path = $"{Main.InternalName}/{path}.json";
			
			TryCreateDirectoryForPath(path);
			
			var serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore,
				/*ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy(false, true, true)
				}*/
			});
			API.ConfigFilesystem.Write(path, Encoding.UTF8.GetBytes(serializedData.Replace("  ", "\t")));
		}
	}
}