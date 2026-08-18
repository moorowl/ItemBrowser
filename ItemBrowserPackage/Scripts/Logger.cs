using System;
using UnityEngine;

namespace ItemBrowser {
	internal static class Logger {
		public static void LogInfo(string text) {
			Debug.Log($"[{Main.DisplayName}]: {text}");
		}
		
		public static void LogWarning(string text) {
			Debug.LogWarning($"[{Main.DisplayName}]: {text}");
		}
		
		public static void LogError(string text) {
			Debug.LogError($"[{Main.DisplayName}]: {text}");
		}
	
		public static void LogException(Exception ex) {
			Debug.LogException(ex);
		}
	}
}