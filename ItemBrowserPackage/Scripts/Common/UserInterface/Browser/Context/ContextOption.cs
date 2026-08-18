using System;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ContextOption {
		public readonly string Term;
		public readonly Action Function;
			
		public ContextOption(string term, Action function) {
			Term = term;
			Function = function;
		}
	}
}