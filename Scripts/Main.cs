using System;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser;
using ItemBrowser.Api;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

public class Main : IMod {
	public const string Version = "0.1.0";
	public const string InternalName = "ItemBrowser";
	public const string DisplayName = "Item Browser";
	
	internal static AssetBundle AssetBundle { get; private set; }
	
	public void EarlyInit() {
		Debug.Log($"[{DisplayName}]: Mod version: {Version}");

		var modInfo = API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(this));
		AssetBundle = modInfo!.AssetBundles[0];
	}

	public void Init() {
		Options.Instance.Init();
		ItemBrowserAPI.Init();
		ModUtils.InitOnModLoad();
	}

	public void Shutdown() { }

	public void Update() {
		Options.Instance.Update();
	}

	public void ModObjectLoaded(Object obj) { }

	public static void Log(string context, string text) {
		Debug.Log($"[ItemBrowser]: ({context}) {text}");
	}
	
	public static void Log(Exception ex) {
		Debug.LogException(ex);
	}
}