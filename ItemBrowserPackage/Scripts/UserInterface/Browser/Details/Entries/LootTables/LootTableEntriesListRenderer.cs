using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Content.VanillaData.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class LootTableEntriesListRenderer : EntriesListRenderer {
		public static readonly LootTableEntriesListRenderer Instance = new();

		private EntriesList _list;
		private PrimaryLootTable _lootTable;
		private readonly List<UIelement> _activePooledElements = new();

		public override bool SetEntries(EntriesList list, ObjectDataCD objectData, List<ObjectEntry> entries) {
			_list = list;
			_lootTable = (PrimaryLootTable) entries[0];

			return true;
		}
		
		public override void RenderList() {
			AddChanceHeader();

			foreach (var pool in _lootTable.Pools) {
				if (pool.Entries.Count == 0)
					continue;

				AddHeader(pool);

				foreach (var item in pool.Entries)
					AddItem(item);
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


		private void AddChanceHeader() {
			var header = ItemBrowserAPI.GetPooledElement<LootTableChanceHeader>();
			var headerHeight = UserInterfaceUtils.CalculateHeight(header);

			header.transform.SetParent(_list.container);
			_activePooledElements.Add(header);

			TotalHeight -= headerHeight / 2f;
			header.transform.localPosition = new Vector3(0f, TotalHeight, 0f);
			TotalHeight -= headerHeight / 2f;

			AddDivider();
		}

		private void AddHeader(PrimaryLootTable.Pool pool) {
			var header = ItemBrowserAPI.GetPooledElement<LootTablePoolHeader>();

			if (header.Render(pool)) {
				var headerHeight = UserInterfaceUtils.CalculateHeight(header);

				header.transform.SetParent(_list.container);
				_activePooledElements.Add(header);

				TotalHeight -= headerHeight / 2f;
				header.transform.localPosition = new Vector3(0f, TotalHeight, 0f);
				TotalHeight -= headerHeight / 2f;

				AddDivider();
			}
			else {
				ItemBrowserAPI.FreePooledElement(header);
			}
		}

		private void AddItem(PrimaryLootTable.Entry entry) {
			var listItem = ItemBrowserAPI.GetPooledElement<LootTableListItem>();
			listItem.SetItem(entry);

			_activePooledElements.Add(listItem);

			var listItemHeight = UserInterfaceUtils.CalculateHeight(listItem);
			TotalHeight -= listItemHeight / 2f;
			listItem.transform.SetParent(_list.container);
			listItem.transform.localPosition = new Vector3(0f, TotalHeight, 0f);
			TotalHeight -= listItemHeight / 2f;

			if (ItemBrowserAPI.Registry.EntryToDisplayComponent.TryGetValue(entry.GetType(), out var displayComponent)) {
				var display = (ObjectEntryDisplayBase) ItemBrowserAPI.GetPooledElement(displayComponent.GetType());

				if (display != null) {
					var moreInfoButton = ItemBrowserAPI.GetPooledElement<EntryDescriptionButton>();
					moreInfoButton.transform.SetParent(_list.container);
					moreInfoButton.transform.localPosition = new Vector3(5.125f, listItem.transform.localPosition.y, 0f);
					moreInfoButton.Clear();

					display.SetEntryAndOccupy(default, entry, moreInfoButton);
					display.RenderDescription();

					var requirements = entry.Requirements.Select(requirement => (Requirement: requirement, IsFulfilled: requirement.IsFulfilled()))
						.OrderBy(requirement => requirement.IsFulfilled ? 1 : 0)
						.ToList();
					AppendRequirements((entry, requirements, requirements.All(requirement => requirement.IsFulfilled)), moreInfoButton);
					
					if (moreInfoButton.LineCount <= 1)
						ItemBrowserAPI.FreePooledElement(moreInfoButton);
					else
						_activePooledElements.Add(moreInfoButton);

					ItemBrowserAPI.FreePooledElement(display);
				}
			}
			
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