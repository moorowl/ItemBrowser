namespace ItemBrowser.Utilities.DataStructures {
	public readonly struct ValueBasedOnWorldState<T> {
		public delegate T GetValueDelegate();

		private readonly GetValueDelegate _getter;

		public ValueBasedOnWorldState(GetValueDelegate getter) {
			_getter = getter;
		}
		
		public ValueBasedOnWorldState(T staticValue) {
			_getter = () => staticValue;
		}
		
		public T Get() {
			return _getter();
		}

		public static implicit operator ValueBasedOnWorldState<T>(T staticValue) {
			return new ValueBasedOnWorldState<T>(staticValue);
		}
	}
}