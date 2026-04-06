using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Api.Entries.Requirements;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class BasicEntriesListRenderer : EntriesListRenderer {
		public static readonly BasicEntriesListRenderer Instance = new();
		
		private EntriesList _list;
		private ObjectDataCD _objectData;
		private List<ObjectEntry> _entries;
		private ObjectEntryDisplayBase _displayComponent;
		private readonly List<UIelement> _activePooledElements = new();

		public override bool SetEntries(EntriesList list, ObjectDataCD objectData, List<ObjectEntry> entries) {
			_list = list;
			_objectData = objectData;
			_entries = entries;
			
			return ItemBrowserAPI.Registry.EntryToDisplayComponent.TryGetValue(entries[0].GetType(), out _displayComponent);
		}

		public override void RenderList() {
			var allEntriesSorted = _displayComponent.SortEntries(_entries)
				.Select(entry => {
					var requirements = entry.Requirements.Select(requirement => (Requirement: requirement, IsFulfilled: requirement.IsFulfilled()))
						.OrderBy(requirement => requirement.IsFulfilled ? 1 : 0)
						.ToList();
					
					return (Entry: entry, Requirements: requirements, IsAllFulfilled: requirements.All(requirement => requirement.IsFulfilled));
				})
				.ToList();
			
			var allAvailableEntries = allEntriesSorted.Where(entry => entry.IsAllFulfilled).ToList();
			var allUnavailableEntries = allEntriesSorted.Where(entry => !entry.IsAllFulfilled).ToList();
			
			foreach (var entry in allAvailableEntries)
				AddEntry(entry);

			if (allUnavailableEntries.Count > 0) {
				var header = ItemBrowserAPI.GetPooledElement<EntriesUnavailableHeader>();
				header.SetAmount(allUnavailableEntries.Count);
				var headerHeight = UserInterfaceUtility.CalculateHeight(header);

				header.transform.SetParent(_list.container);
				_activePooledElements.Add(header);
				
				TotalHeight -= headerHeight / 2f;
				header.transform.localPosition = new Vector3(0f, TotalHeight, 0f);
				TotalHeight -= headerHeight / 2f;
				
				AddDivider();
				
				foreach (var entry in allUnavailableEntries)
					AddEntry(entry);	
			}
			
			foreach (var scrollItem in _list.container.GetComponentsInChildren<IScrollItem>())
				scrollItem.OnScrollWindowChanged(_list.scrollWindow);
		}

		public override void ClearList() {
			TotalHeight = 0f;
			
			for (var i = _activePooledElements.Count - 1; i >= 0; i--) {
				var element = _activePooledElements[i];

				foreach (var pugText in element.GetComponentsInChildren<PugText>(true)) {
					var wasActive = pugText.gameObject.activeSelf;
					pugText.Clear();
					pugText.gameObject.SetActive(wasActive);
				}

				ItemBrowserAPI.FreePooledElement(element);
			}

			_activePooledElements.Clear();
		}

		private void AddEntry((ObjectEntry Entry, List<(ObjectEntryRequirement Requirement, bool IsFulfilled)> Requirements, bool IsAllFulfilled) entry) {
			var display = (ObjectEntryDisplayBase) ItemBrowserAPI.GetPooledElement(_displayComponent.GetType());
			if (display == null)
				return;

			_activePooledElements.Add(display);

			var moreInfoButton = ItemBrowserAPI.GetPooledElement<EntryDescriptionButton>();

			foreach (var pugText in display.GetComponentsInChildren<PugText>(true)) {
				if (pugText.renderOnStart)
					pugText.Render();
			}

			display.SetEntryAndOccupy(_objectData, entry.Entry, moreInfoButton);
			display.Render();
			display.RenderDescription();
			
			AppendRequirements(entry, moreInfoButton);

			var displayHeight = UserInterfaceUtility.CalculateHeight(display);
			display.transform.SetParent(_list.container);
			display.transform.localPosition = new Vector3(0f, TotalHeight - 0.625f, 0f);
			TotalHeight -= displayHeight + (displayHeight - 1.25f) / 2.5f;

			moreInfoButton.transform.SetParent(_list.container);
			moreInfoButton.transform.localPosition = new Vector3(5.125f, display.transform.localPosition.y, 0f);

			if (moreInfoButton.LineCount <= 1)
				ItemBrowserAPI.FreePooledElement(moreInfoButton);
			else
				_activePooledElements.Add(moreInfoButton);

			AddDivider();
		}

		private void AddDivider() {
			var divider = ItemBrowserAPI.GetPooledElement<EntriesDivider>();
			_activePooledElements.Add(divider);

			TotalHeight -= _list.dividerPadding;
			divider.transform.SetParent(_list.container);
			divider.transform.localPosition = new Vector3(0f, TotalHeight, 0f);
			TotalHeight -= _list.dividerPadding;
			TotalHeight -= 1f / 16f;
		}
	}
}