using System.Collections.Generic;
using System.Linq;

namespace ItemBrowser.UserInterface.Browser {
	public class EntryDescriptionButton : ItemBrowserButton {
		private readonly List<TextAndFormatFields> _lines = new();
		
		public void AddLine(TextAndFormatFields line) {
			_lines.Add(line);
		}

		public void AddPadding(float amount = 0.125f) {
			if (_lines.Count == 0)
				return;
			
			_lines[^1].paddingBeneath += amount;
		}
		
		public void Clear() {
			_lines.Clear();
		}

		public override TextAndFormatFields GetHoverTitle() {
			return _lines.Count == 0 ? null : _lines[0];
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return _lines.Skip(1).ToList();
		}
	}
}