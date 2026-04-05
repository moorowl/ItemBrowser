using System.Collections.Generic;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures.SortingAndFiltering;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class FilterButton : ItemBrowserButton {
		public GridView gridView;
		public SpriteRenderer toggledBackground;
		public SpriteRenderer[] icons;
		public PugText[] symbols;
		public ColorReplacer[] colorReplacers;
		
		public Filter<ObjectDataCD> Filter { get; set; }

		private FilterState _currentState;
		public FilterState CurrentState {
			get => _currentState;
			set {
				if (value == _currentState)
					return;
				
				_currentState = value;
				gridView.RequestListRefresh(false);
			}
		}

		public void SetFilter(Filter<ObjectDataCD> filter) {
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
				var iconSprite = ObjectUtils.GetIcon(iconContainedObject.objectData, true);
				var iconSpriteSize = iconSprite.bounds.size;
				if (iconSpriteSize is { x: > 1f, y: > 1f } && ItemBrowserSlot.IsCarriedObject(iconObjectInfo)) {
					iconSpriteSize.x = 1f;
					iconSpriteSize.y = 1f;
				}
				var iconScale = Mathf.Min(1f / iconSpriteSize.x, 1f / iconSpriteSize.y);
				
				for (var i = 0; i < icons.Length; i++) {
					var icon = icons[i];
					icon.sprite = iconSprite;
					icon.material = UserInterfaceUtils.GetUISpriteColorReplaceMaterial();
					icon.transform.localPosition = iconObjectInfo.iconOffset;
					icon.transform.localScale = new Vector3(iconScale, iconScale, 1f);
					
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
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = Filter.Name,
				formatFields = Filter.NameFormatFields,
				dontLocalizeFormatFields = !Filter.LocalizeNameFormatFields
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = Filter.Description,
					formatFields = Filter.DescriptionFormatFields,
					dontLocalizeFormatFields = !Filter.LocalizeDescriptionFormatFields,
					color = UserInterfaceUtils.DescriptionColor
				},
				new() {
					text = $"ItemBrowser-FilterStates/{CurrentState}",
					color = GetStateColor(CurrentState)
				}
			};

			UserInterfaceUtils.AppendButtonHint(lines, "ItemBrowser-ButtonHints/" + (CurrentState == FilterState.Include ? "RemoveFilter" : "IncludeFilter"), "UIInteract");
			UserInterfaceUtils.AppendButtonHint(lines, "ItemBrowser-ButtonHints/" + (CurrentState == FilterState.Exclude ? "RemoveFilter" : "ExcludeFilter"), "UISecondInteract");
			
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