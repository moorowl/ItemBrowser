using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class GroupResults {
		private readonly Dictionary<ObjectDataCD, int> _groupIndexes;
		private readonly Dictionary<int, string> _groupNames;
		
		private GroupResults(Dictionary<ObjectDataCD, int> groupIndexes, Dictionary<int, string> groupNames) {
			_groupIndexes = groupIndexes;
			_groupNames = groupNames;
		}

		public int GetGroup(ObjectDataCD objectData) {
			if (objectData.variation > 0 && !ObjectUtility.IsPrimaryVariation(objectData))
				return _groupIndexes.GetValueOrDefault(objectData, _groupIndexes.GetValueOrDefault(new ObjectDataCD { objectID = objectData.objectID }, -1));
			
			return _groupIndexes.GetValueOrDefault(objectData, -1);
		}

		public string GetGroupName(int index) {
			return _groupNames.GetValueOrDefault(index);
		}

		public static GroupResults Create(Group group, HashSet<ObjectDataCD> objectsToGroup) {
			var groupIndexes = new Dictionary<ObjectDataCD, int>();
			var groupNames = new Dictionary<int, string>();
			
			for (var i = 0; i < group.Names.Length; i++)
				groupNames[i] = group.Names[i];

			foreach (var objectData in objectsToGroup)
				groupIndexes.TryAdd(objectData, group.Function(objectData));

			return new GroupResults(groupIndexes, groupNames);
		}
	}
}