using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Api.Entries.Requirements.Types {
	public class BossDefeated : ObjectEntryRequirement {
		public readonly ObjectID Boss;
			
		public BossDefeated(ObjectID boss) {
			Boss = boss;
		}

		public override bool IsFulfilled() {
			var worldInfo = ClientWorldStateSystem.WorldInfo;

			return Boss switch {
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
				API.Localization.GetLocalizedTerm("ItemBrowser-ObjectEntryRules/BossDefeated"),
				ObjectUtils.GetLocalizedDisplayNameOrDefault(Boss)
			);
		}
	}
}