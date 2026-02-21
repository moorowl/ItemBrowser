namespace ItemBrowser.Api.Entries.Requirements {
	public abstract class ObjectEntryRequirement {
		public readonly DataBlockAddress Guid = DataBlockAddress.NewAddress();
		
		public abstract bool IsFulfilled();
		
		public abstract string GetLocalizedDescription();
	}
}