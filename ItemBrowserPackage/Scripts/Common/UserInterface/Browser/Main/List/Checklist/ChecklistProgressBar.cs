using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ChecklistProgressBar : UIelement {
		public ObjectListView listView;
		public GameObject maskPivot;
		public SpriteRenderer fill;
		public Gradient fillGradient;
		public Gradient completedFillGradient;
		
		private int _includedObjectsCount;
		private int _includedObjectsCollectedCount;
		private static bool _forceShowComplete;

		private bool IsComplete => _includedObjectsCollectedCount >= _includedObjectsCount || _forceShowComplete;

		protected override void LateUpdate() {
			base.LateUpdate();

			UpdateFillColor();
		}

		public void UpdateProgress() {
			var includedObjects = listView.GetIncludedObjects();

			_includedObjectsCount = includedObjects.Count;
			_includedObjectsCollectedCount = includedObjects.Count(objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Collected));
			
			maskPivot.transform.localScale = new Vector3(_includedObjectsCount == 0 ? 0f : _includedObjectsCollectedCount / (float) _includedObjectsCount, 1f, 1f);
		}

		private void UpdateFillColor() {
			fill.color = (IsComplete ? completedFillGradient : fillGradient).Evaluate(Time.time % 1f);
		}

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = "ItemBrowser-General/ChecklistProgress"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = "ItemBrowser-General/CollectedCount",
					formatFields = new[] {
						_includedObjectsCollectedCount.ToString(),
						_includedObjectsCount.ToString(),
						((_includedObjectsCount == 0 ? 0f : _includedObjectsCollectedCount / (float) _includedObjectsCount) * 100f).ToString("0.##")
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				}
			};

			if (IsComplete) {
				lines[^1].paddingBeneath = UserInterfaceUtility.DescriptionPadding;
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser-General/ChecklistProgressComplete",
					color = fill.color
				});
			}
			
			return lines;
		}
		
		[CommandWithModSupport("itemBrowser.forceShowComplete")]
		public static void ForceShowComplete() {
			_forceShowComplete = !_forceShowComplete;
		}
	}
}