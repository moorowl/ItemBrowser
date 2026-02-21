using System;
using System.Collections.Generic;
using ItemBrowser.UserInterface.Browser;

namespace ItemBrowser.Api.Entries {
	public abstract class ObjectEntryDisplayBase : UIelement {
		public abstract Type AssociatedEntry { get; }
		
		public abstract bool ShouldRerender { get; }

		public abstract void SetEntryAndOccupy(ObjectDataCD objectData, ObjectEntry entry, EntryDescriptionButton entryDescriptionButton);

		public abstract IEnumerable<ObjectEntry> SortEntries(IEnumerable<ObjectEntry> entries);
		
		public abstract void Render();

		public abstract void RenderDescription();
	}
}