using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Api.Pooling;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.Api {
	public abstract class ItemBrowserPlugin {
		internal LoadedMod AssociatedLoadedMod;

		public virtual bool AutomaticallyRegisterFromAssets => false;
		
		public virtual void OnEarlyRegister(ItemBrowserRegistry registry) { }
		
		public virtual void OnRegister(ItemBrowserRegistry registry) { }
		
		public virtual void OnLateRegister(ItemBrowserRegistry registry) { }
		
		public virtual void OnAutomaticallyRegisterFromAssets(ItemBrowserRegistry registry) {
			foreach (var asset in AssociatedLoadedMod.Assets) {
				if (asset is not GameObject gameObject)
					continue;

				if (gameObject.TryGetComponent<ObjectEntryDisplayBase>(out var displayComponent))
					registry.AddEntryDisplay(displayComponent);

				if (gameObject.TryGetComponent<PooledElement>(out var pooledElement))
					registry.AddPooledElement(pooledElement);
			}
		}
	}
}