using System;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Content.VanillaData;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

public class Main : IMod {
	public const string Version = "1.3.1";
	public const string InternalName = "ItemBrowser";
	public const string DisplayName = "Item Browser";

	internal static AssetBundle AssetBundle { get; private set; }
	
	public void EarlyInit() {
		Log(nameof(Main), $"Mod version: {Version}");

		AssetBundle = API.ModLoader.LoadedMods.First(modInfo => modInfo.Handlers.Contains(this)).AssetBundles[0];
	}

	public void Init() {
		OptionsManager.Instance.Init();

		ItemBrowserAPI.AddPlugin(new VanillaPlugin(), this);

		ModUtility.Bake();
	}

	public void Shutdown() { }

	public void Update() {
		OptionsManager.Instance.Update();
	}

	public void ModObjectLoaded(Object obj) { }

	public static void Log(string context, string text) {
		Debug.Log($"[{DisplayName}]: ({context}) {text}");
	}
	
	public static void Log(Exception ex) {
		Debug.LogException(ex);
	}
}