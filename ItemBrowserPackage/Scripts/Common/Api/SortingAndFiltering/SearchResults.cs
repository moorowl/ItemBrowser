using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugMod;

namespace ItemBrowser.Common.Api.SortingAndFiltering {
	public class SearchResults {
		private static readonly List<string> CharactersToStrip = new() {
			"\'", " ", "-", "."
		};
		
		private readonly HashSet<ObjectDataCD> _matches;

		private SearchResults(HashSet<ObjectDataCD> matches) {
			_matches = matches;
		}

		public bool Matches(ObjectDataCD objectData) {
			return _matches.Contains(objectData);
		}
		
		private static string StripUnimportantCharacters(string term) {
			term = term.ToLower();

			foreach (var characterToStrip in CharactersToStrip)
				term = term.Replace(characterToStrip, "");

			return term;
		}

		public static SearchResults Create(string term) {
			term = StripUnimportantCharacters(term);
			
			var matches = new HashSet<ObjectDataCD>();
			var termsToMatch = new List<string>(8);

			foreach (var objectData in ObjectUtility.GetAllObjects()) {
				var displayName = ObjectUtility.GetLocalizedDisplayName(objectData);
				var displayNameNote = ObjectUtility.GetUnlocalizedDisplayNameNote(objectData);
				if (displayNameNote != null)
					displayNameNote = API.Localization.GetLocalizedTerm(displayNameNote);
				
				termsToMatch.Clear();
				if (displayName != null)
					termsToMatch.Add(displayName);
				if (displayNameNote != null)
					termsToMatch.Add(displayNameNote);
				termsToMatch.Add(ObjectUtility.GetInternalName(objectData));
				termsToMatch.Add(((int) objectData.objectID).ToString());

				foreach (var termToMatch in termsToMatch) {
					if (StripUnimportantCharacters(termToMatch).Contains(term)) {
						matches.Add(objectData);
						break;
					}
				}
			}

			return new SearchResults(matches);
		}
	}
}