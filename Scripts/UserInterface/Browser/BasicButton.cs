using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class BasicButton : ButtonUIElement {
		public TextAndFormatFields Title { get; set; } = new();
		public List<TextAndFormatFields> Description { get; set; } = new();

		public bool IsToggled { get; set; }

		[SerializeField]
		private GameObject optionalToggledMarker;
		
		private BoxCollider _boxCollider;
		private UIScrollWindow _scrollWindow;
		private Transform _displayTransform;

		public override float localScrollPosition => transform.localPosition.y + (_displayTransform != null ? _displayTransform.localPosition.y : 0f);
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;

		public bool IsSelected { get; private set; }
		public virtual float ScrollPadding => _boxCollider != null ? Mathf.Max(_boxCollider.size.x, _boxCollider.size.y) : 0f;

		protected override void Awake() {
			base.Awake();
			
			_boxCollider = GetComponent<BoxCollider>();
			_scrollWindow = GetComponentInParent<UIScrollWindow>();
			
			if (_scrollWindow != null && _scrollWindow.gameObject.GetComponent<ObjectEntriesList>() != null)
				_displayTransform = GetComponentInParent<ObjectEntryDisplayBase>().transform;

			LateUpdate();
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);
			
			if (onRightClick != null && onRightClick.GetPersistentEventCount() > 0 && canBeClicked && playClickSoundEffect)
				AudioManager.SfxUI(Manager.audio.InspectorFriendlySfxIDToSfxID(clickSoundEffect), clickSoundPitch, false, 1f, 0f);
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			if (optionalSelectedMarker != null && optionalSelectedMarker.activeSelf && !canBeClicked)
				optionalSelectedMarker.SetActive(false);
			
			if (optionalToggledMarker != null)
				optionalToggledMarker.SetActive(canBeClicked && (IsToggled || leftClickIsHeldDown || (onRightClick != null && onRightClick.GetPersistentEventCount() > 0 && rightClickIsHeldDown)));
		}
		
		public override void OnSelected() {
			base.OnSelected();

			IsSelected = true;
			_scrollWindow?.MoveScrollToIncludePosition(localScrollPosition, ScrollPadding);
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
			return UserInterfaceUtils.IsUsingMouseAndKeyboard ? HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR : HoverWindowAlignment.BOTTOM_RIGHT_OF_SCREEN;
		}
	}
}