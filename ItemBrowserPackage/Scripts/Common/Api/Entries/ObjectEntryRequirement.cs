namespace ItemBrowser.Common.Api.Entries {
	public abstract class ObjectEntryRequirement {
		public abstract bool IsFulfilled();
		
		public abstract string GetLocalizedDescription();
	}
}