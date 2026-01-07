using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Api.Entries {
	public abstract class ObjectEntryDisplay<T> : ObjectEntryDisplayBase where T : ObjectEntry {
		protected T Entry { get; private set; }
		protected ObjectDataCD RegisteredTo { get; private set; }
		protected MoreInfoButton MoreInfo => moreInfoButton;
		
		[SerializeField]
		private MoreInfoButton moreInfoButton;

		public override Type AssociatedEntry => typeof(T);

		public override void SetEntry(ObjectEntry entry, ObjectData objectData) {
			Entry = (T) entry;
			RegisteredTo = objectData;
		}

		public override IEnumerable<ObjectEntry> SortEntries(IEnumerable<ObjectEntry> entries) {
			return SortEntries(entries.Cast<T>());
		}

		public override void Render() {
			if (moreInfoButton != null) {
				moreInfoButton.Clear();
				moreInfoButton.AddLine(new TextAndFormatFields {
					text = Entry.Category.GetTitle(ObjectUtils.IsNonObtainable(RegisteredTo.objectID, RegisteredTo.variation))
				});
			}
			
			RenderSelf();
		}

		public virtual IEnumerable<T> SortEntries(IEnumerable<T> entries) {
			return entries;
		}

		public abstract void RenderSelf();

		public override float CalculateHeight() {
			var height = 0f;

			foreach (var boxCollider in GetComponentsInChildren<BoxCollider>())
				height = Mathf.Max(height, Mathf.Abs(boxCollider.transform.localPosition.y) + Mathf.Abs(boxCollider.size.y));
			
			foreach (var pugText in GetComponentsInChildren<PugText>())
				height = Mathf.Max(height, pugText.dimensions.height - Mathf.Abs((pugText.dimensions.y + pugText.transform.localPosition.y) / 12f));

			return height;
		}
	}
}