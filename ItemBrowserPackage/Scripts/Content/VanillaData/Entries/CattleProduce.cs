using System;
using System.Collections.Generic;
using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Content.VanillaData.Entries {
	public record CattleProduce : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser-ObjectEntryNames/CattleProduce", ObjectID.Egg, VanillaPriorities.CattleProduce);
		
		public ObjectID Result { get; set; }
		public ObjectID Cattle { get; set; }
		public int Amount { get; set; }
		public List<ObjectCategoryTag> SuitableFeed { get; set; }
		public int SuitableFeedRequired { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, _) in allObjects) {
					if (!PugDatabase.TryGetComponent<CraftingCD>(objectData, out var craftingCD) || !PugDatabase.HasComponent<CanCraftObjectsBuffer>(objectData))
						continue;
					
					if (craftingCD.craftingType != CraftingType.Cattle || !PugDatabase.TryGetComponent<BehaviourTagsCD>(objectData, out var behaviorTagsCD))
						continue;

					var eatsTags = GetTagsFromBitMask(behaviorTagsCD.eatsTagsBitMask);
					var canCraftObjects = PugDatabase.GetBuffer<CanCraftObjectsBuffer>(objectData);
					
					foreach (var canCraftObject in canCraftObjects) {
						var objectInfo = PugDatabase.GetObjectInfo(canCraftObject.objectID);
						if (objectInfo == null || canCraftObject.objectID == ObjectID.None || canCraftObject.entityAmountToConsume == 0)
							continue;

						var entry = new CattleProduce {
							Result = canCraftObject.objectID,
							Cattle = objectData.objectID,
							Amount = Math.Max(canCraftObject.amount, 1),
							SuitableFeed = eatsTags,
							SuitableFeedRequired = canCraftObject.entityAmountToConsume
						};
						registry.Register(ObjectEntryType.Source, canCraftObject.objectID, 0, entry);
						registry.Register(ObjectEntryType.Usage, objectData.objectID, 0, entry);
						foreach (var eatsTag in eatsTags) {
							foreach (var ingredient in ObjectUtils.GetAllObjectsWithTag(eatsTag)) {
								registry.Register(ObjectEntryType.Usage, ingredient.objectID, ingredient.variation, entry);
							}
						}
					}
				}
			}

			private static List<ObjectCategoryTag> GetTagsFromBitMask(ulong bitMask) {
				var tags = new List<ObjectCategoryTag>();
				for (var i = 1; i < 64; i++) {
					if ((bitMask & (1uL << i)) != 0)
						tags.Add((ObjectCategoryTag) i);
				}

				return tags;
			}
		}
	}
}