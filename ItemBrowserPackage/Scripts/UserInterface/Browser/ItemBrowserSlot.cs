using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using PlayerEquipment;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ItemBrowserSlot : SlotUIBase, IScrollItem {
		private static DisplayedObject EmptyDisplayedObject => new DisplayedObject.Basic(new ObjectDataCD());
		
		public ColorReplacer colorReplacer;
		public Sprite[] rarityBorders;
		public GameObject favoritedBorder;
		public SpriteRenderer missingIcon;
		public bool preferSmallIcons;
		public bool linksSource;
		public bool linksUsage;
		public bool showAmountInTitle;

		private DisplayedObject _displayedObject = EmptyDisplayedObject;
		public DisplayedObject DisplayedObject {
			get => _displayedObject;
			set {
				_displayedObject = value ?? EmptyDisplayedObject;
				UpdateVisuals();
			}
		}
		
		public bool IsSelected => hoverBorder.gameObject.activeSelf;
		public bool IsFavorited => Options.Instance.IsFavorited(FavoritedKey);
		public bool CanBeFavorited => FavoritedKey.objectID != ObjectID.None;
		private ObjectDataCD FavoritedKey => new() {
			objectID = DisplayedObject.ContainedObject.objectID,
			variation = DisplayedObject.ContainedObject.variation
		};
		public bool IsDiscoveredTemporarily => Time.time <= _temporaryShowUndiscoveredObjectUntil;
		public bool IsDiscovered => !Options.Instance.DiscoveryMode
		                            || DisplayedObject.ContainedObject.objectID == ObjectID.None
		                            || IsDiscoveredTemporarily
		                            || ObjectUtils.HasBeenDiscovered(DisplayedObject.ContainedObject.objectData, true);
		
		private float _height;
		private UIScrollWindow _scrollWindow;
		private float _temporaryShowUndiscoveredObjectUntil;
		private bool _wasDiscovered;
		
		public override float localScrollPosition => _scrollWindow != null ? -Mathf.Abs(_scrollWindow.scrollingContent.position.y - transform.position.y) + _scrollWindow.scrollingContent.GetChild(0).localPosition.y : 0f;
		private bool ShowHoverWindow => _scrollWindow == null || _scrollWindow.IsShowingPosition(localScrollPosition);
		public override bool isVisibleOnScreen => ShowHoverWindow && base.isVisibleOnScreen;
		public override UIScrollWindow uiScrollWindow => _scrollWindow;
		
		public static bool CanCheatInObjects => Options.Instance.CheatMode && ClientWorldStateSystem.IsAdminOrInCreative;
		
		protected override void Awake() {
			base.Awake();
			
			_scrollWindow = GetComponentInParent<UIScrollWindow>();
			
			var boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null)
				_height = boxCollider.size.y;

			icon.material = UserInterfaceUtils.GetUISpriteColorReplaceMaterial();
			UpdateVisuals();
		}

		public void OnScrollWindowChanged(UIScrollWindow scrollWindow) {
			_scrollWindow = scrollWindow;
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			_displayedObject.Update(this);
			
			if (favoritedBorder != null) {
				UpdateFavoriting();
				favoritedBorder.gameObject.SetActive(IsFavorited);
			}

			var isDiscovered = IsDiscovered;
			if (isDiscovered != _wasDiscovered) {
				UpdateVisuals();
				_wasDiscovered = isDiscovered;
			}

			if (IsSelected)
				UpdateCheatObjectIn();
		}

		private void UpdateFavoriting() {
			var input = Manager.input.singleplayerInputModule;
			
			if (IsSelected && CanBeFavorited && InputHelper.IsToggleFavoritePressed) {
				if (IsFavorited) {
					UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.Unfavorite, this);
					Options.Instance.RemoveFavorite(FavoritedKey);
				} else {
					UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.Favorite, this);
					Options.Instance.AddFavorite(FavoritedKey);
				}

				OnFavoritedStateChanged();
			}
		}

		private void UpdateCheatObjectIn() {
			var containedObjectData = _displayedObject.ContainedObject.objectData;
			if (containedObjectData.objectID == ObjectID.None || !InputHelper.IsSpawnItemPressed || !CanCheatInObjects)
				return;
			
			var player = Manager.main.player;
			player.playerCommandSystem.CreateAndDropEntity(containedObjectData.objectID, player.WorldPosition, CanPickUpStack(containedObjectData) ? Constants.inventoryMaxAmountPerSlot : 1, player.entity, containedObjectData.variation);
			UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.AddObjectToInventory, this);
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

			if (!IsDiscovered && !IsDiscoveredTemporarily) {
				SetDiscoveredTemporarily();
				return;
			}

			if (linksSource && !(_displayedObject.ShowDetails(this, DetailsTab.Sources) || (!UserInterfaceUtils.IsUsingMouseAndKeyboard && _displayedObject.ShowDetails(this, DetailsTab.Usages))))
				UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.NoSourcesOrUsages, this);
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);

			if (!IsDiscovered && !IsDiscoveredTemporarily) {
				SetDiscoveredTemporarily();
				return;
			}

			if (linksUsage && !_displayedObject.ShowDetails(this, DetailsTab.Usages))
				UserInterfaceUtils.PlaySound(UserInterfaceUtils.MenuSound.NoSourcesOrUsages, this);
		}

		private void SetDiscoveredTemporarily() {
			_temporaryShowUndiscoveredObjectUntil = Time.time + 10f;
		}

		public override TextAndFormatFields GetHoverTitle() {
			if (!IsDiscovered) {
				return new TextAndFormatFields {
					text = "ItemBrowser-General/Undiscovered"
				};
			}
			
			var title = _displayedObject.GetHoverTitle(this);
			var visualObject = _displayedObject.VisualObject;
			var amount = _displayedObject.Amount;
			var isDiscovered = IsDiscovered;

			if (!IsDiscovered)
				title.text = API.Localization.GetLocalizedTerm("ItemBrowser-General/Undiscovered");
			
			if (showAmountInTitle && visualObject.objectID != ObjectID.None && amount.Max > 1) {
				return new TextAndFormatFields {
					text = "ItemBrowser-General/NameAndAmountFormat",
					formatFields = new[] {
						title.dontLocalize ? title.text : API.Localization.GetLocalizedTerm(title.text) ?? title.text,
						amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString()
					},
					dontLocalizeFormatFields = true,
					color = isDiscovered ? Manager.text.GetRarityColor(PugDatabase.GetObjectInfo(visualObject.objectID).rarity) : Color.white
				};
			}

			return title;
		}
		
		public override List<TextAndFormatFields> GetHoverDescription() {
			if (!IsDiscovered)
				return new List<TextAndFormatFields>();
			
			var lines = _displayedObject.GetHoverDescription(this) ?? new List<TextAndFormatFields>();
			
			var containedObjectData = _displayedObject.ContainedObject.objectData;
			if (containedObjectData.objectID != ObjectID.None) {
				if (IsFavorited) {
					lines.Add(new TextAndFormatFields {
						text = "ItemBrowser-General/Favorited",
						color = Color.yellow
					});	
				}

				var hasSources = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Source, containedObjectData).Any();
				var hasUsages = ItemBrowserAPI.ObjectEntryRegistry.GetAllEntries(ObjectEntryType.Usage, containedObjectData).Any();
				
				if (linksSource && hasSources)
					UserInterfaceUtils.AppendButtonHint(lines, "ItemBrowser-ButtonHints/ViewSource", "UIInteract");

				if (linksUsage && hasUsages) {
					if (!hasSources && !UserInterfaceUtils.IsUsingMouseAndKeyboard)
						UserInterfaceUtils.AppendButtonHint(lines, "ItemBrowser-ButtonHints/ViewUsage", "UIInteract");

					UserInterfaceUtils.AppendButtonHint(lines, "ItemBrowser-ButtonHints/ViewUsage", "UISecondInteract");
				}

				if (CanCheatInObjects)
					UserInterfaceUtils.AppendButtonHint(lines, CanPickUpStack(containedObjectData) ? "ItemBrowser-ButtonHints/GiveStack" : "ItemBrowser-ButtonHints/GiveOne", "ControlMapper/ItemBrowser-SpawnItem");
			}
			
			return lines;
		}
		
		public override List<TextAndFormatFields> GetHoverStats(bool previewReinforced) {
			return !IsDiscovered ? new List<TextAndFormatFields>() : _displayedObject.GetHoverStats(this, previewReinforced);
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

		public override HoverTitleIconType GetHoverTitleIconType() {
			return HoverTitleIconType.None;
		}

		public override HoverWindowAlignment GetHoverWindowAlignment() {
			return UserInterfaceUtils.IsUsingMouseAndKeyboard ? HoverWindowAlignment.BOTTOM_RIGHT_OF_CURSOR : HoverWindowAlignment.BOTTOM_RIGHT_OF_SCREEN;
		}

		protected override ContainedObjectsBuffer GetSlotObject() {
			return IsDiscovered ? _displayedObject.ContainedObject : default;
		}
		
		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}

		public void UpdateVisuals() {
			background.sprite = rarityBorders[0];
			if (favoritedBorder != null)
				favoritedBorder.gameObject.SetActive(IsFavorited);

			var visualObject = DisplayedObject.VisualObject;
			RenderAmountNumberRange(DisplayedObject.Amount);

			icon.transform.localScale = Vector3.one;
			colorReplacer.UpdateColorReplacerFromObjectData(visualObject);
			Manager.ui.ApplyAnyIconGradientMap(visualObject, icon);
			
			SetMissingIcon(false);
			
			if (visualObject.objectID == ObjectID.None) {
				SetEmptyIcon();
				return;
			}

			if (!PugDatabase.TryGetObjectInfo(visualObject.objectID, out var objectInfo, visualObject.variation) || !IsDiscovered) {
				SetMissingIcon(true);
				return;
			}

			var iconToUse = ObjectUtils.GetIcon(visualObject.objectData, preferSmallIcons);
			if (iconToUse == null) {
				SetMissingIcon(true);
				return;
			}

			icon.sprite = iconToUse;
			icon.transform.localPosition = objectInfo.iconOffset;
			
			var spriteSize = icon.sprite.bounds.size;
			if (spriteSize is { x: > 1f, y: > 1f } && IsCarriedObject(objectInfo)) {
				spriteSize.x = 1f;
				spriteSize.y = 1f;
			}
			var scale = Mathf.Min(1f / spriteSize.x, 1f / spriteSize.y);
			icon.transform.localScale = new Vector3(scale, scale, 1f);
			
			colorReplacer.UpdateColorReplacerFromObjectData(visualObject);
			Manager.ui.ApplyAnyIconGradientMap(visualObject, icon);
			icon.transform.localPosition = objectInfo.iconOffset;

			var rarityIndex = (int) objectInfo.rarity;
			if (rarityIndex >= 0 && rarityIndex < rarityBorders.Length)
				background.sprite = rarityBorders[rarityIndex];
		}

		private void SetMissingIcon(bool isVisible) {
			if (missingIcon != null)
				missingIcon.gameObject.SetActive(isVisible);
			icon.gameObject.SetActive(!isVisible);
		}
		
		private bool RenderAmountNumberRange((int Min, int Max) amount) {
			if (amountNumber == null)
				return false;
			
			if (amount.Max > 1 && !AmountIsShownAsBar()) {
				var text = UserInterfaceUtils.FormatRange(amount);
				
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

		private static readonly HashSet<EquipmentSlotType> CarriedEquipmentSlotTypes = new() {
			EquipmentSlotType.MeleeWeaponSlot,
			EquipmentSlotType.RangeWeaponSlot,
			EquipmentSlotType.ShovelSlot,
			EquipmentSlotType.HoeSlot,
			EquipmentSlotType.BugNet,
			EquipmentSlotType.SeederSlot,
			EquipmentSlotType.Shield,
			EquipmentSlotType.InstrumentSlot,
			EquipmentSlotType.FishingRodSlot
		};

		public static bool IsCarriedObject(ObjectInfo objectInfo) {
			var objectType = objectInfo.objectType;
			var equipmentSlotType = PlayerController.GetEquippedSlotTypeForObjectType(objectType, default, default, default, default);

			return objectType != ObjectType.ThrowingWeapon && CarriedEquipmentSlotTypes.Contains(equipmentSlotType);
		}
		
		private static bool CanPickUpStack(ObjectDataCD objectData) {
			return InputHelper.IsPickUpTenHeld && PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation) is {
				isStackable: true
			};
		}
	}
}