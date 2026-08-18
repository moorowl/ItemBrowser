using System;
using System.Collections.Generic;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Common.Api.Entries {
	public abstract record ObjectEntry {
		public abstract ObjectEntryCategory Category { get; }

		protected virtual Type Renderer => typeof(BasicEntriesListRenderer);

		public List<ObjectEntryRequirement> Requirements = new();

		public void AddRequirement(ObjectEntryRequirement requirement) {
			Requirements.Add(requirement);
		}

		public EntriesListRenderer CreateRenderer() {
			return (EntriesListRenderer) Activator.CreateInstance(Renderer);
		}
	}
}