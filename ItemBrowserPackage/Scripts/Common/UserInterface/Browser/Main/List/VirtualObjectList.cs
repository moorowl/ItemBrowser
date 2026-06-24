using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using ItemBrowser.Utilities.Extensions;
using Pug.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class VirtualObjectList : UIelement, IScrollable {
		public Vector2Int size = Vector2Int.one;
		public SlotUIBase listLayoutItemPrefab;
		public SlotUIBase gridLayoutItemPrefab;
		public float spread;
		public GameObject itemSlotsRoot;
		public bool keepSlotsEnabledAfterInit;
		public bool autoPositionSlots = true;

		private Dictionary<VirtualObjectListLayout, List<SlotUIBase>> _listItems;
		private float _currentScroll;
		private int _prevStartIndex;
		private int _prevSelectedSlot;
		private List<ObjectDataCD> _objects = new();
		private readonly Dictionary<int, int> _slotToObjectIndex = new();
		private VirtualObjectListLayout _layout;

		public override bool isShowing => gameObject.activeInHierarchy;

		public override UIScrollWindow uiScrollWindow => GetComponent<UIScrollWindow>();

		private void Awake() {
			TryInstantiateListItems();
			SetLayout(OptionsManager.Instance.ListLayout);
		}

		private void OnEnable() {
			LateUpdate();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			if (_layout != OptionsManager.Instance.ListLayout)
				SetLayout(OptionsManager.Instance.ListLayout);
		}

		private void TryInstantiateListItems() {
			if (_listItems != null)
				return;
			
			itemSlotsRoot.SetActive(keepSlotsEnabledAfterInit);
			_listItems = new Dictionary<VirtualObjectListLayout, List<SlotUIBase>>();

			foreach (VirtualObjectListLayout layout in Enum.GetValues(typeof(VirtualObjectListLayout))) {
				_listItems[layout] = new List<SlotUIBase>();
				
				GetLayoutInfo(layout, out var rows, out var columns, out _, out var prefab);
				
				var slotIndex = 0;
				for (var y = 0; y < rows; y++) {
					for (var x = 0; x < columns; x++) {
						var listItem = Instantiate(prefab, itemSlotsRoot.transform);
						listItem.uiSlotXPosition = x;
						listItem.uiSlotYPosition = y;
						listItem.visibleSlotIndex = slotIndex;
						listItem.gameObject.SetActive(false);

						_listItems[layout].Add(listItem);
					
						slotIndex++;
					}
				}
			}
		}

		public void SetObjects(List<ObjectDataCD> objects, bool preserveScrollPosition) {
			if (_objects.SequenceEqual(objects))
				return;
			
			_objects = objects;
			_prevStartIndex = 0;
			
			UpdateList();
			uiScrollWindow.SetScrollValueImmediately(preserveScrollPosition ? uiScrollWindow.GetScrollValue() : 1f, this);
		}

		public void SetLayout(VirtualObjectListLayout layout) {
			GetLayoutInfo(_layout, out _, out _, out var previousListItems, out _);
			GetLayoutInfo(layout, out _, out _, out var currentListItems, out _);
			
			_layout = layout;
			_prevStartIndex = 0;
			
			foreach (var listItem in previousListItems)
				listItem.gameObject.SetActive(false);
			
			foreach (var listItem in currentListItems)
				listItem.gameObject.SetActive(true);
			
			UpdateList();
			uiScrollWindow.ResetScrollValueImmediately(this);
		}

		public void TrySelectListItem(int slotIndex) {
			if (_objects.Count == 0 || UserInterfaceUtility.IsUsingMouseAndKeyboard)
				return;

			GetLayoutInfo(_layout, out _, out _, out var listItems, out _);
			
			foreach (var listItem in listItems) {
				if (listItem.visibleSlotIndex == slotIndex)
					UserInterfaceUtility.SelectAndMoveMouseTo(listItem);
			}
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
			
			GetLayoutInfo(_layout, out _, out var columns, out _, out _);

			return indexOfElement >= _objects.Count - _objects.Count % columns;
		}

		public bool IsTopElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;
			
			GetLayoutInfo(_layout, out _, out var columns, out _, out _);

			return indexOfElement < columns;
		}

		private int GetIndexOfElement(UIelement element) {
			GetLayoutInfo(_layout, out _, out _, out var listItems, out _);
			
			for (var i = 0; i < listItems.Count && listItems[i].gameObject.activeSelf; i++) {
				if (listItems[i] == element)
					return _slotToObjectIndex.GetValueOrDefault(i);
			}

			return -1;
		}

		public float GetCurrentWindowHeight() {
			GetLayoutInfo(_layout, out _, out var columns, out var listItems, out _);
			
			if (listItems.Count > columns) {
				var totalRows = math.ceil((float) _objects.Count / columns);
				return spread * totalRows - 1f / 16f;
			}

			return 0f;
		}

		public void UpdateList() {
			GetLayoutInfo(_layout, out var rows, out var columns, out var listItems, out _);
			
			var num = math.max(0, ((int) math.floor(_currentScroll / spread) - 1) * columns);
			var num2 = math.max(0, ((int) math.floor(_currentScroll / spread) + rows) * columns);
			var num3 = spread * (num / columns);
			var sideStartPosition = GetSideStartPosition(columns);
			var num4 = 0f;

			var prevSelectedSlot = -1;
			if (Manager.ui.currentSelectedUIElement is VirtualObjectListSlot prevSlot)
				prevSelectedSlot = prevSlot.visibleSlotIndex;

			_slotToObjectIndex.Clear();

			for (var i = 0; i < listItems.Count; i++) {
				var num5 = num + i;
				if (num5 >= num2 || num5 >= _objects.Count) {
					listItems[i].gameObject.SetActive(value: false);
					continue;
				}

				var slot = listItems[i] as VirtualObjectListSlot;
				slot.visibleSlotIndex = i;
				slot.SetObject(_objects[num5]);
				var num6 = i % columns;
				var num7 = i / columns;
				slot.transform.localPosition = new Vector3(sideStartPosition + num6 * spread, num4 - num7 * spread - num3, 0f);
				slot.gameObject.SetActive(value: true);
				slot.OnDeselectSlot();

				_slotToObjectIndex[i] = num5;
			}

			if (_prevStartIndex != num) {
				_prevStartIndex = num;
				TrySelectListItem(_prevSelectedSlot);
			}
			
			if (Manager.ui.currentSelectedUIElement is VirtualObjectListSlot currentSlot)
				currentSlot.OnSelectSlot();
			
			_prevSelectedSlot = prevSelectedSlot;
		}

		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}

		private float GetSideStartPosition(int size) {
			return (0f - (size - 1) / 2f) * spread;
		}

		private void GetLayoutInfo(VirtualObjectListLayout layout, out int rows, out int columns, out List<SlotUIBase> listItems, out SlotUIBase prefab) {
			switch (layout) {
				case VirtualObjectListLayout.Grid:
					rows = size.y;
					columns = size.x;
					prefab = gridLayoutItemPrefab;
					listItems = _listItems.GetValueOrDefault(VirtualObjectListLayout.Grid);
					break;
				case VirtualObjectListLayout.List:
					rows = size.y;
					columns = 1;
					prefab = listLayoutItemPrefab;
					listItems = _listItems.GetValueOrDefault(VirtualObjectListLayout.List);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(layout), layout, null);
			}
		}
	}
}