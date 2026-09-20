using System.Collections.Generic;
using System.Linq;
using Inventory;
using ItemBrowser.Common.Api;
using ItemBrowser.Common.Options;
using ItemBrowser.Utilities;
using PugMod;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ItemBrowser.Common.UserInterface.Browser {
	public class EntryDescriptionButton : ItemBrowserButton {
		private readonly List<TextAndFormatFields> _lines = new();
		private float _showDescriptionUntil;
		private readonly List<PugDatabase.MaterialInfo> _materials = new();
		
		public int LineCount => _lines.Count;

		public void AddLine(TextAndFormatFields line) {
			_lines.Add(line);
		}
		
		public void AddMaterials(params ObjectWithAmount[] materials) {
			_materials.AddRange(MaterialInfoUtility.GetMaterialInfos(materials));
		}

		public void AddMaterialsForRecipe(ObjectID objectID) {
			_materials.AddRange(MaterialInfoUtility.GetMaterialInfosForRecipe(new ObjectDataCD { objectID = objectID }));
		}

		public void AddPadding(float amount = UserInterfaceUtility.DescriptionPadding) {
			if (_lines.Count == 0)
				return;
			
			_lines[^1].paddingBeneath += amount;
		}
		
		public void Clear() {
			_lines.Clear();
			_materials.Clear();
		}

		public override TextAndFormatFields GetHoverTitle() {
			return _lines.Count == 0 ? null : _lines[0];
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields>();
			
			if (!CanShowDescription(out var temporaryTimeRemaining)) {
				TryShowButtonHint(ButtonHint.DiscoverTemporarily);
				return lines;
			}
			
			lines = _lines.Skip(1).ToList();

			if (temporaryTimeRemaining > 0f) {
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

			return lines;
		}

		public override List<PugDatabase.MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing) {
			return CanShowDescription(out _) ? _materials : base.GetRequiredMaterials(isRepairing, isReinforcing);
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!CanShowDescription(out _))
				ShowDescriptionTemporarily();
		}

		public override void OnRightClicked(bool mod1, bool mod2) {
			base.OnRightClicked(mod1, mod2);

			if (!CanShowDescription(out _))
				ShowDescriptionTemporarily();
		}

		protected override void OnEnable() {
			base.OnEnable();

			_showDescriptionUntil = 0f;
		}

		private void ShowDescriptionTemporarily() {
			_showDescriptionUntil = Time.time + 15f;
		}

		private bool CanShowDescription(out float temporaryTimeRemaining) {
			temporaryTimeRemaining = 0f;

			if (!OptionsManager.Instance.DiscoveryMode)
				return true;
			
			temporaryTimeRemaining = Mathf.Max(_showDescriptionUntil - Time.time, 0f);
			return temporaryTimeRemaining > 0f;
		}
		
		private static void GetNearestMatchingChest(List<Entity> allNearbyChests, Entity chestInMaterialInfo, out Entity nearestChest, out Sprite nearestChestIcon) {
			nearestChest = Entity.Null;
			nearestChestIcon = null;

			foreach (var nearbyChest in allNearbyChests) {
				if (nearbyChest != chestInMaterialInfo)
					continue;

				nearestChest = nearbyChest;
				break;
			}
			
			if (EntityUtility.TryGetComponentData<ObjectDataCD>(nearestChest, API.Client.World, out var nearestChestObjectData))
				nearestChestIcon = PugDatabase.GetObjectInfo(nearestChestObjectData.objectID, nearestChestObjectData.variation)?.smallIcon;
		}
	}
}