using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class EntryDescriptionButton : ItemBrowserButton {
		private readonly List<TextAndFormatFields> _lines = new();
		private float _showDescriptionUntil;
		
		public int LineCount => _lines.Count;

		public void AddLine(TextAndFormatFields line) {
			_lines.Add(line);
		}

		public void AddPadding(float amount = UserInterfaceUtility.DescriptionPadding) {
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
			var lines = new List<TextAndFormatFields>();
			
			if (!CanShowDescription(out var temporaryTimeRemaining)) {
				UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/DiscoverTemporarily", "UIInteract");
				return lines;
			}
			
			lines = _lines.Skip(1).ToList();

			if (temporaryTimeRemaining > 0f) {
				if (temporaryTimeRemaining <= 99f) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/DiscoveredTemporarilySeconds",
						formatFields = new[] {
							Mathf.CeilToInt(temporaryTimeRemaining).ToString()
						},
						dontLocalizeFormatFields = true,
						color = ItemBrowserAPI.ItemBrowserUI.GetTemporarilyDiscoveredColor()
					});
				} else {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/DiscoveredTemporarily",
						color = ItemBrowserAPI.ItemBrowserUI.GetTemporarilyDiscoveredColor()
					});
				}
			}

			return lines;
		}
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!CanShowDescription(out _))
				ShowDescriptionTemporarily();
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);

			if (!CanShowDescription(out _))
				ShowDescriptionTemporarily();
		}

		protected override void OnEnable() {
			base.OnEnable();

			_showDescriptionUntil = 0f;
		}

		private void ShowDescriptionTemporarily() {
			_showDescriptionUntil = Time.time + 10f;
		}

		private bool CanShowDescription(out float temporaryTimeRemaining) {
			temporaryTimeRemaining = 0f;

			if (!OptionsManager.Instance.DiscoveryMode)
				return true;
			
			temporaryTimeRemaining = Mathf.Max(_showDescriptionUntil - Time.time, 0f);
			return temporaryTimeRemaining > 0f;
		}
	}
}