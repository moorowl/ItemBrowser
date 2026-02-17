using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class EntriesView : DetailsSubView {
		private const int MaxCategoryButtons = 8;

		public ObjectEntryType entryType;
		public DetailsView detailsView;
		public EntriesList entriesList;
		public PugText selectedCategoryLabel;
		public SwapCategoryButton nextCategoryButton;
		public SwapCategoryButton previousCategoryButton;
		public SwapCategoryButton categoryButtonPrefab;
		public Transform categoryButtonContainer;
		public float categoryButtonGap;
		
		private List<List<ObjectEntry>> _entries = new();
		private readonly List<SwapCategoryButton> _categoryButtons = new();

		public int SelectedCategory { get; private set; }

		protected override void OnShow(bool isFirstTimeShowing) {
			detailsView.TrySelectSelectedObjectSlot();
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
			if (previousCategoryButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_OUT_MAP))
				CycleToPreviousCategory();
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
		
		public override void OnApplyState(DetailsState currentState, DetailsState previousState) {
			_entries = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(entryType, detailsView.SelectedObject)
				.GroupBy(details => details.Category.GetTitle(detailsView.IsSelectedObjectNonObtainable))
				.Select(group => group.ToList())
				.OrderByDescending(entries => entries.First().Category.Priority)
				.ToList();

			var category = entryType switch {
				ObjectEntryType.Source => currentState.EntriesSourceCategory,
				ObjectEntryType.Usage => currentState.EntriesUsageCategory,
				_ => 0
			};
			var scrollProgress = entryType switch {
				ObjectEntryType.Source => currentState.EntriesSourceScrollProgress,
				ObjectEntryType.Usage => currentState.EntriesUsageScrollProgress,
				_ => 0
			};
			SetCategory(category, scrollProgress);
		}
		
		public void SetCategory(int category, float scrollProgress = 1f) {
			SelectedCategory = Math.Clamp(category, 0, Math.Max(_entries.Count - 1, 0));
			
			selectedCategoryLabel.gameObject.SetActive(false);
			nextCategoryButton.canBeClicked = false;
			previousCategoryButton.canBeClicked = false;

			TryInstantiateCategoryButtons();
			foreach (var button in _categoryButtons)
				button.gameObject.SetActive(false);
			for (var i = 0; i < Math.Min(_entries.Count, MaxCategoryButtons); i++) {
				var button = _categoryButtons[i];
				button.SetCategory(i, _entries[i].Count, _entries[i].First().Category);
				button.gameObject.SetActive(true);
			}

			if (_entries.Count == 0)
				return;
			
			var entriesInCategory = _entries[SelectedCategory];
			if (entriesInCategory.Count == 0)
				return;
			
			selectedCategoryLabel.gameObject.SetActive(true);
			selectedCategoryLabel.Render(entriesInCategory[0].Category.GetTitle(detailsView.IsSelectedObjectNonObtainable));

			if (_entries.Count >= 2) {
				var nextCategoryIndex = SelectedCategory + 1;
				if (nextCategoryIndex >= _entries.Count)
					nextCategoryIndex = 0;
				var nextCategory = _entries[nextCategoryIndex].First().Category;
				
				var previousCategoryIndex = SelectedCategory - 1;
				if (previousCategoryIndex < 0)
					previousCategoryIndex = _entries.Count - 1;
				var prevCategory = _entries[previousCategoryIndex].First().Category;
				
				nextCategoryButton.canBeClicked = true;
				nextCategoryButton.SetCategory(nextCategoryIndex, _entries[nextCategoryIndex].Count, nextCategory);
				previousCategoryButton.canBeClicked = true;
				previousCategoryButton.SetCategory(previousCategoryIndex, _entries[previousCategoryIndex].Count, prevCategory);
			}
			
			entriesList.SetEntries(detailsView.SelectedObject, entriesInCategory, scrollProgress);
		}

		private void CycleToNextCategory() {
			var nextCategory = SelectedCategory + 1;
			if (nextCategory >= _entries.Count)
				nextCategory = 0;
			
			SetCategory(nextCategory);
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTabOrCategory, this);
		}
		
		private void CycleToPreviousCategory() {
			var nextCategory = SelectedCategory - 1;
			if (nextCategory < 0)
				nextCategory = _entries.Count - 1;
			
			SetCategory(nextCategory);
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.ChangeTabOrCategory, this);
		}
	}
}