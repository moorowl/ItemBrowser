using ItemBrowser.Api.Entries;
using ItemBrowser.Api.Themes;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Api {
	public abstract class ItemBrowserPlugin {
		internal LoadedMod AssociatedLoadedMod;

		public abstract string AssociatedMod { get; }
		public virtual bool IsEnabled => ModUtils.IsLoaded(AssociatedMod);
		public virtual bool AutomaticallyRegisterFromAssets => false;
		
		public virtual void OnEarlyRegister(ItemBrowserRegistry registry) { }
		
		public virtual void OnRegister(ItemBrowserRegistry registry) { }
		
		public virtual void OnAutomaticallyRegisterFromAssets(ItemBrowserRegistry registry) {
			foreach (var asset in AssociatedLoadedMod.Assets) {
				if (asset is not GameObject gameObject)
					continue;

				if (gameObject.TryGetComponent<ObjectEntryDisplayBase>(out var displayComponent))
					registry.AddEntryDisplay(displayComponent);
				
				foreach (var overrides in gameObject.GetComponents<ObjectNameAndIconOverride>())
					registry.AddObjectNameAndIconOverride(overrides);
				
				if (gameObject.TryGetComponent<PooledElement>(out var pooledElement))
					registry.AddPooledElement(pooledElement);
			}
		}
	}
}