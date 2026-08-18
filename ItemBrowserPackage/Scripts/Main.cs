using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Content.VanillaData;
using PinyinNet;
using PugMod;
using UnityEngine;
using Logger = ItemBrowser.Logger;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

public class Main : IMod {
	public const string Version = "1.5";
	public const string InternalName = "ItemBrowser";
	public const string DisplayName = "Item Browser";

	internal static AssetBundle AssetBundle { get; private set; }
	
	public void EarlyInit() {
		Logger.LogInfo($"Mod version: {Version}");

		AssetBundle = API.ModLoader.LoadedMods.First(modInfo => modInfo.Handlers.Contains(this)).AssetBundles[0];
	}

	public void Init() {
		OptionsManager.Instance.Init();
		PinyinConvert.Load();

		ItemBrowserAPI.AddPlugin(new VanillaPlugin(), this);

		ModUtility.Bake();
	}

	public void Shutdown() { }

	public void Update() {
		OptionsManager.Instance.Update();
	}

	public void ModObjectLoaded(Object obj) { }
}