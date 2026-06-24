using System;
using System.Collections.Generic;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class OptionsEntryType {
		public Action OnLeftClick { get; set; }
		public Action OnRightClick { get; set; }
		public Action<List<TextAndFormatFields>, bool> GetDescription { get; set; }
		public Action<PugText, bool> UpdateValueText { get; set; }
		public Func<bool> CanBeClicked { get; set; }
	}
}