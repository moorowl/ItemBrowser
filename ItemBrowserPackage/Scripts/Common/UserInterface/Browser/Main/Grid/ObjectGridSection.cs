using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using Pug.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ObjectGridSection  : ItemSlotsUIContainer {
		public Vector2Int size = Vector2Int.one;
		public float headerHeight;

		private float _scrollValue;
		private ObjectGrid _grid;
		private int _prevStartIndex;
		private int _prevSelectedSlot;
		private List<ObjectDataCD> _objects = new();
		private readonly Dictionary<int, int> _slotToObjectIndex = new();

		public override bool isShowing => gameObject.activeInHierarchy;
		public override int MAX_ROWS => size.y;
		public override int MAX_COLUMNS => size.x;

		public float TotalHeight {
			get {
				var totalRows = math.ceil((float) _objects.Count / MAX_COLUMNS);
				return (spread * totalRows - 1f / 16f) + headerHeight;
			}
		}

		public void SetGrid(ObjectGrid grid) {
			_grid = grid;
		}
		
		public void SetObjects(List<ObjectDataCD> objects) {
			if (_objects.SequenceEqual(objects))
				return;
			
			_objects = objects;
			_prevStartIndex = 0;
			
			RefreshList();
		}

		public void TrySelectSlot(int slotIndex) {
			if (_objects.Count == 0 || UserInterfaceUtility.IsUsingMouseAndKeyboard)
				return;
			
			foreach (var slot in itemSlots) {
				if (slot.visibleSlotIndex == slotIndex)
					UserInterfaceUtility.SelectAndMoveMouseTo(slot);
			}
		}

		public override void ShowContainerUI() {
			base.ShowContainerUI();
			gameObject.SetActive(true);
		}

		public override void HideContainerUI() {
			base.HideContainerUI();
			gameObject.SetActive(false);
		}

		public void SetLocalScrollValue(float scrollValue) {
			_scrollValue = scrollValue;
			RefreshList();
		}
		
		public void RefreshList() {
			var num = math.max(0, ((int) math.floor(_scrollValue / spread) - 1) * MAX_COLUMNS);
			var num2 = math.max(0, ((int) math.floor(_scrollValue / spread) + MAX_ROWS) * MAX_COLUMNS);
			var num3 = spread * (num / MAX_COLUMNS);
			var sideStartPosition = GetSideStartPosition(MAX_COLUMNS);
			var num4 = 0f;

			var prevSelectedSlot = -1;
			if (Manager.ui.currentSelectedUIElement is ObjectGridSlot prevSlot)
				prevSelectedSlot = prevSlot.visibleSlotIndex;

			_slotToObjectIndex.Clear();

			for (var i = 0; i < itemSlots.Count; i++) {
				var num5 = num + i;
				if (num5 >= num2 || num5 >= _objects.Count) {
					itemSlots[i].gameObject.SetActive(value: false);
					continue;
				}

				var slot = itemSlots[i] as ObjectGridSlot;
				slot.visibleSlotIndex = i;
				slot.SetObject(_objects[num5], _grid);
				var num6 = i % MAX_COLUMNS;
				var num7 = i / MAX_COLUMNS;
				slot.transform.localPosition = new Vector3(sideStartPosition + num6 * spread, num4 - num7 * spread - num3, 0f);
				slot.gameObject.SetActive(value: true);
				slot.OnDeselectSlot();

				_slotToObjectIndex[i] = num5;
			}

			if (_prevStartIndex != num) {
				_prevStartIndex = num;
				TrySelectSlot(_prevSelectedSlot);
			}
			
			if (Manager.ui.currentSelectedUIElement is ObjectGridSlot currentSlot)
				currentSlot.OnSelectSlot();
			
			_prevSelectedSlot = prevSelectedSlot;
		}

		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}

		private float GetSideStartPosition(int size) {
			return (0f - (size - 1) / 2f) * spread;
		}
	}
}