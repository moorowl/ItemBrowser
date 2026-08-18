using System.Collections.Generic;
using ItemBrowser.Common.Api;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ItemBrowserButton : ButtonUIElement, IScrollItem {
		public TextAndFormatFields Title { get; set; } = new();
		public List<TextAndFormatFields> Description { get; set; } = new();
		
		public GameObject optionalToggledMarker;
		public GameObject[] markersToDisableWhenDisabled;
		public GameObject[] markersToEnableWhenDisabled;
		
		private float _height;
		private UIScrollWindow _scrollWindow;
		
		public bool IsToggled { get; set; }

		public override float localScrollPosition => _scrollWindow != null ? -Mathf.Abs(_scrollWindow.scrollingContent.position.y - transform.position.y) + _scrollWindow.scrollingContent.GetChild(0).localPosition.y : 0f;
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;

		public bool IsSelected { get; private set; }

		protected override void Awake() {
			base.Awake();
			
			_scrollWindow = GetComponentInParent<UIScrollWindow>();
			
			var boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null)
				_height = boxCollider.size.y;

			LateUpdate();
		}

		protected virtual void OnEnable() {
			LateUpdate();
		}
		
		public void OnScrollWindowChanged(UIScrollWindow scrollWindow) {
			_scrollWindow = scrollWindow;
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);

			if (onRightClick != null && onRightClick.GetPersistentEventCount() > 0)
				TryPlayClickSound();
		}

		protected void TryPlayClickSound() {
			if (canBeClicked && playClickSoundEffect)
				AudioManager.SfxUI(Manager.audio.InspectorFriendlySfxIDToSfxID(clickSoundEffect), clickSoundPitch, false, 1f, 0f);
		}

		protected void TryShowButtonHint(ButtonHint hint, params object[] formatFields) {
			if (IsSelected)
				ItemBrowserAPI.ItemBrowserUI.ShowButtonHint(hint, formatFields);
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			if (optionalSelectedMarker != null && optionalSelectedMarker.activeSelf && !canBeClicked)
				optionalSelectedMarker.SetActive(false);
			
			if (optionalToggledMarker != null)
				optionalToggledMarker.SetActive(canBeClicked && (IsToggled || leftClickIsHeldDown || (onRightClick != null && onRightClick.GetPersistentEventCount() > 0 && rightClickIsHeldDown)));

			if (markersToEnableWhenDisabled != null) {
				foreach (var marker in markersToEnableWhenDisabled)
					marker.gameObject.SetActive(!canBeClicked);
			}
			if (markersToDisableWhenDisabled != null) {
				foreach (var marker in markersToDisableWhenDisabled)
					marker.gameObject.SetActive(canBeClicked);
			}
		}
		
		public override void OnSelected() {
			base.OnSelected();

			IsSelected = true;
			_scrollWindow?.MoveScrollToIncludePosition(localScrollPosition, _height * 1.25f);
		}

		public override void OnDeselected(bool playEffect = true) {
			base.OnDeselected(playEffect);

			IsSelected = false;
		}

		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (!canBeClicked)
				return null;
			
			if (showHoverTitle)
				return base.GetHoverTitle();
			
			return Title.text == null ? null : Title;
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!canBeClicked)
				return null;
			
			return showHoverDesc ? base.GetHoverDescription() : Description;
		}

		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return UserInterfaceUtility.IsUsingMouseAndKeyboard ? HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR : HoverWindowAlignment.BOTTOM_RIGHT_OF_SCREEN;
		}
	}
}