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
			var querySystem = API.Client.World.GetExistingSystemManaged<PugQuerySystem>();
			var nearbyChests = ClientWorldStateSystem.NearbyChests;

			using var cookingIngredientsRequired = new NativeList<ObjectWithAmount>(Allocator.Temp);
			using var inventories = new NativeList<Entity>(Allocator.Temp);
			inventories.Add(Manager.main.player.entity);
			
			foreach (var nearbyChest in nearbyChests)
				inventories.Add(nearbyChest);
			
			using var objectsRequired = new NativeList<ObjectWithAmount>(1, Allocator.Temp);
			foreach (var material in materials)
				objectsRequired.Add(material);
			
			using var materialInfos = InventoryUtility.GetMaterialInfos(
				querySystem.GetBufferLookup<ContainedObjectsBuffer>(),
				querySystem.GetBufferLookup<InventoryBuffer>(),
				ClientWorldStateSystem.PugDatabaseBank,
				objectsRequired,
				1f,
				inventories,
				1,
				Allocator.Temp
			);

			for (var i = 0; i < materialInfos.Length; i++) {
				GetNearestMatchingChest(nearbyChests, materialInfos[i].nearbyChestWithMaterial, out var nearestChest, out var nearestChestIcon);

				_materials.Add(new PugDatabase.MaterialInfo(
					materialInfos[i].objectID,
					materialInfos[i].amountNeeded,
					materialInfos[i].amountAvailable,
					nearestChest,
					nearestChestIcon
				));
			}
		}

		public void AddMaterialsFor(ObjectID objectID) {
			var objectInfo = PugDatabase.GetObjectInfo(objectID);

			if (objectInfo != null) {
				var querySystem = API.Client.World.GetExistingSystemManaged<PugQuerySystem>();
				var nearbyChests = ClientWorldStateSystem.NearbyChests;

				using var cookingIngredientsRequired = new NativeList<ObjectWithAmount>(Allocator.Temp);
				using var inventories = new NativeList<Entity>(Allocator.Temp);
				inventories.Add(Manager.main.player.entity);
				
				foreach (var nearbyChest in nearbyChests)
					inventories.Add(nearbyChest);
				
				using var materialInfos = InventoryUtility.GetCraftingMaterialInfosForRecipe(
					ClientWorldStateSystem.PugDatabaseBank,
					querySystem.GetBufferLookup<ContainedObjectsBuffer>(),
					querySystem.GetBufferLookup<InventoryBuffer>(),
					querySystem.GetComponentLookup<AnvilCD>(),
					querySystem.GetComponentLookup<ObjectDataCD>(),
					querySystem.GetBufferLookup<SummarizedConditionsBuffer>(),
					querySystem.GetComponentLookup<DurabilityCD>(),
					querySystem.GetComponentLookup<PrioritizedRepairMaterialCD>(),
					querySystem.GetComponentLookup<LevelCD>(),
					new ObjectWithAmount { objectID = objectID, amount = 1 },
					cookingIngredientsRequired,
					inventories,
					1,
					false,
					false,
					Manager.main.player.entity,
					Manager.main.player.entity,
					Allocator.Temp
				);

				for (var i = 0; i < materialInfos.Length; i++) {
					GetNearestMatchingChest(nearbyChests, materialInfos[i].nearbyChestWithMaterial, out var nearestChest, out var nearestChestIcon);

					_materials.Add(new PugDatabase.MaterialInfo(
						materialInfos[i].objectID,
						materialInfos[i].amountNeeded,
						materialInfos[i].amountAvailable,
						nearestChest,
						nearestChestIcon
					));
				}
			}
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
			return _materials;
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