using NaughtyAttributes;
using UnityEngine;

namespace ItemBrowser.Common.Api.Pooling {
	public class PooledElement : MonoBehaviour {
		public bool useAdvancedParameters;
		[ShowIf("useAdvancedParameters")]
		public int initialSize = 16;
		[ShowIf("useAdvancedParameters")]
		public int maxFreeSize = 16;
		[ShowIf("useAdvancedParameters")]
		public int maxSize = 1024;
		public bool overrideMaskInteractionAtRuntime;
		[ShowIf("overrideMaskInteractionAtRuntime")]
		public SpriteMaskInteraction maskInteraction;
	}
}