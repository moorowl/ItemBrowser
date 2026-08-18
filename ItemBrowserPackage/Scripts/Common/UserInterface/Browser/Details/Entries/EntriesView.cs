using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class EntriesView : DetailsSubView {
		public const int MaxCategoryButtonsPerRow = 8;

		public ObjectEntryType entryType;
		public DetailsView detailsView;
		public EntriesList entriesList;
		public PugText selectedCategoryLabel;
		public PugText selectedCategoryCountLabel;
		public SwapCategoryButton nextCategoryButton;
		public SwapCategoryButton previousCategoryButton;
		public SwapCategoryButton categoryButtonPrefab;
		public Transform categoryButtonContainer;
		public float categoryButtonGap;
		public float categoryButtonAscend = 1f;
		
		private List<List<ObjectEntry>> _entries = new();
		private readonly List<SwapCategoryButton> _categoryButtons = new();
		private ObjectDataCD _lastSelectedObject;

		public int SelectedCategory { get; private set; }
		public string SelectedCategoryTerm { get; private set; }
		
		protected override void LateUpdate() {
			base.LateUpdate();
			UpdateControllerInput();
		}

		private void UpdateControllerInput() {
			if (UserInterfaceUtility.IsUsingMouseAndKeyboard)
				return;
			
			var inputModule = Manager.input.singleplayerInputModule;
			if (nextCategoryButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_IN_MAP))
				CycleToNextCategory();
			if (previousCategoryButton.canBeClicked && inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.ZOOM_OUT_MAP))
				CycleToPreviousCategory();
		}

		private void HideAllCategoryButtons() {
			foreach (var categoryButton in _categoryButtons)
				categoryButton.gameObject.SetActive(false);
		}

		private SwapCategoryButton GetFreeCategoryButton() {
			foreach (var categoryButton in _categoryButtons) {
				if (!categoryButton.gameObject.activeSelf) {
					categoryButton.gameObject.SetActive(true);
					return categoryButton;
				}
			}
			
			var instance = Instantiate(categoryButtonPrefab, categoryButtonContainer);
			instance.gameObject.SetActive(true);
			instance.transform.localPosition = new Vector3(
				categoryButtonGap * (_categoryButtons.Count % MaxCategoryButtonsPerRow),
				categoryButtonAscend * Mathf.Floor(_categoryButtons.Count / (float) MaxCategoryButtonsPerRow),
				0f
			);
				
			_categoryButtons.Add(instance);
			return instance;
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

			_lastSelectedObject = detailsView.SelectedObject;
			detailsView.TrySelectSelectedObjectSlot();
		}
		
		public void SetCategory(int category, float scrollProgress = 1f) {
			if (_lastSelectedObject.Equals(detailsView.SelectedObject))
				detailsView.AddCurrentStateToHistory();
			
			SelectedCategory = Math.Clamp(category, 0, Math.Max(_entries.Count - 1, 0));
			SelectedCategoryTerm = "";
			
			selectedCategoryLabel.gameObject.SetActive(false);
			selectedCategoryCountLabel.gameObject.SetActive(false);
			nextCategoryButton.canBeClicked = false;
			previousCategoryButton.canBeClicked = false;
			
			HideAllCategoryButtons();

			if (_entries.Count == 0)
				return;
			
			var entriesInCategory = _entries[SelectedCategory];
			if (entriesInCategory.Count == 0)
				return;
			
			for (var i = 0; i < _entries.Count; i++)
				GetFreeCategoryButton().SetCategory(i, _entries[i].Count, _entries[i][0].Category);
			
			selectedCategoryLabel.gameObject.SetActive(true);
			selectedCategoryLabel.Render(entriesInCategory[0].Category.GetTitle(detailsView.IsSelectedObjectNonObtainable));
			SelectedCategoryTerm = entriesInCategory[0].Category.GetTitle(detailsView.IsSelectedObjectNonObtainable);
			
			selectedCategoryCountLabel.gameObject.SetActive(true);
			selectedCategoryCountLabel.Render($"{category + 1}/{_entries.Count}");

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
			
			detailsView.AddCurrentStateToHistory();
		}

		private void CycleToNextCategory() {
			var nextCategory = SelectedCategory + 1;
			if (nextCategory >= _entries.Count)
				nextCategory = 0;
			
			SetCategory(nextCategory);
			UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.ChangeTabOrCategory, this);
		}
		
		private void CycleToPreviousCategory() {
			var nextCategory = SelectedCategory - 1;
			if (nextCategory < 0)
				nextCategory = _entries.Count - 1;
			
			SetCategory(nextCategory);
			UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.ChangeTabOrCategory, this);
		}
	}
}