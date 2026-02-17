using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;

namespace ItemBrowser.Api.Entries {
	public abstract class ObjectEntryDisplay<T> : ObjectEntryDisplayBase where T : ObjectEntry {
		private ObjectDataCD _registeredTo;
		private T _entry;
		private EntryDescriptionButton _entryDescriptionButton;
		
		public override Type AssociatedEntry => typeof(T);

		private bool _shouldRerender;
		public override bool ShouldRerender => _shouldRerender;

		protected void RequestRerender() {
			_shouldRerender = true;
		}
		
		public override void SetEntryAndOccupy(ObjectDataCD objectData, ObjectEntry entry, EntryDescriptionButton entryDescriptionButton) {
			_entry = (T) entry;
			_entryDescriptionButton = entryDescriptionButton;
		}

		public override IEnumerable<ObjectEntry> SortEntries(IEnumerable<ObjectEntry> entries) {
			return OnSort(entries.Cast<T>());
		}

		public override void Render() {
			_shouldRerender = false;
			OnRender(_entry);
		}

		public override void RenderDescription() {
			_entryDescriptionButton.Clear();
			_entryDescriptionButton.AddLine(new TextAndFormatFields {
				text = _entry.Category.GetTitle(ObjectUtils.IsNonObtainable(_registeredTo.objectID, _registeredTo.variation))
			});
			OnRenderDescription(_entry, _entryDescriptionButton);
		}

		public virtual IEnumerable<T> OnSort(IEnumerable<T> entries) {
			return entries;
		}

		protected abstract void OnRender(T entry);
		
		protected abstract void OnRenderDescription(T entry, EntryDescriptionButton description);
	}
}