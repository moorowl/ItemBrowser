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
		public VirtualObjectListHeader headerPrefab;
		public float spread;
		public GameObject itemSlotsRoot;
		public bool keepSlotsEnabledAfterInit;
		public bool autoPositionSlots = true;

		private Dictionary<VirtualObjectListLayout, List<SlotUIBase>> _listSlots;
		private List<VirtualObjectListHeader> _listHeaders;
		private float _currentScroll;
		private int _prevStartIndex;
		private int _prevSelectedSlot;
		private Dictionary<VirtualObjectListLayout, List<Entry>> _listEntries = new();
		private readonly Dictionary<int, int> _slotToEntryIndex = new();
		private VirtualObjectListLayout _layout;

		public override bool isShowing => gameObject.activeInHierarchy;
		public override UIScrollWindow uiScrollWindow => GetComponent<UIScrollWindow>();

		private void Awake() {
			TryInstantiateListSlots();
			TryInstantiateListHeaders();
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

		private void TryInstantiateListSlots() {
			if (_listSlots != null)
				return;
			
			itemSlotsRoot.SetActive(keepSlotsEnabledAfterInit);
			_listSlots = new Dictionary<VirtualObjectListLayout, List<SlotUIBase>>();
			_listEntries = new Dictionary<VirtualObjectListLayout, List<Entry>>();

			foreach (VirtualObjectListLayout layout in Enum.GetValues(typeof(VirtualObjectListLayout))) {
				_listSlots[layout] = new List<SlotUIBase>();
				_listEntries[layout] = new List<Entry>();
				
				GetLayoutInfo(layout, out var rows, out var columns, out _, out var prefab, out _);
				
				var slotIndex = 0;
				for (var y = 0; y < rows; y++) {
					for (var x = 0; x < columns; x++) {
						var listItem = Instantiate(prefab, itemSlotsRoot.transform);
						listItem.uiSlotXPosition = x;
						listItem.uiSlotYPosition = y;
						listItem.visibleSlotIndex = slotIndex;
						listItem.gameObject.SetActive(false);

						_listSlots[layout].Add(listItem);
					
						slotIndex++;
					}
				}
			}
		}
		
		private void TryInstantiateListHeaders() {
			if (_listHeaders != null)
				return;

			_listHeaders = new List<VirtualObjectListHeader>();
			GetLayoutInfo(VirtualObjectListLayout.Grid, out var rows, out var columns, out _, out _, out _);

			for (var y = 0; y < rows; y++) {
				for (var x = 0; x < columns; x++) {
					var listItem = Instantiate(headerPrefab, itemSlotsRoot.transform);
					listItem.gameObject.SetActive(false);

					_listHeaders.Add(listItem);
				}
			}
		}

		public void SetEntries(List<Entry> entries, int filteredOutCount, bool preserveScrollPosition) {
			_prevStartIndex = 0;

			var previousScrollPosition = uiScrollWindow.GetScrollPosition();
			
			GetLayoutInfo(VirtualObjectListLayout.Grid, out _, out var gridColumns, out _, out _, out var gridEntries);
			gridEntries.Clear();
			foreach (var entry in entries) {
				if (entry.Type == EntryType.Header) {
					var remainingOnRow = gridEntries.Count % gridColumns == 0 ? 0 : gridColumns - gridEntries.Count % gridColumns;
					for (var i = 0; i < remainingOnRow; i++)
						gridEntries.Add(Entry.Empty());
				}
				
				gridEntries.Add(entry);

				if (entry.Type == EntryType.Header) {
					for (var i = 1; i < gridColumns; i++)
						gridEntries.Add(Entry.Empty());
				}
			}
			
			GetLayoutInfo(VirtualObjectListLayout.List, out _, out _, out _, out _, out var listEntries);
			listEntries.Clear();
			listEntries.AddRange(entries);
			
			/*if (filteredOutCount > 0) {
				gridEntries.Add(Entry.ExtraAmount(filteredOutCount));
				listEntries.Add(Entry.ExtraAmount(filteredOutCount));
			}*/

			UpdateList();
			
			if (!preserveScrollPosition)
				uiScrollWindow.SetScrollValueImmediately(1f, this);
			else
				uiScrollWindow.ScrollToPosition(previousScrollPosition);
		}

		public void SetLayout(VirtualObjectListLayout layout) {
			GetLayoutInfo(_layout, out _, out _, out var previousListItems, out _, out _);
			GetLayoutInfo(layout, out _, out _, out var currentListItems, out _, out _);
			
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
			GetLayoutInfo(_layout, out _, out _, out _, out _, out var entries);
			
			if (entries.Count == 0 || UserInterfaceUtility.IsUsingMouseAndKeyboard)
				return;

			GetLayoutInfo(_layout, out _, out _, out var listItems, out _, out _);
			
			foreach (var listItem in listItems) {
				if (listItem.visibleSlotIndex == slotIndex)
					UserInterfaceUtility.SelectAndMoveMouseTo(listItem);
			}
		}
		
		public void UpdateContainingElements(float scroll) {
			var previousScroll = _currentScroll;
			_currentScroll = scroll;
			
			if (!Mathf.Approximately(previousScroll, _currentScroll))
				UpdateList();
		}

		public bool IsBottomElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;
			
			GetLayoutInfo(_layout, out _, out var columns, out _, out _, out var entries);

			return indexOfElement >= entries.Count - entries.Count % columns;
		}

		public bool IsTopElementSelected() {
			if (Manager.ui.currentSelectedUIElement == null)
				return false;

			var indexOfElement = GetIndexOfElement(Manager.ui.currentSelectedUIElement);
			if (indexOfElement == -1)
				return false;
			
			GetLayoutInfo(_layout, out _, out var columns, out _, out _, out _);

			return indexOfElement < columns;
		}

		private int GetIndexOfElement(UIelement element) {
			GetLayoutInfo(_layout, out _, out _, out var listItems, out _, out _);
			
			for (var i = 0; i < listItems.Count && listItems[i].gameObject.activeSelf; i++) {
				if (listItems[i] == element)
					return _slotToEntryIndex.GetValueOrDefault(i);
			}

			return -1;
		}

		public float GetCurrentWindowHeight() {
			GetLayoutInfo(_layout, out _, out var columns, out var listItems, out _, out var entries);
			
			if (listItems.Count > columns) {
				var totalRows = math.ceil((float) entries.Count / columns);
				return spread * totalRows - 1f / 16f;
			}

			return 0f;
		}

		public void UpdateList() {
			GetLayoutInfo(_layout, out var rows, out var columns, out var listSlots, out _, out var entries);
			
			var baseOffset = math.max(0, ((int) math.floor(_currentScroll / spread) - 1) * columns);
			var num2 = math.max(0, ((int) math.floor(_currentScroll / spread) + rows) * columns);
			var num3 = spread * (baseOffset / columns);
			var sideStartPosition = GetSideStartPosition(columns);
			var num4 = 0f;

			var prevSelectedSlot = -1;
			if (Manager.ui.currentSelectedUIElement is VirtualObjectListSlot prevSlot)
				prevSelectedSlot = prevSlot.visibleSlotIndex;

			_slotToEntryIndex.Clear();

			for (var i = 0; i < listSlots.Count; i++) {
				var entryToShowIndex = baseOffset + i;
				if (entryToShowIndex >= num2 || entryToShowIndex >= entries.Count) {
					listSlots[i].gameObject.SetActive(false);
					_listHeaders[i].gameObject.SetActive(false);
					continue;
				}
				
				var entry = entries[entryToShowIndex];

				if (i >= listSlots.Count || i >= num2)
					continue;

				var listHeader = _listHeaders[i];
				var listSlot = listSlots[i];

				switch (entry.Type) {
					case EntryType.Empty:
						listHeader.gameObject.SetActive(false);
						listSlot.gameObject.SetActive(false);
						
						break;
					case EntryType.Object:
						listHeader.gameObject.SetActive(false);
						listSlot.gameObject.SetActive(true);
						
						var objectSlot = (VirtualObjectListSlot) listSlot;
						objectSlot.visibleSlotIndex = i;
						objectSlot.SetObject(entry.ObjectData);

						var objectSlotOffsetInColumn = i % columns;
						var objectSlotRow = i / columns;
						objectSlot.transform.localPosition = new Vector3(
							sideStartPosition + objectSlotOffsetInColumn * spread,
							num4 - objectSlotRow * spread - num3,
							0f
						);
						objectSlot.OnDeselectSlot();

						break;
					case EntryType.ExtraAmount:
						listHeader.gameObject.SetActive(false);
						listSlot.gameObject.SetActive(true);
						
						var extraAmountSlot = (VirtualObjectListSlot) listSlot;
						extraAmountSlot.visibleSlotIndex = i;
						extraAmountSlot.SetExtraAmount(entry.Amount);

						var extraAmountSlotOffsetInColumn = i % columns;
						var extraAmountSlotRow = i / columns;
						extraAmountSlot.transform.localPosition = new Vector3(
							sideStartPosition + extraAmountSlotOffsetInColumn * spread,
							num4 - extraAmountSlotRow * spread - num3,
							0f
						);
						extraAmountSlot.OnDeselectSlot();
						
						break;
					case EntryType.Header:
						listHeader.gameObject.SetActive(true);
						listSlot.gameObject.SetActive(false);
						
						var header = _listHeaders[i];
						header.SetGroup(entry.Index, entry.Name, entry.Amount);
						header.transform.localPosition = new Vector3(
							0f,
							num4 - (i / columns) * spread - num3,
							0f
						);

						break;
				}

				_slotToEntryIndex[i] = entryToShowIndex;
			}

			if (_prevStartIndex != baseOffset) {
				_prevStartIndex = baseOffset;
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

		private void GetLayoutInfo(VirtualObjectListLayout layout, out int rows, out int columns, out List<SlotUIBase> listItems, out SlotUIBase prefab, out List<Entry> entries) {
			switch (layout) {
				case VirtualObjectListLayout.Grid:
					rows = size.y;
					columns = size.x;
					prefab = gridLayoutItemPrefab;
					listItems = _listSlots.GetValueOrDefault(VirtualObjectListLayout.Grid);
					entries = _listEntries[VirtualObjectListLayout.Grid];
					break;
				case VirtualObjectListLayout.List:
					rows = size.y;
					columns = 1;
					prefab = listLayoutItemPrefab;
					listItems = _listSlots.GetValueOrDefault(VirtualObjectListLayout.List);
					entries = _listEntries[VirtualObjectListLayout.List];
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(layout), layout, null);
			}
		}

		public readonly struct Entry {
			public readonly EntryType Type;
			public readonly int Index;
			public readonly string Name;
			public readonly int Amount;
			public readonly ObjectDataCD ObjectData;

			public Entry(EntryType type, int index, string name, int amount, ObjectDataCD objectData) {
				Type = type;
				Index = index;
				Name = name;
				Amount = amount;
				ObjectData = objectData;
			}

			public static Entry Empty() {
				return new Entry(EntryType.Empty, -1, null, 0, default);
			}
			
			public static Entry Object(ObjectDataCD objectData) {
				return new Entry(EntryType.Object, -1, null, 0, objectData);
			}
			
			public static Entry Header(int index, int amount, string name) {
				return new Entry(EntryType.Header, index, name, amount, default);
			}
			
			public static Entry ExtraAmount(int amount) {
				return new Entry(EntryType.ExtraAmount, -1, null, amount, default);
			}
		}

		public enum EntryType {
			Empty,
			Object,
			Header,
			ExtraAmount
		}
	}
}