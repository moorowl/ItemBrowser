using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class SortOrderButton : ItemBrowserButton {
		public GridView gridView;
		public Sprite ascendingSprite;
		public Sprite descendingSprite;
		public List<SpriteRenderer> spritesToUpdate;

		private bool _previousState;

		protected override void LateUpdate() {
			base.LateUpdate();

			if (gridView.UseReverseSorting == _previousState)
				return;
			
			var newSprite = gridView.UseReverseSorting ? descendingSprite : ascendingSprite;
			foreach (var sr in spritesToUpdate)
				sr.sprite = newSprite;
				
			_previousState = gridView.UseReverseSorting;
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new() {
				new() {
					text = gridView.UseReverseSorting ? "ItemBrowser-SortingOrders/Descending" : "ItemBrowser-SortingOrders/Ascending",
					color = UserInterfaceUtility.DescriptionColor
				}
			};
		}
	}
}