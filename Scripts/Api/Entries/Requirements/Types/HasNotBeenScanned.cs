using ItemBrowser.Utilities;
using PugMod;
using Unity.Collections;

namespace ItemBrowser.Api.Entries.Requirements.Types {
	public class HasNotBeenScanned : ObjectEntryRequirement {
		public readonly ObjectID ScannedObject;
		
		public HasNotBeenScanned(ObjectID scannedObject) {
			ScannedObject = scannedObject;
		}

		public override bool IsFulfilled() {
			using var scannedObjects = API.Client.GetEntityQuery(typeof(CanBeScannedCD)).ToComponentDataArray<CanBeScannedCD>(Allocator.Temp);

			for (var i = 0; i < scannedObjects.Length; i++) {
				if (scannedObjects[i].objectData.objectID == ScannedObject)
					return false;
			}

			return true;
		}

		public override string GetLocalizedDescription() {
			return string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/HasNotBeenScanned"),
				ObjectUtils.GetLocalizedDisplayNameOrDefault(ScannedObject)
			);
		}
	}
}