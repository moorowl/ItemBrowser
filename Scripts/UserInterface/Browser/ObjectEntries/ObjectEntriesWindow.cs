using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ObjectEntriesWindow : ItemBrowserWindow {
		private const int MaxCategoryButtons = 8;
		
		[SerializeField]
		private DetailsPanel detailsPanel;
		[SerializeField]
		private SelectedItemSlot selectedItemSlot;
		[SerializeField]
		private ObjectEntriesList objectEntriesList;
		[SerializeField]
		private PugText selectedTypeText;
		[SerializeField]
		private ChangeCategoryAndTypeButton nextTypeButton;
		[SerializeField]
		private ChangeCategoryAndTypeButton prevTypeButton;
		[SerializeField]
		private PugText selectedCategoryText;
		[SerializeField]
		private ChangeCategoryAndTypeButton nextCategoryButton;
		[SerializeField]
		private ChangeCategoryAndTypeButton prevCategoryButton;
		[SerializeField]
		private ChangeCategoryAndTypeButton categoryButtonPrefab;
		[SerializeField]
		private Transform categoryButtonContainer;
		[SerializeField]
		private float categoryButtonGap;
		
		private ObjectDataCD _objectData;
		private List<List<ObjectEntry>> _entries = new();

		private readonly List<ChangeCategoryAndTypeButton> _categoryButtons = new();
		private readonly Stack<(ObjectDataCD ObjectData, ObjectEntryType SelectedType, int SelectedCategory, float ScrollProgress)> _history = new();
		
		public ObjectEntryType SelectedType { get; private set; }
		public int SelectedCategory { get; private set; }
		public bool HasAnyHistory => _history.Count > 0;
		public ObjectEntryType NextType => SelectedType switch {
			ObjectEntryType.Source => ObjectEntryType.Usage,
			ObjectEntryType.Usage => ObjectEntryType.Source,
			_ => throw new ArgumentOutOfRangeException()
		};
		public bool IsSelectedObjectNonObtainable { get; private set; }

		protected override void OnShow(bool isFirstTimeShowing) {
			TrySelectSelectedItemSlot();
			AdjustWindowPosition();
		}

		private void LateUpdate() {
			UpdateControllerInput();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtils.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;

			if (nextCategoryButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_IN_MAP))
				CycleToNextCategory();
			if (prevCategoryButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_OUT_MAP))
				CycleToPreviousCategory();

			if (nextTypeButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_NEXT_MAP_MARKER))
				CycleType();
			if (prevTypeButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SELECT_PREVIOUS_MAP_MARKER))
				CycleType();
		}

		private void TryInstantiateCategoryButtons() {
			if (_categoryButtons.Count > 0)
				return;
			
			for (var i = 0; i < MaxCategoryButtons; i++) {
				var button = Instantiate(categoryButtonPrefab, categoryButtonContainer);
				button.gameObject.SetActive(false);
				button.transform.localPosition = new Vector3(categoryButtonGap * i, 0f, 0f);
				
				_categoryButtons.Add(button);
			}
		}

		public bool PushObjectData(ObjectDataCD objectData, ObjectEntryType initialSelectedType, bool clearHistory) {
			if (objectData.Equals(_objectData) && initialSelectedType == SelectedType)
				return false;
			
			var entries = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(initialSelectedType, objectData.objectID, objectData.variation);
			if (!entries.Any())
				return false;

			if (clearHistory) {
				_history.Clear();
			} else if (_objectData.objectID != ObjectID.None && !_objectData.Equals(objectData)) {
				_history.Push((_objectData, SelectedType, SelectedCategory, objectEntriesList.CurrentScrollProgress));
			}

			_objectData = objectData;
			SetTypeAndCategory(initialSelectedType, 0);

			return true;
		}

		public bool PopObjectData() {
			if (!_history.TryPop(out var state))
				return false;
			
			_objectData = state.ObjectData;
			Main.Log("ObjectEntriesWindow", $"Restoring entry list state for {_objectData.objectID}:{_objectData.variation} ({state.SelectedType}/{state.SelectedCategory}, {state.ScrollProgress * 100f}%)");
			SetTypeAndCategory(state.SelectedType, state.SelectedCategory, state.ScrollProgress);

			return true;
		}

		public void Clear() {
			_history.Clear();
			_objectData = default;
		}
		
		private void AdjustWindowPosition() {
			transform.localPosition = new Vector3(Mathf.Round(detailsPanel.IsShowing ? -(detailsPanel.WindowWidth / 2f + (1f / 16f)) : 0f), transform.localPosition.y, transform.localPosition.z);
		}

		private void TrySelectSelectedItemSlot() {
			if (!UserInterfaceUtils.IsUsingMouseAndKeyboard)
				UserInterfaceUtils.SelectAndMoveMouseTo(selectedItemSlot);
		}
		
		public void SetTypeAndCategory(ObjectEntryType type, int category, float scrollProgress = 1f) {
			SelectedType = type;
			SelectedCategory = Math.Clamp(category, 0, Math.Max(_entries.Count - 1, 0));

			IsSelectedObjectNonObtainable = ObjectUtils.IsNonObtainable(_objectData.objectID, _objectData.variation);
			var allEntriesOfSelectedType = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(SelectedType, _objectData.objectID, _objectData.variation).ToList();
			var allEntriesOfOtherType = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(NextType, _objectData.objectID, _objectData.variation).ToList();
			
			_entries = allEntriesOfSelectedType
				.GroupBy(details => details.Category.GetTitle(IsSelectedObjectNonObtainable))
				.Select(group => group.ToList())
				.OrderByDescending(entries => entries.First().Category.Priority)
				.ToList();

			selectedItemSlot.SetObjectData(_objectData);
			TrySelectSelectedItemSlot();
			
			var typeHeaderTerm = IsSelectedObjectNonObtainable
				? $"ItemBrowser:ObjectEntryTypeHeader_NonObtainable/{SelectedType}"
				: $"ItemBrowser:ObjectEntryTypeHeader/{SelectedType}";
			selectedTypeText.Render(typeHeaderTerm);
			nextTypeButton.canBeClicked = false;
			prevTypeButton.canBeClicked = false;
			
			selectedCategoryText.gameObject.SetActive(false);
			nextCategoryButton.canBeClicked = false;
			prevCategoryButton.canBeClicked = false;

			TryInstantiateCategoryButtons();
			foreach (var button in _categoryButtons)
				button.gameObject.SetActive(false);
			for (var i = 0; i < Math.Min(_entries.Count, MaxCategoryButtons); i++) {
				var button = _categoryButtons[i];
				button.SetCategoryAndType(i, _entries[i].Count, SelectedType, allEntriesOfSelectedType.Count, _entries[i].First().Category);
				button.gameObject.SetActive(true);
			}

			if (allEntriesOfOtherType.Any()) {
				nextTypeButton.canBeClicked = true;
				nextTypeButton.SetCategoryAndType(0, 0, NextType, allEntriesOfOtherType.Count, allEntriesOfOtherType.First().Category);
				prevTypeButton.canBeClicked = true;
				prevTypeButton.SetCategoryAndType(0, 0, NextType, allEntriesOfOtherType.Count, allEntriesOfOtherType.First().Category);
			}
			
			detailsPanel.SetObjectData(_objectData);
			
			if (_entries.Count == 0)
				return;

			Debug.Log(category.ToString());
			var details = _entries[category];
			if (details.Count == 0)
				return;
			
			selectedCategoryText.gameObject.SetActive(true);
			selectedCategoryText.Render(details[0].Category.GetTitle(IsSelectedObjectNonObtainable));

			if (_entries.Count >= 2) {
				var nextCategoryIndex = SelectedCategory + 1;
				if (nextCategoryIndex >= _entries.Count)
					nextCategoryIndex = 0;
				var nextCategory = _entries[nextCategoryIndex].First().Category;
				
				var prevCategoryIndex = SelectedCategory - 1;
				if (prevCategoryIndex < 0)
					prevCategoryIndex = _entries.Count - 1;
				var prevCategory = _entries[prevCategoryIndex].First().Category;
				
				nextCategoryButton.canBeClicked = true;
				nextCategoryButton.SetCategoryAndType(nextCategoryIndex, 0, SelectedType, allEntriesOfSelectedType.Count, nextCategory);
				prevCategoryButton.canBeClicked = true;
				prevCategoryButton.SetCategoryAndType(prevCategoryIndex, 0, SelectedType, allEntriesOfSelectedType.Count, prevCategory);
			}
			
			objectEntriesList.SetEntries(_objectData, details, scrollProgress);
		}

		public void SetCategory(int category) {
			SetTypeAndCategory(SelectedType, category);
		}

		private void CycleToNextCategory() {
			var nextCategory = SelectedCategory + 1;
			if (nextCategory >= _entries.Count)
				nextCategory = 0;
			
			SetCategory(nextCategory);
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTypeOrCategory, this);
		}
		
		private void CycleToPreviousCategory() {
			var nextCategory = SelectedCategory - 1;
			if (nextCategory < 0)
				nextCategory = _entries.Count - 1;
			
			SetCategory(nextCategory);
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTypeOrCategory, this);
		}
		
		private void CycleType() {
			SetTypeAndCategory(NextType, SelectedCategory);
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTypeOrCategory, this);
		}
	}
}