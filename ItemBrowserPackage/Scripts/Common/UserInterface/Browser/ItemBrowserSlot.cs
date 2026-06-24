using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Common.Input;
using ItemBrowser.Common.Options;
using ItemBrowser.Common.UserInterface.SlotIcons;
using ItemBrowser.Utilities;
using ItemBrowser.Utilities.DataStructures;
using PlayerEquipment;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class ItemBrowserSlot : SlotUIBase, IScrollItem {
		private static SlotIcon EmptyIcon => new BasicSlotIcon(new ObjectDataCD());
		
		public ColorReplacer colorReplacer;
		public Sprite[] rarityBorders;
		public GameObject favoritedBorder;
		public SpriteRenderer missingIcon;
		public bool preferSmallIcons;
		public bool linksSource;
		public bool linksUsage;
		public bool showAmountInTitle;
		public bool alwaysShowAmount;
		public bool alwaysDiscovered;

		private SlotIcon _icon = EmptyIcon;
		public SlotIcon Icon {
			get => _icon;
			set {
				_icon = value ?? EmptyIcon;
				UpdateVisuals();
			}
		}
		
		public bool IsSelected => hoverBorder.gameObject.activeSelf;
		public bool IsFavorited => OptionsManager.Instance.HasTag(FavoritedKey, ObjectTagType.Favorited);
		public bool CanBeFavorited => FavoritedKey.objectID != ObjectID.None && HasBeenDiscovered && !HasBeenDiscoveredTemporarily;
		public bool HasBeenDiscovered => alwaysDiscovered || _icon.HasBeenDiscovered(this, out _);
		public bool HasBeenDiscoveredTemporarily => _icon.HasBeenDiscovered(this, out var temporaryTimeRemaining) && temporaryTimeRemaining > 0f;
		private ObjectDataCD FavoritedKey => new() {
			objectID = Icon.ContainedObject.objectID,
			variation = Icon.ContainedObject.variation
		};

		private float _height;
		private UIScrollWindow _scrollWindow;
		private bool _wasDiscovered;
		
		public override float localScrollPosition => _scrollWindow != null ? -Mathf.Abs(_scrollWindow.scrollingContent.position.y - transform.position.y) + _scrollWindow.scrollingContent.GetChild(0).localPosition.y : 0f;
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;
		
		public static bool CanCheatInObjects => OptionsManager.Instance.CheatMode && ClientWorldStateSystem.IsAdminOrInCreative;
		
		protected override void Awake() {
			base.Awake();
			
			_scrollWindow = GetComponentInParent<UIScrollWindow>();
			
			var boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null)
				_height = boxCollider.size.y;

			icon.material = UserInterfaceUtility.GetUISpriteColorReplaceMaterial();
			UpdateVisuals();
		}

		private void OnEnable() {
			UpdateVisuals();
		}

		public void OnScrollWindowChanged(UIScrollWindow scrollWindow) {
			_scrollWindow = scrollWindow;
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			_icon.Update(this);
			
			if (favoritedBorder != null) {
				UpdateFavoriting();
				favoritedBorder.gameObject.SetActive(IsFavorited);
			}

			var isDiscovered = HasBeenDiscovered;
			if (isDiscovered != _wasDiscovered) {
				UpdateVisuals();
				_wasDiscovered = isDiscovered;
			}

			if (IsSelected)
				UpdateCheatObjectIn();
		}

		private void UpdateFavoriting() {
			if (IsSelected && CanBeFavorited && InputHelper.IsToggleFavoritePressed) {
				if (IsFavorited) {
					UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.Unfavorite, this);
					OptionsManager.Instance.RemoveTag(FavoritedKey, ObjectTagType.Favorited);
				} else {
					UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.Favorite, this);
					OptionsManager.Instance.AddTag(FavoritedKey, ObjectTagType.Favorited);
				}

				OnFavoritedStateChanged();
			}
		}

		private void UpdateCheatObjectIn() {
			var containedObjectData = _icon.ContainedObject.objectData;
			if (containedObjectData.objectID == ObjectID.None || !InputHelper.IsSpawnItemPressed || !CanCheatInObjects)
				return;
			
			var player = Manager.main.player;
			player.playerCommandSystem.CreateAndDropEntity(containedObjectData.objectID, player.WorldPosition, GetAmountToPickUp(containedObjectData).Amount, player.entity, containedObjectData.variation);
			UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.AddObjectToInventory, this);
			PlayBumpAnimation();
		}

		public void PlayBumpAnimation() {
			Manager.main.StartCoroutine(PlayBumpAnimationWhenShowing());
		}

		private IEnumerator PlayBumpAnimationWhenShowing() {
			yield return new WaitUntil(() => gameObject.activeInHierarchy);
			SetAnimationTrigger(AnimID.scaleUp);
		}

		public virtual void OnFavoritedStateChanged() { }

		public override void OnSelected() {
			_scrollWindow?.MoveScrollToIncludePosition(localScrollPosition, _height * 1.25f);
			OnSelectSlot();
		}

		public override void OnDeselected(bool playEffect = true) {
			OnDeselectSlot();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!HasBeenDiscovered) {
				SetDiscoveredTemporarily();
				return;
			}

			if (linksSource && !(_icon.ShowDetails(this, DetailsTab.Sources) || (!UserInterfaceUtility.IsUsingMouseAndKeyboard && _icon.ShowDetails(this, DetailsTab.Usages))))
				UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.NoSourcesOrUsages, this);
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);

			if (!HasBeenDiscovered) {
				SetDiscoveredTemporarily();
				return;
			}

			if (linksUsage && !_icon.ShowDetails(this, DetailsTab.Usages))
				UserInterfaceUtility.PlaySound(UserInterfaceUtility.MenuSound.NoSourcesOrUsages, this);
		}

		private void SetDiscoveredTemporarily() {
			_icon.SetTemporarilyDiscovered(this, 10f);
		}

		public override TextAndFormatFields GetHoverTitle() {
			var title = _icon.GetHoverTitle(this);
			var visualObject = _icon.VisualObject;
			var amount = _icon.Amount;

			if (visualObject.objectID != ObjectID.None && HasBeenDiscovered && showAmountInTitle && (amount.Max > 1 || alwaysShowAmount)) {
				title = new TextAndFormatFields {
					text = "ItemBrowser-General/NameAndAmountFormat",
					formatFields = new[] {
						title.dontLocalize ? title.text : API.Localization.GetLocalizedTerm(title.text) ?? title.text,
						amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString()
					},
					dontLocalizeFormatFields = true,
					color = title.color
				};
			}

			return title;
		}
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = _icon.GetHoverDescription(this) ?? new List<TextAndFormatFields>();

			if (!HasBeenDiscovered) {
				UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/DiscoverTemporarily", "UIInteract");
				return lines;
			}

			var containedObjectData = _icon.ContainedObject.objectData;
			if (containedObjectData.objectID != ObjectID.None) {
				if (IsFavorited) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/Favorited",
						color = Color.yellow
					});
				}
			}
			
			if (_icon.HasBeenDiscovered(this, out var temporaryTimeRemaining) && temporaryTimeRemaining > 0f) {
				if (temporaryTimeRemaining <= 99f) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/DiscoveredTemporarilySeconds",
						formatFields = new[] {
							Mathf.CeilToInt(temporaryTimeRemaining).ToString()
						},
						dontLocalizeFormatFields = true,
						color = ItemBrowserAPI.ItemBrowserUI.GetTemporarilyDiscoveredColor()
					});
				} else {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/DiscoveredTemporarily",
						color = ItemBrowserAPI.ItemBrowserUI.GetTemporarilyDiscoveredColor()
					});
				}
			}

			if (containedObjectData.objectID != ObjectID.None) {
				var hasSources = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, containedObjectData).Any();
				var hasUsages = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Usage, containedObjectData).Any();
				
				if (linksSource && hasSources)
					UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/ViewSource", "UIInteract");

				if (linksUsage && hasUsages) {
					if (!hasSources && !UserInterfaceUtility.IsUsingMouseAndKeyboard)
						UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/ViewUsage", "UIInteract");

					UserInterfaceUtility.AppendButtonHint(lines, "ItemBrowser-ButtonHints/ViewUsage", "UISecondInteract");
				}

				if (CanCheatInObjects)
					UserInterfaceUtility.AppendButtonHint(lines, GetAmountToPickUp(containedObjectData).Hint, "ControlMapper/ItemBrowser-SpawnItem");
				
				if (CanBeFavorited)
					UserInterfaceUtility.AppendButtonHint(lines, IsFavorited ? "ItemBrowser-ButtonHints/RemoveFavorite" : "ItemBrowser-ButtonHints/AddFavorite", "ToggleLocking");
			}
			
			return lines;
		}
		
		public override List<TextAndFormatFields> GetHoverStats(bool previewReinforced) {
			return _icon.GetHoverStats(this, previewReinforced);
		}
		
		public override bool GetDurabilityOrFullnessOrXp(out int durability, out int maxDurability, out AmountType amountType) {
			durability = 0;
			maxDurability = 0;
			amountType = AmountType.Durability;
			
			var slotObject = GetSlotObject();
			if (slotObject.objectID == ObjectID.None)
				return false;

			if (PugDatabase.HasComponent<DurabilityCD>(slotObject.objectData)) {
				maxDurability = PugDatabase.GetComponent<DurabilityCD>(slotObject.objectData).maxDurability;
				durability = maxDurability;
				return true;
			}
			
			if (PugDatabase.HasComponent<FullnessCD>(slotObject.objectData)) {
				amountType = AmountType.Fullness;
				maxDurability = PugDatabase.GetComponent<FullnessCD>(slotObject.objectData).maxFullness;
				durability = maxDurability;
				return true;
			}
			
			if (PugDatabase.HasComponent<PetCD>(slotObject.objectData)) {
				if (PetExtensions.IsAtMaxLevel(slotObject.amount))
					return false;

				amountType = AmountType.Experience;
				maxDurability = PetExtensions.GetTotalXpNeededToLevelUp(slotObject.amount);
				durability = 0;
				return true;
			}
			
			return false;
		}
		
		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return UserInterfaceUtility.IsUsingMouseAndKeyboard ? HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR : HoverWindowAlignment.BOTTOM_RIGHT_OF_SCREEN;
		}

		protected override ContainedObjectsBuffer GetSlotObject() {
			return HasBeenDiscovered ? _icon.ContainedObject : default;
		}
		
		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}

		public void UpdateVisuals() {
			background.sprite = rarityBorders[0];
			if (favoritedBorder != null)
				favoritedBorder.gameObject.SetActive(IsFavorited);

			var visualObject = Icon.VisualObject;
			RenderAmountNumberRange(Icon.Amount);

			icon.transform.localScale = Vector3.one;
			colorReplacer.UpdateColorReplacerFromObjectData(visualObject);
			Manager.ui.ApplyAnyIconGradientMap(visualObject, icon);
			
			SetMissingIcon(false);
			
			if (visualObject.objectID == ObjectID.None) {
				SetEmptyIcon();
				return;
			}

			if (!PugDatabase.TryGetObjectInfo(visualObject.objectID, out var objectInfo, visualObject.variation)) {
				SetMissingIcon(true);
				return;
			}
			
			var rarityIndex = (int) objectInfo.rarity;
			if (rarityIndex >= 0 && rarityIndex < rarityBorders.Length)
				background.sprite = rarityBorders[rarityIndex];

			var iconToUse = ObjectUtility.GetIcon(visualObject.objectData, preferSmallIcons);
			if (iconToUse == null || !HasBeenDiscovered) {
				SetMissingIcon(true);
				return;
			}

			icon.sprite = iconToUse;
			UserInterfaceUtility.ApplyObjectIconTransform(icon, objectInfo, 1f);

			colorReplacer.UpdateColorReplacerFromObjectData(visualObject);
			Manager.ui.ApplyAnyIconGradientMap(visualObject, icon);
		}

		private void SetMissingIcon(bool isVisible) {
			if (missingIcon != null)
				missingIcon.gameObject.SetActive(isVisible);
			icon.gameObject.SetActive(!isVisible);
		}
		
		private bool RenderAmountNumberRange((int Min, int Max) amount) {
			if (amountNumber == null)
				return false;
			
			if ((amount.Max > 1 || alwaysShowAmount) && !AmountIsShownAsBar()) {
				var text = UserInterfaceUtility.FormatRange(amount);
				
				amountNumber.gameObject.SetActive(true);
				amountNumber.Render(text);
				if (amountNumberShadow != null) {
					amountNumberShadow.gameObject.SetActive(true);
					amountNumberShadow.Render(text);
				}

				return true;
			}
			
			amountNumber.gameObject.SetActive(false);
			if (amountNumberShadow != null)
				amountNumberShadow.gameObject.SetActive(false);
			
			return false;
		}

		private static (int Amount, string Hint) GetAmountToPickUp(ObjectDataCD objectData) {
			const string giveStackHint = "ItemBrowser-ButtonHints/GiveStack";
			const string giveTenHint = "ItemBrowser-ButtonHints/GiveTen";
			const string giveOneHint = "ItemBrowser-ButtonHints/GiveOne";
			
			var isStackable = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation) is {
				isStackable: true
			};

			if (isStackable && InputHelper.IsPickUpStackHeld)
				return (Constants.inventoryMaxAmountPerSlot, giveStackHint);

			if (isStackable && InputHelper.IsPickUpTenHeld)
				return (10, giveTenHint);

			return (1, giveOneHint);
		}
	}
}