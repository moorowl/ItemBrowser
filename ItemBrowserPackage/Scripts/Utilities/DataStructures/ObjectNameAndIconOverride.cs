using NaughtyAttributes;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Utilities.DataStructures {
	public class ObjectNameAndIconOverride : MonoBehaviour {
		[PickStringFromEnum(typeof(ObjectID))]
		public string appliesTo;
		public int appliesToVariation; 
		
		public bool overrideName = true;
		[ShowIf("overrideName")]
		public string name;
		
		public bool overrideIcon = true;
		[ShowIf("overrideIcon")]
		public Sprite icon;
		
		public bool showNameNote;
		[ShowIf("showNameNote")]
		public string nameNote;

		public ObjectDataCD AppliesToObjectData => new() {
			objectID = API.Authoring.GetObjectID(appliesTo),
			variation = appliesToVariation,
		};
	}
}