using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class VirtualObjectList : ItemSlotsUIContainer, IScrollable {
		public Vector2Int size = Vector2Int.one;
		
		private float _currentScroll;
		private int _prevStartIndex;
		private int _prevSelectedSlot;
		private List<ObjectDataCD> _objects = new();
		private readonly Dictionary<int, int> _slotToObjectIndex = new();

		public override bool isShowing => gameObject.activeInHierarchy;
		public override int MAX_ROWS => size.y;
		public override int MAX_COLUMNS => size.x;

		public override UIScrollWindow uiScrollWindow => GetComponent<UIScrollWindow>();

		public void SetObjects(List<ObjectDataCD> objects, bool preserveScrollPosition) {
			if (_objects.SequenceEqual(objects))
				return;
			
			_objects = objects;
			_prevStartIndex = 0;
			
			UpdateList();
			
			if (!preserveScrollPosition)
				uiScrollWindow.ResetScroll();
		}

		public void TrySelectSlot(int slotIndex) {
			if (_objects.Count == 0 || UserInterfaceUtils.IsUsingMouseAndKeyboard)
				return;
			
			foreach (var slot in itemSlots) {
				if (slot.visibleSlotIndex == slotIndex)
					UserInterfaceUtils.SelectAndMoveMouseTo(slot);
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
		
		public void UpdateContainingElements(float scroll) {
			_currentScroll = scroll;
			UpdateList();
		}

		public bool IsBottomElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;

			return indexOfElement >= _objects.Count - _objects.Count % MAX_COLUMNS;
		}

		public bool IsTopElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;

			return indexOfElement < MAX_COLUMNS;
		}

		private int GetIndexOfElement(UIelement element) {
			for (var i = 0; i < itemSlots.Count && itemSlots[i].gameObject.activeSelf; i++) {
				if (itemSlots[i] == element)
					return _slotToObjectIndex.GetValueOrDefault(i);
			}

			return -1;
		}

		public float GetCurrentWindowHeight() {
			if (itemSlots.Count > MAX_COLUMNS) {
				var totalRows = math.ceil((float) _objects.Count / MAX_COLUMNS);
				return spread * totalRows - 1f / 16f;
			}

			return 0f;
		}

		public void UpdateList() {
			var num = math.max(0, ((int) math.floor(_currentScroll / spread) - 1) * MAX_COLUMNS);
			var num2 = math.max(0, ((int) math.floor(_currentScroll / spread) + MAX_ROWS) * MAX_COLUMNS);
			var num3 = spread * (num / MAX_COLUMNS);
			var sideStartPosition = GetSideStartPosition(MAX_COLUMNS);
			var num4 = 0f;

			var prevSelectedSlot = -1;
			if (Manager.ui.currentSelectedUIElement is VirtualObjectListItem prevSlot)
				prevSelectedSlot = prevSlot.visibleSlotIndex;

			_slotToObjectIndex.Clear();

			for (var i = 0; i < itemSlots.Count; i++) {
				var num5 = num + i;
				if (num5 >= num2 || num5 >= _objects.Count) {
					itemSlots[i].gameObject.SetActive(value: false);
					continue;
				}

				var slot = itemSlots[i] as VirtualObjectListItem;
				slot.visibleSlotIndex = i;
				slot.SetObjectData(_objects[num5], this);
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
			
			if (Manager.ui.currentSelectedUIElement is VirtualObjectListItem currentSlot)
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