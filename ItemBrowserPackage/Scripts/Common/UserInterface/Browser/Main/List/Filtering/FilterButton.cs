using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api.SortingAndFiltering;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class FilterButton : ItemBrowserButton {
		public ObjectListView objectListView;
		public SpriteRenderer toggledBackground;
		public SpriteRenderer[] icons;
		public PugText[] symbols;
		public ColorReplacer[] colorReplacers;
		public string appliesToTerm;
		public bool showCollectionProgress;
		
		public Filter Filter { get; set; }

		private FilterState _currentState;
		public FilterState CurrentState {
			get => _currentState;
			set {
				if (value == _currentState)
					return;
				
				_currentState = value;
				objectListView.OnFilterStateChanged(Filter);
			}
		}

		private int _filteredCount;
		private int _filteredCollectedCount;
		private float _lastUpdatedCount;

		public void SetFilter(Filter filter) {
			Filter = filter;
			ResetState();

			var showIcon = filter.Icon != ObjectID.None;
			var showSymbol = !showIcon && !string.IsNullOrWhiteSpace(filter.Symbol);

			foreach (var icon in icons)
				icon.gameObject.SetActive(showIcon);
			foreach (var symbol in symbols)
				symbol.gameObject.SetActive(showSymbol);

			if (showIcon) {
				var iconContainedObject = new ContainedObjectsBuffer {
					objectData = new ObjectDataCD {
						objectID = filter.Icon
					}
				};
				var iconObjectInfo = PugDatabase.GetObjectInfo(filter.Icon);
				var iconSprite = ObjectUtility.GetIcon(iconContainedObject.objectData, true);

				for (var i = 0; i < icons.Length; i++) {
					var icon = icons[i];
					icon.sprite = iconSprite;
					icon.material = UserInterfaceUtility.GetUISpriteColorReplaceMaterial();
					UserInterfaceUtility.ApplyObjectIconTransform(icon, iconObjectInfo, 1f);
					
					colorReplacers[i].UpdateColorReplacerFromObjectData(iconContainedObject);
					Manager.ui.ApplyAnyIconGradientMap(iconContainedObject, icon);
				}
			}

			if (showSymbol) {
				foreach (var symbol in symbols) {
					symbol.Render(filter.Symbol);
					// Offset 1px left because single letters don't center nicely
					symbol.transform.localPosition = new Vector3(symbol.displayedTextString.Length == 1 ? -1f / 16f : 0f, symbol.transform.localPosition.y, symbol.transform.localPosition.z);
				}
			}
		}

		public void ResetState() {
			CurrentState = Filter.DefaultState();
		}
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			CurrentState = CurrentState switch {
				FilterState.None => FilterState.Include,
				FilterState.Exclude => FilterState.Include,
				_ => FilterState.None
			};
		}
		
		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			CurrentState = CurrentState switch {
				FilterState.None => FilterState.Exclude,
				FilterState.Include => FilterState.Exclude,
				_ => FilterState.None
			};
		}

		protected override void LateUpdate() {
			IsToggled = CurrentState != FilterState.None;
			toggledBackground.color = GetStateColor(CurrentState).ColorWithNewAlpha(toggledBackground.color.a);
			
			base.LateUpdate();
			
			TryShowButtonHint(CurrentState == FilterState.Include ? ButtonHint.RemoveFilterPrimary : ButtonHint.IncludeFilter);
			TryShowButtonHint(CurrentState == FilterState.Exclude ? ButtonHint.RemoveFilterSecondary : ButtonHint.ExcludeFilter);	
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = Filter.Name,
				formatFields = Filter.NameFormatFields,
				dontLocalizeFormatFields = !Filter.LocalizeNameFormatFields
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			// TODO this sucks but whatever
			var descriptionFormatFields = new List<string> {
				API.Localization.GetLocalizedTerm(appliesToTerm ?? "ItemBrowser-General/AppliesToItems")
			};
			if (Filter.DescriptionFormatFields != null) {
				foreach (var term in Filter.DescriptionFormatFields)
					descriptionFormatFields.Add(Filter.LocalizeDescriptionFormatFields ? API.Localization.GetLocalizedTerm(term) ?? term : term);
			}

			var lines = new List<TextAndFormatFields> {
				new() {
					text = Filter.Description,
					formatFields = descriptionFormatFields.ToArray(),
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.DescriptionColor
				},
				new() {
					text = $"ItemBrowser-FilterStates/{CurrentState}",
					color = GetStateColor(CurrentState)
				}
			};

			if (showCollectionProgress) {
				if (Time.time >= _lastUpdatedCount + 1f) {
					var objectsInFilter = FilterResults.Create(Filter, objectListView.GetIncludedObjects()).Results;
					_filteredCount = objectsInFilter.Count;
					_filteredCollectedCount = objectsInFilter.Count(objectData => OptionsManager.Instance.HasTag(objectData, ObjectTagType.Collected));

					_lastUpdatedCount = Time.time;
				}
				
				lines[^1].paddingBeneath = UserInterfaceUtility.DescriptionPadding;
				lines.Add(new TextAndFormatFields {
					text = $"ItemBrowser-General/CollectedCount",
					formatFields = new[] {
						_filteredCollectedCount.ToString(),
						_filteredCount.ToString(),
						((_filteredCount == 0 ? 0f : _filteredCollectedCount / (float) _filteredCount) * 100f).ToString("0.##")
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtility.AlmostWhiteColor
				});
			}

			return lines;
		}

		private static Color GetStateColor(FilterState state) {
			return state switch {
				FilterState.Include => Color.green,
				FilterState.Exclude => Manager.ui.brokenColor,
				_ => Color.white
			};
		}
	}
}