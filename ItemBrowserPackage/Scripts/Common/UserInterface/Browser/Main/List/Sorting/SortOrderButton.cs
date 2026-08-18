using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SortOrderButton : ItemBrowserButton {
		public ObjectListView objectListView;
		public Sprite ascendingSprite;
		public Sprite descendingSprite;
		public List<SpriteRenderer> spritesToUpdate;

		private bool _previousState;

		protected override void LateUpdate() {
			base.LateUpdate();

			if (objectListView.UseReverseSorting == _previousState)
				return;
			
			var newSprite = objectListView.UseReverseSorting ? descendingSprite : ascendingSprite;
			foreach (var sr in spritesToUpdate)
				sr.sprite = newSprite;
				
			_previousState = objectListView.UseReverseSorting;
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new() {
				new() {
					text = objectListView.UseReverseSorting ? "ItemBrowser-SortingOrders/Descending" : "ItemBrowser-SortingOrders/Ascending",
					color = UserInterfaceUtility.DescriptionColor
				}
			};
		}
	}
}