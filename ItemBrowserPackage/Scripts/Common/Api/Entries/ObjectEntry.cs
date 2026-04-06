using System;
using System.Collections.Generic;
using ItemBrowser.Common.Api.Entries.Requirements;
using ItemBrowser.Common.UserInterface.Browser;

namespace ItemBrowser.Common.Api.Entries {
	public abstract record ObjectEntry {
		public abstract ObjectEntryCategory Category { get; }
		
		public virtual Type Renderer => typeof(BasicEntriesListRenderer);

		private readonly List<ObjectEntryRequirement> _requirements = new();
		public IEnumerable<ObjectEntryRequirement> Requirements => _requirements;

		public void AddRequirement(ObjectEntryRequirement requirement) {
			_requirements.Add(requirement);
		}

		public EntriesListRenderer CreateRenderer() {
			return (EntriesListRenderer) Activator.CreateInstance(Renderer);
		}
	}
}