using ItemBrowser.Common.Api;
using ItemBrowser.Common.Api.Entries;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Content.VanillaData.Entries.Requirements {
	public class EnemyDefeatedRequirement : ObjectEntryRequirement {
		public readonly ObjectID Enemy;
			
		public EnemyDefeatedRequirement(ObjectID enemy) {
			Enemy = enemy;
		}

		public override bool IsFulfilled() {
			var worldInfo = ClientWorldStateSystem.WorldInfo;

			return Enemy switch {
				ObjectID.BirdBoss => worldInfo.birdBossBeenKilled,
				ObjectID.OctopusBoss => worldInfo.octopusBossHasBeenKilled,
				ObjectID.ScarabBoss => worldInfo.scarabHasBeenKilled,
				ObjectID.HydraBossNature => worldInfo.hydraBossNatureHasBeenKilled,
				ObjectID.HydraBossSea => worldInfo.hydraBossSeaHasBeenKilled,
				ObjectID.HydraBossDesert => worldInfo.hydraBossDesertHasBeenKilled,
				ObjectID.CoreBoss => worldInfo.coreBossHasBeenKilled,
				ObjectID.WallBoss => worldInfo.wallBossHasBeenKilled,
				ObjectID.GiantCicadaBoss => worldInfo.giantCicadaBossHasBeenKilled,
				ObjectID.RobotBoss => worldInfo.robotBossHasBeenKilled,
				_ => false
			};
		}

		public override string GetLocalizedDescription() {
			return string.Format(
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRequirements/EnemyDefeated"),
				ObjectUtility.GetLocalizedDisplayNameOrDefault(Enemy)
			);
		}
	}
}