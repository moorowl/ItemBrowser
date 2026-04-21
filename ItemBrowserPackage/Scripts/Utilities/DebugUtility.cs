using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PugMod;

namespace ItemBrowser.Utilities {
	public static class DebugUtility {
		private const string DebugDirectoryPath = Main.InternalName + "/Debug";
		
		public static void ExportData(string fileName, object data) {
			if (!API.ConfigFilesystem.DirectoryExists(DebugDirectoryPath))
				API.ConfigFilesystem.CreateDirectory(DebugDirectoryPath);
			
			var serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new DefaultContractResolver {
					NamingStrategy = new CamelCaseNamingStrategy(false, true, true)
				}
			});
			API.ConfigFilesystem.Write($"{DebugDirectoryPath}/{fileName}.json", Encoding.UTF8.GetBytes(serializedData.Replace("  ", "\t")));
		}
	}
}