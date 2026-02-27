using PugMod;
using PugTilemap;

namespace ItemBrowser.Utilities {
	public static class TileUtils {
		public static bool IsBlock(TileType tileType, Tileset tileset, out ObjectID wallObjectId, out ObjectID groundObjectId) {
			wallObjectId = ObjectID.None;
			groundObjectId = ObjectID.None;
			
			if (tileType != TileType.wall && tileType != TileType.ground)
				return false;
			
			var wallObjectInfo = PugDatabase.TryGetTileItemInfo(TileType.wall, (int) tileset);
			var groundObjectInfo = PugDatabase.TryGetTileItemInfo(TileType.ground, (int) tileset);
			if (wallObjectInfo == null || groundObjectInfo == null)
				return false;
			
			wallObjectId = wallObjectInfo.objectID;
			groundObjectId = groundObjectInfo.objectID;
			return true;
		}
		
		public static bool IsBlock(TileType tileType, Tileset tileset) {
			return IsBlock(tileType, tileset, out _, out _);
		}

		public static bool TryGetAssociatedObject(TileType tileType, Tileset tileset, out ObjectDataCD objectData) {
			objectData = default;
			
			if (IsBlock(tileType, tileset, out var wallObject, out _)) {
				objectData = new ObjectDataCD {
					objectID = wallObject
				};
			} else {
				var associatedObjectInfo = PugDatabase.TryGetTileItemInfo(tileType, (int) tileset);
				if (associatedObjectInfo == null)
					return false;
				
				objectData = new ObjectDataCD {
					objectID = associatedObjectInfo.objectID,
					variation = associatedObjectInfo.variation
				};
			}
		
			return true;
		}
		
		public static string GetLocalizedDisplayName(TileType tileType, Tileset? tileset) {
			if (tileset == null)
				return API.Localization.GetLocalizedTerm($"ItemBrowser-AnyTileType/{tileType}");

			if (IsBlock(tileType, tileset.Value, out var wallObjectId, out _)) {
				return string.Format(
					API.Localization.GetLocalizedTerm(tileType == TileType.wall ? "ItemBrowser-General/WallBlockSuffix" : "ItemBrowser-General/GroundBlockSuffix"),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(wallObjectId)
				);
			}
			
			var objectInfo = PugDatabase.TryGetTileItemInfo(tileType, (int) tileset);
			if (objectInfo != null)
				return ObjectUtils.GetLocalizedDisplayNameOrDefault(objectInfo.objectID, objectInfo.variation);

			return $"{tileType}/{tileset}";
		}
	}
}